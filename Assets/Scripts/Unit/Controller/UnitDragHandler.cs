using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private Camera mainCamera;

    // 참조들
    private BoxCollider2D allowedArea;
    private UnitStatBase unitStatData;
    private BattleBeforeUI ui;
    private Collider2D[] allColliders;

    private bool isEnabled = true;
    public void EnableDrag(bool value) => isEnabled = value;

    private void Awake()
    {
        mainCamera = Camera.main;
        allColliders = GetComponentsInChildren<Collider2D>(includeInactive: false);
    }

    public void SetReferences(UnitStatBase stat, BoxCollider2D spawnArea, BattleBeforeUI uiRef)
    {
        unitStatData = stat;
        allowedArea = spawnArea;
        ui = uiRef;
        isEnabled = true;
    }

    // 드래그 허용 조건 수정
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

    private void TryPlacement()
    {
        if (allowedArea && !allowedArea.OverlapPoint(transform.position))
        {
            TryRecall();
            return;
        }

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
        Debug.Log("유닛 회수");
        ui?.OnUnitRecalled(unitStatData);
        var mgr = BattleSystemManager.Instance;
        if (mgr) mgr.UnregisterUnit(GetComponent<UnitBase>());

        Destroy(gameObject);
    }
}