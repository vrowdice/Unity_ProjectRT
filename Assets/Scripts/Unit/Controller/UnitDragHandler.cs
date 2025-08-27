using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class UnitDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Vector3 originalPosition;
    private Camera mainCamera;

    // 참조들
    private BoxCollider2D allowedArea;     // 배치 가능 영역
    private UnitData unitStatData;       
    private BattleBeforeUI ui;               
    private Collider2D[] allColliders;
    private UnitBase unit;                   // 회수시 캐시는 매니저로 감

    private bool isEnabled = true;
    public void EnableDrag(bool value) => isEnabled = value;

    private void Awake()
    {
        mainCamera = Camera.main;
        allColliders = GetComponentsInChildren<Collider2D>(includeInactive: false);
        unit = GetComponent<UnitBase>();
    }

    public void SetReferences(UnitData stat, BoxCollider2D spawnArea, BattleBeforeUI uiRef)
    {
        unitStatData = stat;
        allowedArea = spawnArea;
        ui = uiRef;
        isEnabled = true;
    }

    // 드래그 허용 조건: 아군 진영 뷰 + 전투중 아님 + 이 핸들러가 활성화
    private bool CanDrag
    {
        get
        {
            var mgr = BattleSystemManager.Instance;
            return isEnabled
                && ui != null
                && (mgr?.IsViewingAllyBase ?? false)
                && !(mgr?.IsBattleRunning ?? false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag) return;

        originalPosition = transform.position;

        foreach (var col in allColliders) col.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag) return;

        Vector3 p = mainCamera.ScreenToWorldPoint(eventData.position);
        p.z = 0;

        if (allowedArea)
        {
            var b = allowedArea.bounds;
            p.x = Mathf.Clamp(p.x, b.min.x, b.max.x);
            p.y = Mathf.Clamp(p.y, b.min.y, b.max.y);
        }

        transform.position = p;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        foreach (var col in allColliders) col.enabled = true;
        if (!CanDrag) return;

        if (BattleBeforeUI.IsInPlacementMode) TryPlacement();
        else TryRecall();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!isEnabled) return;

        var mgr = BattleSystemManager.Instance;
        if (mgr != null && mgr.IsBattleRunning) return;

        if (BattleBeforeUI.IsInPlacementMode) return;

        TryRecall();
    }

    private void TryPlacement()
    {
        if (allowedArea && !allowedArea.OverlapPoint(transform.position))
        {
            TryRecall();
            return;
        }

        // 최소 간격 체크
        const float minDistance = 0.5f;
        var overlaps = Physics2D.OverlapCircleAll(transform.position, minDistance);
        bool tooClose = overlaps.Any(c =>
            c.gameObject != this.gameObject &&
            c.GetComponent<UnitBase>() != null &&
            Vector2.Distance(transform.position, c.transform.position) < minDistance);

        if (tooClose)
        {
            Debug.LogWarning("다른 유닛과 너무 가까움! 원위치 복귀");
            transform.position = originalPosition;
        }
    }

    private void TryRecall()
    {
        // 방어 코드: 참조 없으면 중단
        if (ui == null || unitStatData == null || unit == null)
        {
            Debug.LogWarning("[UnitDragHandler] 회수 실패: 참조 누락");
            return;
        }

        ui.OnUnitRecalled(unitStatData);

        var mgr = BattleSystemManager.Instance;
        if (mgr != null)
        {
            bool ok = mgr.RecallAlly(unit);
            if (!ok)
            {
                Debug.LogWarning("[UnitDragHandler] RecallAlly 실패 ");
            }
        }
        else
        {
            Debug.LogWarning("[UnitDragHandler] BattleSystemManager 없음");
            Destroy(gameObject);
        }
    }
}
