using UnityEngine;

public class UnitMovementController : MonoBehaviour
{
    private Vector3 _targetPosition;
    private Vector3 _dir;
    private float _moveSpeed;
    private bool _isMoving = false;
    private bool _directional = false;

    public void MoveTo(Vector3 target, float speed)
    {
        _directional = false;
        _targetPosition = target;
        _moveSpeed = speed;
        _isMoving = true;
    }

    public void StartMoveInDirection(Vector3 direction, float speed)
    {
        _directional = true;
        _dir = direction.normalized;
        _moveSpeed = speed;
        _isMoving = true;
    }

    public void StopMove()
    {
        _isMoving = false;
    }

    private void Update()
    {
        if (!_isMoving) return;

        if (_directional)
        {
            transform.position += _dir * _moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
                StopMove();
        }
    }
}
