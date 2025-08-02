using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private Camera mainCamera;

    private BoxCollider2D attackSpawnArea;

    public UnitStatBase unitStatData;

    private void Awake()
    {
        mainCamera = Camera.main;
        attackSpawnArea = GameObject.Find("AttackSpawnArea").GetComponent<BoxCollider2D>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!BattleBeforeUI.IsInPlacementMode)
        {
            return;
        }

        Vector2 worldPoint = mainCamera.ScreenToWorldPoint(eventData.position);

        Collider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null || !collider.OverlapPoint(worldPoint))
        {
            // 콜라이더 내부가 아닌 경우 드래그 취소
            Debug.Log("유닛 중심 근처를 눌러야 드래그 가능");
            return;
        }

        Debug.Log("드래그 시작");
        originalPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePos = eventData.position;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        transform.position = mainCamera.ScreenToWorldPoint(mousePos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (BattleBeforeUI.IsInPlacementMode)
        {
            TryPlacement();
        }
        else
        {
            TryRecall();
        }
    }

    private void TryPlacement()
    {
        float minDistance = 2.0f;

        if (attackSpawnArea != null && attackSpawnArea.OverlapPoint(transform.position))
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, minDistance);
            bool overlapped = overlaps.Any(c =>
                c.gameObject != this.gameObject &&
                c.GetComponent<UnitBase>() != null &&
                Vector2.Distance(transform.position, c.transform.position) < minDistance);

            if (overlapped)
            {
                Debug.LogWarning("다른 유닛과 너무 가까움! 원위치 복귀");
                transform.position = originalPosition;
            }
            else
            {
                Debug.Log("정상 배치");
                // 배치 확정
            }
        }
        else
        {
            Debug.LogWarning("배치 가능 영역 아님. 원위치 복귀");
            transform.position = originalPosition;
        }
    }

    private void TryRecall()
    {
        Debug.Log("유닛 회수됨");

        // BattleBeforeUI에 유닛 정보 되돌리기
        BattleBeforeUI battleBeforeUI = FindObjectOfType<BattleBeforeUI>();
        if (battleBeforeUI != null)
        {
            battleBeforeUI.AddUnitToList(unitStatData);
        }
        else
        {
            Debug.LogWarning("BattleBeforeUI를 찾을 수 없습니다.");
        }

        Destroy(this.gameObject); // 또는 풀링 방식 반환
    }
}