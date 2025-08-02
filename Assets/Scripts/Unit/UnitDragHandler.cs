using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private Camera mainCamera;

    private BoxCollider2D attackSpawnArea;
    private Collider2D myCollider;

    // 드래그 중 다른 유닛과 겹침 방지용 거리
    private float minDistance = 2.0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        attackSpawnArea = GameObject.Find("AttackSpawnArea").GetComponent<BoxCollider2D>();
        myCollider = GetComponent<Collider2D>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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
        if (myCollider != null)
            myCollider.enabled = true;

        if (attackSpawnArea != null && attackSpawnArea.OverlapPoint(transform.position))
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, minDistance);
            bool tooCloseToOtherUnits = overlaps.Any(collider =>
                collider.gameObject != this.gameObject &&
                collider.GetComponent<UnitBase>() != null &&
                Vector2.Distance(transform.position, collider.transform.position) < minDistance);

            if (tooCloseToOtherUnits)
            {
                Debug.LogWarning($"{gameObject.name}은(는) 다른 유닛과 너무 가까움! 원위치로 이동");
                transform.position = originalPosition;
            }
            else
            {
                Debug.Log($"{gameObject.name} 정상적으로 배치됨");
                // 배치 확정 처리 (필요 시 여기서 로직 추가)
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}은(는) 배치 가능 영역 밖에 있음. 원위치로 이동");
            transform.position = originalPosition;
        }
    }
}