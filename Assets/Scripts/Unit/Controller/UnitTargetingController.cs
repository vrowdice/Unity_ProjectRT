using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UnitTargetingController : MonoBehaviour
{
    [SerializeField] private UnitBase owner;
    public GameObject TargetedEnemy { get; private set; }

    private LayerMask _opponentMask;

    private static readonly Collider2D[] s_buf = new Collider2D[64];
    private struct Candidate { public UnitBase ub; public float d; }
    private static readonly List<Candidate> s_candidates = new();

    private void Awake()
    {
        if (owner == null) owner = GetComponent<UnitBase>();
        RefreshMask();
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

    }


    public int AcquireTargetsNonAlloc(List<UnitBase> outList, Vector3 center, float radius,
                                      int maxTargets = -1, bool preferCurrentFirst = true)
    {
        outList?.Clear();
        if (outList == null || owner == null) return 0;

        int n = (_opponentMask != 0)
            ? Physics2D.OverlapCircleNonAlloc(center, radius, s_buf, _opponentMask)
            : Physics2D.OverlapCircleNonAlloc(center, radius, s_buf);

        s_candidates.Clear();

        UnitBase current = null;
        if (preferCurrentFirst && TargetedEnemy)
            current = TargetedEnemy.GetComponent<UnitBase>() ?? TargetedEnemy.GetComponentInParent<UnitBase>();

        for (int i = 0; i < n; i++)
        {
            var h = s_buf[i];
            if (!h) continue;

            var ub = h.GetComponent<UnitBase>();
            if (!ub || ub.IsDead) continue;

            if (_opponentMask == 0 && ub.Team == owner.Team) continue;

            float d = Vector3.Distance(center, ub.transform.position);
            s_candidates.Add(new Candidate { ub = ub, d = d });
        }

        if (s_candidates.Count == 0) return 0;

        int added = 0;
        if (current && s_candidates.Exists(c => c.ub == current))
        {
            outList.Add(current);
            added++;
        }

        s_candidates.Sort((a, b) => a.d.CompareTo(b.d));

        int cap = (maxTargets <= 0) ? int.MaxValue : maxTargets;
        for (int i = 0; i < s_candidates.Count && added < cap; i++)
        {
            var ub = s_candidates[i].ub;
            if (ub == null) continue;
            if (ub == current) continue; 
            outList.Add(ub);
            added++;
        }

        return added;
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
