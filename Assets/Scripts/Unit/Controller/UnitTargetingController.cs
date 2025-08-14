using UnityEngine;
using System;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UnitTargetingController : MonoBehaviour
{
    private UnitBase _unitBase;

    [Header("탐색/사거리")]
    [Tooltip("적 탐색 반경(기본). UnitBase/Stat에 별도 탐색범위가 있다면 그 값을 우선 사용하도록 옵션 제공")]
    [SerializeField] private float defaultSearchRadius = 8.0f;

    [Tooltip("타겟 유지 히스테리시스: 기존 타겟이 새 후보보다 이 만큼 가까우면 유지")]
    [SerializeField] private float keepTargetBias = 0.5f;

    [Header("재타겟팅 정책")]
    [Tooltip("재타겟팅 최소 간격(초). 너무 잦은 타겟 변경 방지")]
    [SerializeField] private float retargetCooldown = 0.25f;
    private float _nextRetargetAllowedTime = 0.0f;

    [Header("충돌/시야")]
    [Tooltip("타겟 검색에 사용할 레이어 마스크(예: Units 레이어)")]
    [SerializeField] private LayerMask unitLayerMask = ~0; // 기본 모든 레이어

    [Tooltip("시야차단 체크(라인캐스트). 사용 안하면 false")]
    [SerializeField] private bool checkLineOfSight = false;

    [Tooltip("시야차단 레이어(벽/장애물 등)")]
    [SerializeField] private LayerMask obstacleMask = 0;

    [Header("디버그")]
    [SerializeField] private bool drawGizmos = false;

    // 상태
    public GameObject TargetedEnemy { get; private set; }
    public float DistanceToTargetedEnemy { get; private set; } = float.MaxValue;

    // 이벤트
    public event Action<GameObject /*old*/, GameObject /*new*/> OnTargetChanged;

    private void Awake()
    {
        _unitBase = GetComponent<UnitBase>();
        if (!_unitBase)
            Debug.LogError($"[{nameof(UnitTargetingController)}] UnitBase가 없습니다. ({name})");
    }

    private void OnDisable()
    {
        if (TargetedEnemy) SetTarget(null);
    }

    /// 외부에서 “현재 타겟이 살아있고 사거리 내인가?” 빠르게 확인하고 싶을 때.
    public bool IsInRange(float range)
    {
        return TargetedEnemy && DistanceToTargetedEnemy <= range;
    }

    /// 현재 타겟 생존/유효성 재검증 및 거리 갱신
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

    /// 타겟이 살아있는지 판단
    public bool IsTargetAlive(GameObject go)
    {
        if (!go || !go.activeInHierarchy) return false;
        var ub = go.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    /// <summary>
    /// 기획 플로우: “가장 가까운 적” 선택(탐색 반경/시야 옵션 반영)
    /// - 재타겟팅 쿨다운 적용
    /// - 기존 타겟 유지 바이어스(keepTargetBias)
    /// </summary>
    public void FindNewTarget()
    {
        if (Time.time < _nextRetargetAllowedTime) return; 
        _nextRetargetAllowedTime = Time.time + retargetCooldown;

        if (BattleSystemManager.Instance == null || _unitBase == null)
            return;

        // 1) 후보 풀 가져오기
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

    /// 프레임 틱 루틴
    /// - 전투 중일 때, 혹은 공격 루프 틱에서 호출
    /// - 비용이 무거운 FindNewTarget()은 필요 시에만 호출(타겟 무효/사거리 이탈 등)
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
        // 탐색 반경 시각화
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
