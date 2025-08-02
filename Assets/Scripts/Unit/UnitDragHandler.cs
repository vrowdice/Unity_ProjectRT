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
            // �ݶ��̴� ���ΰ� �ƴ� ��� �巡�� ���
            Debug.Log("���� �߽� ��ó�� ������ �巡�� ����");
            return;
        }

        Debug.Log("�巡�� ����");
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
                Debug.LogWarning("�ٸ� ���ְ� �ʹ� �����! ����ġ ����");
                transform.position = originalPosition;
            }
            else
            {
                Debug.Log("���� ��ġ");
                // ��ġ Ȯ��
            }
        }
        else
        {
            Debug.LogWarning("��ġ ���� ���� �ƴ�. ����ġ ����");
            transform.position = originalPosition;
        }
    }

    private void TryRecall()
    {
        Debug.Log("���� ȸ����");

        // BattleBeforeUI�� ���� ���� �ǵ�����
        BattleBeforeUI battleBeforeUI = FindObjectOfType<BattleBeforeUI>();
        if (battleBeforeUI != null)
        {
            battleBeforeUI.AddUnitToList(unitStatData);
        }
        else
        {
            Debug.LogWarning("BattleBeforeUI�� ã�� �� �����ϴ�.");
        }

        Destroy(this.gameObject); // �Ǵ� Ǯ�� ��� ��ȯ
    }
}