using UnityEngine;

[RequireComponent(typeof(UnitTargetingController))]
[RequireComponent(typeof(UnitMovementController))]
[DisallowMultipleComponent]
public class UnitCombatController : MonoBehaviour
{
    // �ܺ� ������Ʈ
    private UnitBase _unit;
    private UnitTargetingController _targeting;
    private UnitMovementController _move;
    private Transform _tr;

    // �ִϸ�����
    private SpriteFlipbookAnimatorSO _anim;

    // �ܺ� ����
    [Header("�ܺ� ����")]
    public BaseAttack attackLogic;
    public BaseSkill skillLogic;

    // ������(��ȹ��)
    [Header("������ ���")]
    [Range(0.5f, 1.0f)]
    [SerializeField] private float stopAtRangePercent = 0.9f;
    [SerializeField, Min(0.05f)] private float retargetEvery = 0.20f;
    [Range(1.0f, 2.0f)]
    [SerializeField] private float releaseRangeMultiplier = 1.0f;
    [SerializeField] private bool resetManaOnSkill = true;
    [SerializeField] private bool keepMarchingWhenNoTarget = true;

    [Header("���� �ɼ�")]
    [SerializeField] private bool blockBasicWhileCasting = true;

    [Header("�ִϸ��̼� ����(Ŭ����)")]
    [SerializeField] private string standingClip = "Standing";
    [SerializeField] private string moveClip = "Move";
    [SerializeField] private string attackClip = "Attack";
    [SerializeField] private bool flipToTargetOnAttackStart = true;
    [SerializeField] private bool hardRestartAttackAnim = true;
    [SerializeField] private bool overrideSpawnFacingWithForwardDir = false;

    // ��Ÿ�� ����
    private bool _running;
    private float _retargetTimer;
    private Vector3 _forwardDir = Vector3.right;

    private bool _hadTargetPrev;
    private bool _isMoving;
    private bool _attackTempoSet;

    private float _stopDistSqr;
    private float _releaseDistSqr;

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
        _forwardDir = (forwardDir.sqrMagnitude > 0.0f) ? forwardDir.normalized : Vector3.right;
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

        // �ʱ� ���
        if (_isMoving) _anim?.Play(moveClip);
        else _anim?.Play(standingClip);
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

        // ���� �ڿ�ȸ��
        if (_unit.ManaRecoveryPerSecond > 0.0f)
            _unit.AddMana(_unit.ManaRecoveryPerSecond * Time.deltaTime);

        // Ÿ�� ����/��Ž��
        EnsureTargetWithinMyRule();

        var tgtGO = _targeting.TargetedEnemy;
        bool hasTarget = tgtGO && tgtGO.activeSelf;

        // Ÿ�� ���� ȹ�� �� �̵� ����
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

        // ��Ÿ� ����
        var tTr = tgtGO.transform;
        float dSqr = (_tr.position - tTr.position).sqrMagnitude;

        if (dSqr <= _stopDistSqr)
        {
            StopMoveIfNeeded();

            // ���� �� ��Ÿ ����
            if (blockBasicWhileCasting && skillLogic && skillLogic.IsCasting)
            {
                attackLogic?.StopAttack();
                _unit.NotifyAttackActiveEnd();
                return;
            }

            // ��ų �켱 �õ�
            if (TryCastSkill(tgtGO)) return;

            // APS==0 �� ��Ÿ ����
            if (_unit.AttackSpeed <= 0.0f)
            {
                attackLogic?.StopAttack();
                _unit.NotifyAttackActiveEnd();
                return;
            }

            // ��Ÿ ����Ŭ ����(�ִϴ� �̺�Ʈ���� 1ȸ ���)
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
            // ��Ÿ� �� �� �߰�
            attackLogic?.StopAttack();
            _unit.NotifyAttackActiveEnd();

            _move.MoveTo(tTr.position, _unit.MoveSpeed);
            _isMoving = true;

            if (!(attackLogic && attackLogic.IsAttacking) && !(_anim && _anim.IsPlayingOnce))
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

    // �̵�/���� ����
    private void StartMoveForwardIfNeeded()
    {
        if (_isMoving) return;
        if (_anim && _anim.IsPlayingOnce) return; // ���� ���� ���̸� ���� ����
        _move.StartMoveInDirection(_forwardDir, _unit.MoveSpeed);
        _isMoving = true;

        if (!(attackLogic && attackLogic.IsAttacking) && !(_anim && _anim.IsPlayingOnce))
            _anim?.Play(moveClip);
    }

    private void StopMoveIfNeeded()
    {
        if (!_isMoving) return;
        if (_anim && _anim.IsPlayingOnce) return;
        _move.StopMove();
        _isMoving = false;

        if (!(attackLogic && attackLogic.IsAttacking))
            _anim?.Play(standingClip);
    }

    // ���� ����Ŭ �� �ִ� ����(����/�ӵ��� SO�� ��ϵ� FPS/Loop ���)
    private void HandleAttackCycleStarted(UnitBase who)
    {
        if (!_anim || !_anim.isActiveAndEnabled || !_anim.gameObject.activeInHierarchy) return;

        if (flipToTargetOnAttackStart)
        {
            var tgt = _targeting.TargetedEnemy ? _targeting.TargetedEnemy.transform : null;
            if (tgt) _anim.SetFlipX(tgt.position.x < _tr.position.x);
        }

        if (hardRestartAttackAnim) _anim.StopCurrent();

        _anim.PlayOnce(
            attackClip,
            onComplete: () =>
            {
                if (!_isMoving && !(attackLogic && attackLogic.IsAttacking))
                    _anim.Play(standingClip);
            }
        );
    }

    private void HandleAttackCycleEnded(UnitBase who)
    {
        if (_anim != null && _anim.IsPlayingOnce) return; // ���� ������ �ݹ鿡�� ���ĵ� ó��
        if (!_isMoving) _anim?.Play(standingClip);
    }
}
