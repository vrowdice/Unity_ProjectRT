using UnityEngine;

[RequireComponent(typeof(UnitTargetingController))]
[RequireComponent(typeof(UnitMovementController))]
[DisallowMultipleComponent]
public class UnitCombatController : MonoBehaviour
{
    // 외부 컴포넌트
    private UnitBase _unit;
    private UnitTargetingController _targeting;
    private UnitMovementController _move;
    private Transform _tr;

    // 애니메이터
    private SpriteFlipbookAnimatorSO _anim;

    // 외부 로직
    [Header("외부 로직")]
    public BaseAttack attackLogic;
    public BaseSkill skillLogic;

    // 데이터(기획값)
    [Header("문서값 기록")]
    [Range(0.5f, 1.0f)]
    [SerializeField] private float stopAtRangePercent = 0.9f;
    [SerializeField, Min(0.05f)] private float retargetEvery = 0.20f;
    [Range(1.0f, 2.0f)]
    [SerializeField] private float releaseRangeMultiplier = 1.0f;
    [SerializeField] private bool resetManaOnSkill = true;
    [SerializeField] private bool keepMarchingWhenNoTarget = true;

    [Header("권장 옵션")]
    [SerializeField] private bool blockBasicWhileCasting = true;

    [Header("애니메이션 설정")]
    [SerializeField] private string standingClip = "Standing";
    [SerializeField] private string moveClip = "Move";
    [SerializeField] private string attackClip = "Attack";
    [SerializeField] private bool flipToTargetOnAttackStart = true;
    [SerializeField] private bool hardRestartAttackAnim = true;
    [SerializeField] private bool overrideSpawnFacingWithForwardDir = false;

    // 런타임 상태
    private bool _running;
    private float _retargetTimer;
    private Vector3 _forwardDir = Vector3.right;

    private bool _hadTargetPrev;
    private bool _isMoving;
    private bool _attackTempoSet;

    private float _stopDistSqr;
    private float _releaseDistSqr;

    private int _minVisibleFps;
    private bool _autoLoopWhenTooFast;
    private UnitData.AttackAnimMode _attackAnimMode;

    private void Awake()
    {
        _tr = transform;
        _unit = GetComponent<UnitBase>();
        _targeting = GetComponent<UnitTargetingController>();
        _move = GetComponent<UnitMovementController>();
        if (!attackLogic) attackLogic = GetComponent<BaseAttack>();
        if (!_anim) _anim = GetComponentInChildren<SpriteFlipbookAnimatorSO>();
    }

    private void OnEnable()
    {
        BindAttackEvents(true);
    }

    private void OnDisable()
    {
        StopBattle();
        BindAttackEvents(false);
    }

    private void BindAttackEvents(bool subscribe)
    {
        if (!attackLogic) return;
        if (subscribe)
        {
            attackLogic.OnAttackCycleStarted -= HandleAttackCycleStarted;
            attackLogic.OnAttackCycleEnded -= HandleAttackCycleEnded;
            attackLogic.OnAttackCycleStarted += HandleAttackCycleStarted;
            attackLogic.OnAttackCycleEnded += HandleAttackCycleEnded;
        }
        else
        {
            attackLogic.OnAttackCycleStarted -= HandleAttackCycleStarted;
            attackLogic.OnAttackCycleEnded -= HandleAttackCycleEnded;
        }
    }

    public void InitForBattle(Vector3 forwardDir)
    {
        _forwardDir = forwardDir.sqrMagnitude > 0.0f ? forwardDir.normalized : Vector3.right;
        _retargetTimer = 0.0f;
        _running = true;
        _hadTargetPrev = false;
        _isMoving = false;
        _attackTempoSet = false;

        float stop = _unit.AttackRange * stopAtRangePercent;
        _stopDistSqr = stop * stop;

        float rel = _unit.EnemySearchRange * releaseRangeMultiplier;
        _releaseDistSqr = rel * rel;

        if (attackLogic)
        {
            attackLogic.SetTempo(_unit.AttackActiveSec, _unit.AttackRecoverySec);
            _attackTempoSet = true;
        }

        StartMoveForwardIfNeeded();

        _unit.NotifyAttackActiveEnd();

        if (overrideSpawnFacingWithForwardDir && _anim)
            _anim.SetFlipX(_forwardDir.x < 0.0f);

        if (_isMoving) _anim?.Play(moveClip);
        else _anim?.Play(standingClip);

        var stat = _unit.UnitStat;
        _attackAnimMode = stat ? stat.attackAnimMode : UnitData.AttackAnimMode.Auto;
        _minVisibleFps = stat ? stat.minVisibleFps : 12;
        _autoLoopWhenTooFast = stat ? stat.autoLoopWhenTooFast : true;

        if (stat) hardRestartAttackAnim = stat.hardRestartAttackAnim;
    }


