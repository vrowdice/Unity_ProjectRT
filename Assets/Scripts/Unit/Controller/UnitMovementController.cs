using System.Collections;
using UnityEngine;

// 이동 컨트롤러
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

    public void StartMove(Vector3 destination, float movementSpeed)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        currentDestination = destination;
        currentMoveSpeed = movementSpeed;
        moveRoutine = StartCoroutine(MoveToRoutine());
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