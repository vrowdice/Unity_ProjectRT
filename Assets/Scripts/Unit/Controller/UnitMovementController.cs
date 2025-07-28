using System.Collections;
using UnityEngine;

// �̵� ��Ʈ�ѷ�
public class UnitMovementController : MonoBehaviour
{
    private UnitBase unit;
    private Coroutine moveRoutine;

    private Vector3 currentDestination;
    private float currentMoveSpeed;

    private void Awake()
    {
        unit = GetComponent<UnitBase>();
        if (unit == null)
        {
            enabled = false;
        }
    }

    public void StartMove(Vector3 destination, float moveSpeed)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        currentDestination = destination; // ���ο� ������ ����
        currentMoveSpeed = moveSpeed;     // ���ο� �̵� �ӵ� ����
        moveRoutine = StartCoroutine(MoveToRoutine()); // �̵� �ڷ�ƾ ����
    }

    protected IEnumerator MoveToRoutine()
    {

        while (Vector3.Distance(transform.position, currentDestination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentDestination, currentMoveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = currentDestination; 
        moveRoutine = null; 
    }

    public void StopMoveRoutine()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;        
        }
    }
}