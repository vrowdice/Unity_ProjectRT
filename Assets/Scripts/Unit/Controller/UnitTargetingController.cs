using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitTargetingController : MonoBehaviour
{
    [SerializeField] private UnitBase owner;
    public GameObject TargetedEnemy { get; private set; }

    private LayerMask _opponentMask;

    private static readonly Collider2D[] s_buf = new Collider2D[64];
    private struct Candidate { public UnitBase ub; public float d; }
    private static readonly List<Candidate> s_candidates = new();

    private UnitMovementController _mover;   

    private void Awake()
    {
        if (owner == null) owner = GetComponent<UnitBase>();
        RefreshMask();

        _mover = GetComponent<UnitMovementController>(); 
    }

    public void RefreshMask()
    {
        if (owner == null) return;
        _opponentMask = TeamLayers.GetEnemyMask(owner.Team);
    }

    public void SetTarget(GameObject go) => TargetedEnemy = go;

    public void FindNewTarget()
    {
        GameObject prev = TargetedEnemy;
        TargetedEnemy = null;
        if (owner == null) return;

        Collider2D[] hits = _opponentMask != 0
            ? Physics2D.OverlapCircleAll(owner.transform.position, owner.EnemySearchRange, _opponentMask)
            : Physics2D.OverlapCircleAll(owner.transform.position, owner.EnemySearchRange);

        float best = float.MaxValue;
        foreach (var h in hits)
        {
            if (h == null) continue;
            var ub = h.GetComponent<UnitBase>();
            if (ub == null || ub.IsDead) continue;

            if (_opponentMask == 0 && ub.Team == owner.Team) continue;

            float d = Vector3.Distance(owner.transform.position, ub.transform.position);
            if (d < best)
            {
                best = d;
                TargetedEnemy = ub.gameObject;
            }
        }

        if (prev == null && TargetedEnemy != null)
            owner.GetComponent<UnitMovementController>()?.StopMove();

        if (TargetedEnemy && _mover && owner)
        {
            Vector3 dir = (TargetedEnemy.transform.position - owner.transform.position).normalized;
            if (dir.sqrMagnitude > 1e-6f)
                _mover.StartMoveInDirection(dir, owner.MoveSpeed);
        }
    }

    private void Update()
    {
        if (!owner || !_mover) return;

        var t = TargetedEnemy;
        if (!t)
            return;

        var tUb = t.GetComponent<UnitBase>() ?? t.GetComponentInParent<UnitBase>();
        if (!tUb || tUb.IsDead)
        {
            TargetedEnemy = null;
            return;
        }

        float dist = Vector3.Distance(owner.transform.position, t.transform.position);
        float stopRange = owner.AttackRange;            
        if (dist > stopRange)
        {
            Vector3 dir = (t.transform.position - owner.transform.position).normalized;
            if (dir.sqrMagnitude > 1e-6f)
                _mover.StartMoveInDirection(dir, owner.MoveSpeed);
        }
        else
        {
            _mover.StopMove();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (owner != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(owner.transform.position, owner.EnemySearchRange);
        }
    }
#endif
}
