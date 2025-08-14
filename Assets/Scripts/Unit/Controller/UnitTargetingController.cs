using UnityEngine;
using System;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UnitTargetingController : MonoBehaviour
{
    private UnitBase _unitBase;

    [Header("Ž��/��Ÿ�")]
    [Tooltip("�� Ž�� �ݰ�(�⺻). UnitBase/Stat�� ���� Ž�������� �ִٸ� �� ���� �켱 ����ϵ��� �ɼ� ����")]
    [SerializeField] private float defaultSearchRadius = 8.0f;

    [Tooltip("Ÿ�� ���� �����׸��ý�: ���� Ÿ���� �� �ĺ����� �� ��ŭ ������ ����")]
    [SerializeField] private float keepTargetBias = 0.5f;

    [Header("��Ÿ���� ��å")]
    [Tooltip("��Ÿ���� �ּ� ����(��). �ʹ� ���� Ÿ�� ���� ����")]
    [SerializeField] private float retargetCooldown = 0.25f;
    private float _nextRetargetAllowedTime = 0.0f;

    [Header("�浹/�þ�")]
    [Tooltip("Ÿ�� �˻��� ����� ���̾� ����ũ(��: Units ���̾�)")]
    [SerializeField] private LayerMask unitLayerMask = ~0; // �⺻ ��� ���̾�

    [Tooltip("�þ����� üũ(����ĳ��Ʈ). ��� ���ϸ� false")]
    [SerializeField] private bool checkLineOfSight = false;

    [Tooltip("�þ����� ���̾�(��/��ֹ� ��)")]
    [SerializeField] private LayerMask obstacleMask = 0;

    [Header("�����")]
    [SerializeField] private bool drawGizmos = false;

    // ����
    public GameObject TargetedEnemy { get; private set; }
    public float DistanceToTargetedEnemy { get; private set; } = float.MaxValue;

    // �̺�Ʈ
    public event Action<GameObject /*old*/, GameObject /*new*/> OnTargetChanged;

    private void Awake()
    {
        _unitBase = GetComponent<UnitBase>();
        if (!_unitBase)
            Debug.LogError($"[{nameof(UnitTargetingController)}] UnitBase�� �����ϴ�. ({name})");
    }

    private void OnDisable()
    {
        if (TargetedEnemy) SetTarget(null);
    }

    /// �ܺο��� ������ Ÿ���� ����ְ� ��Ÿ� ���ΰ�?�� ������ Ȯ���ϰ� ���� ��.
    public bool IsInRange(float range)
    {
        return TargetedEnemy && DistanceToTargetedEnemy <= range;
    }

    /// ���� Ÿ�� ����/��ȿ�� ����� �� �Ÿ� ����
    public void RefreshDistance()
    {
        if (TargetedEnemy && IsTargetAlive(TargetedEnemy))
        {
            DistanceToTargetedEnemy = (TargetedEnemy.transform.position - transform.position).sqrMagnitude; // sqr
        }
        else
        {
            DistanceToTargetedEnemy = float.MaxValue;
            if (TargetedEnemy) SetTarget(null);
        }
    }

    public void SetTarget(GameObject target)
    {
        var old = TargetedEnemy;

        if (target && !IsTargetAlive(target))
            target = null;

        TargetedEnemy = target;
        DistanceToTargetedEnemy = target
            ? (target.transform.position - transform.position).sqrMagnitude
            : float.MaxValue;

        if (old != TargetedEnemy)
            OnTargetChanged?.Invoke(old, TargetedEnemy);
    }

    /// Ÿ���� ����ִ��� �Ǵ�
    public bool IsTargetAlive(GameObject go)
    {
        if (!go || !go.activeInHierarchy) return false;
        var ub = go.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    /// <summary>
    /// ��ȹ �÷ο�: ������ ����� ���� ����(Ž�� �ݰ�/�þ� �ɼ� �ݿ�)
    /// - ��Ÿ���� ��ٿ� ����
    /// - ���� Ÿ�� ���� ���̾(keepTargetBias)
    /// </summary>
    public void FindNewTarget()
    {
        if (Time.time < _nextRetargetAllowedTime) return; 
        _nextRetargetAllowedTime = Time.time + retargetCooldown;

        if (BattleSystemManager.Instance == null || _unitBase == null)
            return;

        // 1) �ĺ� Ǯ ��������
        List<UnitBase> pool = (_unitBase.Faction == FactionType.TYPE.Owl)
            ? BattleSystemManager.Instance.EnemyUnits
            : BattleSystemManager.Instance.AllyUnits;

        if (pool == null || pool.Count == 0)
        {
            SetTarget(null);
            return;
        }

        float searchRadius = defaultSearchRadius;
        if (_unitBase.UnitStat != null && _unitBase.UnitStat.enemySearchRange > 0f)
            searchRadius = _unitBase.UnitStat.enemySearchRange;

        float searchRadiusSqr = searchRadius * searchRadius;

        GameObject best = null;
        float bestSqr = float.MaxValue;

        Vector3 myPos = transform.position;

        foreach (var pt in pool)
        {
            if (!pt || pt.IsDead || !pt.gameObject.activeInHierarchy) continue;

            if (((1 << pt.gameObject.layer) & unitLayerMask) == 0) continue;

            Vector3 to = pt.transform.position - myPos;
            float d2 = to.sqrMagnitude;
            if (d2 > searchRadiusSqr) continue;

            if (checkLineOfSight)
            {
                if (Physics2D.Linecast(myPos, pt.transform.position, obstacleMask)) continue;
            }

            if (d2 < bestSqr)
            {
                bestSqr = d2;
                best = pt.gameObject;
            }
        }

        if (TargetedEnemy && best)
        {
            float cur = (TargetedEnemy.transform.position - myPos).sqrMagnitude;
            if (cur <= bestSqr + (keepTargetBias * keepTargetBias))
            {
                RefreshDistance(); 
                return;
            }
        }

        SetTarget(best);
    }

    /// ������ ƽ ��ƾ
    /// - ���� ���� ��, Ȥ�� ���� ���� ƽ���� ȣ��
    /// - ����� ���ſ� FindNewTarget()�� �ʿ� �ÿ��� ȣ��(Ÿ�� ��ȿ/��Ÿ� ��Ż ��)
    public void TickTargeting(float preferRangeForKeep = -1.0f)
    {
        RefreshDistance();

        if (!TargetedEnemy)
        {
            FindNewTarget();
            return;
        }

        if (preferRangeForKeep > 0.0f &&
            DistanceToTargetedEnemy > preferRangeForKeep * preferRangeForKeep)
        {
            FindNewTarget();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        // Ž�� �ݰ� �ð�ȭ
        Gizmos.color = Color.yellow;
        float r = (_unitBase && _unitBase.UnitStat && _unitBase.UnitStat.enemySearchRange > 0f)
            ? _unitBase.UnitStat.enemySearchRange
            : defaultSearchRadius;
        Gizmos.DrawWireSphere(transform.position, r);

        if (TargetedEnemy)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, TargetedEnemy.transform.position);
        }
    }
#endif
}