    public void StopBattle()
    {
        _running = false;
        attackLogic?.StopAttack();
        if (_isMoving) { _move.StopMove(); _isMoving = false; }
        _unit?.NotifyAttackActiveEnd();

        if (_anim != null && !(attackLogic && attackLogic.IsAttacking))
            _anim.Play(standingClip);
    }

    private void Update()
    {
        if (!_running || !_unit) return;

        // 마나 자연회복
        if (_unit.ManaRecoveryPerSecond > 0.0f)
            _unit.AddMana(_unit.ManaRecoveryPerSecond * Time.deltaTime);

        // 타겟 유지/재탐색
        EnsureTargetWithinMyRule();

        var tgtGO = _targeting.TargetedEnemy;
        bool hasTarget = tgtGO && tgtGO.activeSelf;

        // 타겟 새 획득 시 이동 정지
        if (hasTarget && !_hadTargetPrev) StopMoveIfNeeded();
        _hadTargetPrev = hasTarget;

        if (!hasTarget)
        {
            attackLogic?.StopAttack();
            _unit.NotifyAttackActiveEnd();

            if (keepMarchingWhenNoTarget) StartMoveForwardIfNeeded();
            else StopMoveIfNeeded();
            return;
        }

        // 사거리 판정
        var tTr = tgtGO.transform;
        float dSqr = (_tr.position - tTr.position).sqrMagnitude;

        if (dSqr <= _stopDistSqr)
        {
            StopMoveIfNeeded();

            // 시전 중 평타 차단
            if (blockBasicWhileCasting && skillLogic && skillLogic.IsCasting)
            {
                attackLogic?.StopAttack();
                _unit.NotifyAttackActiveEnd();
                return;
            }

            // 스킬 우선 시도
            if (TryCastSkill(tgtGO)) return;

            // APS==0 → 평타 생략
            if (_unit.AttackSpeed <= 0.0f)
            {
                attackLogic?.StopAttack();
                _unit.NotifyAttackActiveEnd();
                return;
            }

            // 평타 사이클 시작(애니는 이벤트에서 1회 재생)
            if (attackLogic != null && !attackLogic.IsBusy && attackLogic.isActiveAndEnabled && attackLogic.gameObject.activeInHierarchy)
            {
                if (!_attackTempoSet)
                {
                    attackLogic.SetTempo(_unit.AttackActiveSec, _unit.AttackRecoverySec);
                    _attackTempoSet = true;
                }
                attackLogic.StartAttack(_unit, tgtGO);
            }
        }
        else
        {
            // 사거리 밖 → 추격
            attackLogic?.StopAttack();
            _unit.NotifyAttackActiveEnd();

            _move.MoveTo(tTr.position, _unit.MoveSpeed);
            _isMoving = true;

            // 공격 중이면 Move가 Attack을 덮지 않음
            if (!(attackLogic && attackLogic.IsAttacking))
                _anim?.Play(moveClip);
        }
    }

    private bool TryCastSkill(GameObject target)
    {
        if (!skillLogic || skillLogic.IsCasting) return false;
        if (_unit.CurrentMana < skillLogic.manaCost) return false;

        var stat = _unit.UnitStat;
        if (skillLogic is IConfigurableSkill cfg && stat != null)
            cfg.ApplyConfigFromStat(stat);

        if (stat && stat.active != null)
        {
            skillLogic.manaCost = stat.active.manaCost;
            skillLogic.skillName = string.IsNullOrEmpty(stat.active.displayName)
                ? stat.active.skillLogic
                : stat.active.displayName;
        }

        skillLogic.StartCast(_unit, target);

        if (resetManaOnSkill) _unit.AddMana(-_unit.CurrentMana);
        else _unit.UseMana(skillLogic.manaCost);

        _attackTempoSet = false;
        // (스킬 전용 애니가 있다면: _anim?.PlayOnceDuration("Skill", castTime, ...))
        return true;
    }

