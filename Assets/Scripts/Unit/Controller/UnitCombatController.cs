using UnityEngine;

[RequireComponent(typeof(UnitTargetingController))]
[RequireComponent(typeof(UnitMovementController))]
[DisallowMultipleComponent]
public class UnitCombatController : MonoBehaviour
{
    private UnitBase _unit;
    private UnitTargetingController _targeting;
    private UnitMovementController _move;

    [Header("외부 로직(선택)")]
    public BaseAttack attackLogic;
    public BaseSkill skillLogic;

    [Header("문서값")]
    [Range(0.5f, 1.0f)][SerializeField] private float stopAtRangePercent = 0.9f;
    [SerializeField] private float retargetEvery = 0.20f;
    [Range(1.0f, 2.0f)][SerializeField] private float releaseRangeMultiplier = 1.0f;
    [SerializeField] private bool resetManaOnSkill = true;
    [SerializeField] private bool keepMarchingWhenNoTarget = true;

    private bool _running;
    private float _attackTimer;
    private float _retargetTimer;
    private Vector3 _forwardDir = Vector3.right;
    private bool _hadTargetPrev;

    private void Awake()
    {
        _unit = GetComponent<UnitBase>();
        _targeting = GetComponent<UnitTargetingController>();
        _move = GetComponent<UnitMovementController>();
    }

    public void InitForBattle(Vector3 forwardDir)
    {
        _forwardDir = forwardDir.sqrMagnitude > 0.0f ? forwardDir.normalized : Vector3.right;
        _attackTimer = 0.0f;
        _retargetTimer = 0.0f;
        _running = true;
        _hadTargetPrev = false;

        _move.StartMoveInDirection(_forwardDir, _unit.MoveSpeed);
    }

    public void StopBattle()
    {
        _running = false;
        attackLogic?.StopAttack();
        _move?.StopMove();
    }

    private void Update()
    {
        if (!_running || _unit == null) return;

        // 상시 마나 회복
        _unit.AddMana(_unit.ManaRecoveryPerSecond * Time.deltaTime);

        EnsureTargetWithinMyRule();

        var target = _targeting.TargetedEnemy;
        bool hasTarget = target != null && target.activeSelf;

        if (hasTarget && !_hadTargetPrev)
        {
            _move.StopMove();
            attackLogic?.StopAttack();
        }
        _hadTargetPrev = hasTarget;

        if (!hasTarget)
        {
            attackLogic?.StopAttack();
            if (keepMarchingWhenNoTarget)
                _move.StartMoveInDirection(_forwardDir, _unit.MoveSpeed);
            return;
        }

        float stopDist = _unit.AttackRange * stopAtRangePercent;
        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist <= stopDist)
        {
            _move.StopMove();
            RunAttackFlow(target);
        }
        else
        {
            attackLogic?.StopAttack();
            _move.MoveTo(target.transform.position, _unit.MoveSpeed);
        }
    }

    private void RunAttackFlow(GameObject target)
    {
        bool canUseSkill = skillLogic != null &&
                           !skillLogic.IsCasting &&
                           _unit.CurrentMana >= skillLogic.manaCost;

        if (canUseSkill)
        {
            skillLogic.StartCast(_unit, target);
            if (resetManaOnSkill) _unit.AddMana(-_unit.CurrentMana);
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _unit.AttackFrequency)
        {
            _attackTimer = 0.0f;
            if (attackLogic != null)
            {
                attackLogic.StartAttack(_unit, target);
                _unit.AddMana(_unit.ManaRecoveryOnBasicAttack);
            }
        }
    }

    private void EnsureTargetWithinMyRule()
    {
        var cur = _targeting.TargetedEnemy;

        if (cur != null)
        {
            if (!cur.activeSelf)
            {
                _targeting.SetTarget(null);
                cur = null;
            }
            else
            {
                float d = Vector3.Distance(transform.position, cur.transform.position);
                float releaseDist = _unit.EnemySearchRange * releaseRangeMultiplier;
                if (d > releaseDist)
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
}
