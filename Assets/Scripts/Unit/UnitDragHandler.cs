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

    // �巡�� �� �ٸ� ���ְ� ��ħ ������ �Ÿ�
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
                Debug.LogWarning($"{gameObject.name}��(��) �ٸ� ���ְ� �ʹ� �����! ����ġ�� �̵�");
                transform.position = originalPosition;
            }
            else
            {
                Debug.Log($"{gameObject.name} ���������� ��ġ��");
                // ��ġ Ȯ�� ó�� (�ʿ� �� ���⼭ ���� �߰�)
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}��(��) ��ġ ���� ���� �ۿ� ����. ����ġ�� �̵�");
            transform.position = originalPosition;
        }
    }
}