    private void EnsureTargetWithinMyRule()
    {
        var cur = _targeting.TargetedEnemy;

        if (cur)
        {
            if (!cur.activeSelf)
            {
                _targeting.SetTarget(null);
                cur = null;
            }
            else
            {
                float dSqr = (_tr.position - cur.transform.position).sqrMagnitude;
                if (dSqr > _releaseDistSqr)
                {
                    _targeting.SetTarget(null);
                    cur = null;
                }
            }
        }

        _retargetTimer += Time.deltaTime;
        if (cur == null || _retargetTimer >= retargetEvery)
        {
            _retargetTimer = 0.0f;
            _targeting.FindNewTarget();
        }
    }

    // 이동/애니
    private void StartMoveForwardIfNeeded()
    {
        if (_isMoving) return;
        _move.StartMoveInDirection(_forwardDir, _unit.MoveSpeed);
        _isMoving = true;

        if (!(attackLogic && attackLogic.IsAttacking))
            _anim?.Play(moveClip);
    }

    private void StopMoveIfNeeded()
    {
        if (!_isMoving) return;
        _move.StopMove();
        _isMoving = false;

        if (!(attackLogic && attackLogic.IsAttacking))
            _anim?.Play(standingClip);
    }

    private void HandleAttackCycleStarted(UnitBase who)
    {
        if (!_anim || !_anim.isActiveAndEnabled || !_anim.gameObject.activeInHierarchy) return;

        // 공격 시작 시 타깃 방향으로 뒤집기
        if (flipToTargetOnAttackStart)
        {
            var tgt = _targeting.TargetedEnemy ? _targeting.TargetedEnemy.transform : null;
            if (tgt) _anim.SetFlipX(tgt.position.x < _tr.position.x);
        }

        // 프레임/시간 계산
        int frames = (_anim != null) ? _anim.GetFrameCount(attackClip) : 0;
        float act = Mathf.Max(0.05f, _unit.AttackActiveSec);

        float minTimeShowAllAt30 = (frames > 0) ? (frames / 30.0f) : 0.0f;

        bool useLoop;
        switch (_attackAnimMode)
        {
            case UnitData.AttackAnimMode.LoopWhileAttacking:
                useLoop = true;
                break;

            case UnitData.AttackAnimMode.OneShotPerCycle:
                useLoop = false;
                break;

            default: 
                useLoop = _autoLoopWhenTooFast && (frames > 1) && (act < minTimeShowAllAt30);
                break;
        }

        if (useLoop)
        {
            if (_anim.CurrentClip != attackClip || _anim.IsPlayingOnce)
            {
                if (hardRestartAttackAnim) _anim.StopCurrent();
                _anim.Play(attackClip);
            }
            return;
        }

        float desiredFps = (frames > 0 && act > 0.0f) ? Mathf.Clamp(frames / act, _minVisibleFps, 30f) : -1.0f;

        if (_anim.CurrentClip == attackClip && _anim.IsPlayingOnce)
        {
            if (desiredFps > 0f) _anim.SetFpsForCurrent(desiredFps);
            return;
        }

        if (hardRestartAttackAnim) _anim.StopCurrent();

        _anim.PlayOnce(
            attackClip,
            onComplete: () =>
            {
                if (!_isMoving && !(attackLogic && attackLogic.IsAttacking))
                    _anim.Play(standingClip);
            },
            fpsOverride: desiredFps
        );
    }

    private void HandleAttackCycleEnded(UnitBase who)
    {
        if (_anim != null && _anim.IsPlayingOnce) return;

        if (!(attackLogic && attackLogic.IsAttacking))
        {
            if (_isMoving) _anim?.Play(moveClip);
            else _anim?.Play(standingClip);
        }
    }
}
