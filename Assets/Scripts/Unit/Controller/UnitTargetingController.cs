using UnityEngine;

[DisallowMultipleComponent]
public class UnitTargetingController : MonoBehaviour
{
    [SerializeField] private UnitBase owner;
    public GameObject TargetedEnemy { get; private set; }

    private LayerMask _opponentMask;

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
        TargetedEnemy = null;
        if (owner == null) return;

        Collider2D[] hits = _opponentMask != 0
            ? Physics2D.OverlapCircleAll(owner.transform.position, owner.EnemySearchRange, _opponentMask)
            : Physics2D.OverlapCircleAll(owner.transform.position, owner.EnemySearchRange); // ���̾� �̼��� �� ����

        float best = float.MaxValue;
        foreach (var h in hits)
        {
            if (h == null) continue;
            var ub = h.GetComponent<UnitBase>();
            if (ub == null || ub.IsDead) continue;

            // ���� ���� ���� �� ����
            if (_opponentMask == 0 && ub.Team == owner.Team) continue;

            float d = Vector3.Distance(owner.transform.position, ub.transform.position);
            if (d < best)
            {
                best = d;
                TargetedEnemy = ub.gameObject;
            }
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
