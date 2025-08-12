using UnityEngine;
using System.Collections.Generic;

public class UnitTargetingController : MonoBehaviour
{
    private UnitBase _unitBase;

    public GameObject TargetedEnemy { get; private set; }
    public float DistanceToTargetedEnemy { get; private set; } = float.MaxValue;

    private void Awake()
    {
        _unitBase = GetComponent<UnitBase>();
    }

    public bool IsTargetAlive(GameObject go)
    {
        if (!go) return false;
        if (!go.activeInHierarchy) return false;
        var ub = go.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    public void FindNewTarget()
    {
        if (BattleSystemManager.Instance == null || _unitBase == null) return;

        List<UnitBase> potentialTargets =
            (_unitBase.Faction == FactionType.TYPE.Owl)
            ? BattleSystemManager.Instance.EnemyUnits
            : BattleSystemManager.Instance.AllyUnits;

        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var pt in potentialTargets)
        {
            if (!pt || pt.IsDead || !pt.gameObject.activeInHierarchy) continue;

            float d = Vector3.Distance(transform.position, pt.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = pt.gameObject;
            }
        }

        SetTarget(nearest);
    }

    public bool IsInRange(float range)
    {
        return TargetedEnemy && DistanceToTargetedEnemy <= range;
    }

    public void RefreshDistance()
    {
        if (TargetedEnemy)
            DistanceToTargetedEnemy = Vector3.Distance(transform.position, TargetedEnemy.transform.position);
        else
            DistanceToTargetedEnemy = float.MaxValue;
    }

    public void SetTarget(GameObject target)
    {
        TargetedEnemy = IsTargetAlive(target) ? target : null;
        RefreshDistance();
    }
}
