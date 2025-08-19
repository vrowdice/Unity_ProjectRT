using UnityEngine;

[RequireComponent(typeof(UnitTargetingController))]
[RequireComponent(typeof(UnitMovementController))]
[DisallowMultipleComponent]
public class UnitCombatController : MonoBehaviour
{
    private UnitBase _unit;
    private UnitTargetingController _targeting;
    private UnitMovementController _move;
    private Transform _tr;

    [Header("외부 로직(선택)")]
    public BaseAttack attackLogic;
    public BaseSkill skillLogic;

    [Header("문서값")]
    [Range(0.5f, 1.0f)][SerializeField] private float stopAtRangePercent = 0.9f;
    [SerializeField, Min(0.05f)] private float retargetEvery = 0.20f;
    [Range(1.0f, 2.0f)][SerializeField] private float releaseRangeMultiplier = 1.0f;
    [SerializeField] private bool resetManaOnSkill = true;
    [SerializeField] private bool keepMarchingWhenNoTarget = true;

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
    }

    public void InitForBattle(Vector3 forwardDir)
    {
        _forwardDir = forwardDir.sqrMagnitude > 0.0f ? forwardDir.normalized : Vector3.right;
        _retargetTimer = 0.0f; _running = true; _hadTargetPrev = false; _isMoving = false; _attackTempoSet = false;

        float stop = _unit.AttackRange * stopAtRangePercent; _stopDistSqr = stop * stop;
        float rel = _unit.EnemySearchRange * releaseRangeMultiplier; _releaseDistSqr = rel * rel;

        if (attackLogic) { attackLogic.SetTempo(_unit.AttackActiveSec, _unit.AttackRecoverySec); _attackTempoSet = true; }
        StartMoveForwardIfNeeded();
        _unit.NotifyAttackActiveEnd();
    }

    public void StopBattle()
    {
        _running = false;
        if (attackLogic) attackLogic.StopAttack();
        if (_isMoving) { _move.StopMove(); _isMoving = false; }
        _unit.NotifyAttackActiveEnd();
    }

    private void Update()
    {
        if (!_running || !_unit) return;

        if (_unit.ManaRecoveryPerSecond > 0.0f)
            _unit.AddMana(_unit.ManaRecoveryPerSecond * Time.deltaTime);

        EnsureTargetWithinMyRule();

        var tgtGO = _targeting.TargetedEnemy;
        bool hasTarget = tgtGO && tgtGO.activeSelf;

        if (hasTarget && !_hadTargetPrev) { StopMoveIfNeeded(); if (attackLogic) attackLogic.StopAttack(); _unit.NotifyAttackActiveEnd(); }
        _hadTargetPrev = hasTarget;

        if (!hasTarget)
        {
            if (attackLogic) attackLogic.StopAttack();
            _unit.NotifyAttackActiveEnd();
            if (keepMarchingWhenNoTarget) StartMoveForwardIfNeeded();
            return;
        }

        var tTr = tgtGO.transform;
        float dSqr = (_tr.position - tTr.position).sqrMagnitude;

        if (dSqr <= _stopDistSqr)
        {
            StopMoveIfNeeded();

            if (_unit.AttackSpeed <= 0.0f)
            {
                if (attackLogic) attackLogic.StopAttack();
                _unit.NotifyAttackActiveEnd();
                return;
            }

            if (TryCastSkill(tgtGO)) return;

            if (attackLogic != null && !attackLogic.IsAttacking)
            {
                if (!_attackTempoSet) { attackLogic.SetTempo(_unit.AttackActiveSec, _unit.AttackRecoverySec); _attackTempoSet = true; }
                attackLogic.StartAttack(_unit, tgtGO);
            }
        }
        else
        {
            if (attackLogic) attackLogic.StopAttack();
            _unit.NotifyAttackActiveEnd();
            _move.MoveTo(tTr.position, _unit.MoveSpeed);
            _isMoving = true;
        }
    }

    private bool TryCastSkill(GameObject target)
    {
        if (!skillLogic || skillLogic.IsCasting) return false;
        if (_unit.CurrentMana < skillLogic.manaCost) return false;

        var stat = _unit.UnitStat;
        if (skillLogic is IConfigurableSkill cfg && stat != null) cfg.ApplyConfigFromStat(stat);

        if (stat && stat.active != null)
        {
            skillLogic.manaCost = stat.active.manaCost;
            skillLogic.skillName = string.IsNullOrEmpty(stat.active.displayName) ? stat.active.skillLogic : stat.active.displayName;
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
            if (!cur.activeSelf) { _targeting.SetTarget(null); cur = null; }
            else
            {
                float dSqr = (_tr.position - cur.transform.position).sqrMagnitude;
                if (dSqr > _releaseDistSqr) { _targeting.SetTarget(null); cur = null; }
            }
        }

        _retargetTimer += Time.deltaTime;
        if (cur == null || _retargetTimer >= retargetEvery) { _retargetTimer = 0.0f; _targeting.FindNewTarget(); }
    }

    private void StartMoveForwardIfNeeded() { if (_isMoving) return; _move.StartMoveInDirection(_forwardDir, _unit.MoveSpeed); _isMoving = true; }
    private void StopMoveIfNeeded() { if (!_isMoving) return; _move.StopMove(); _isMoving = false; }
}
