using UnityEngine;

[DisallowMultipleComponent]
public class UnitMovementController : MonoBehaviour
{
    [Header("충돌/지형")]
    [SerializeField] private LayerMask obstacleMask;
    [Tooltip("벽에 너무 붙어 보이지 않게 하는 안전 여백")]
    [SerializeField] private float skin = 0.02f;

    [Header("이동 파라미터(디버그/튜닝)")]
    [SerializeField] private float stopEpsilon = 0.01f; 

    [Header("맵 경계")]
    [SerializeField] private BoxCollider2D mapBounds;
    [SerializeField] private bool clampToMapBounds = true;

    private Vector3 _targetPosition;
    private Vector3 _dir;
    private float _moveSpeed;
    private bool _isMoving = false;
    private bool _directional = false;

    private Rigidbody2D _rb2d;
    private Collider2D _col2d;

    private readonly RaycastHit2D[] _hitBuf = new RaycastHit2D[8];
    private ContactFilter2D _contactFilter;

    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _col2d = GetComponent<Collider2D>();

        _contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false
        };
        _contactFilter.SetLayerMask(obstacleMask);

        if (_rb2d)
        {
            _rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    public void SetMapBounds(BoxCollider2D bounds) => mapBounds = bounds;

    public void MoveTo(Vector3 target, float speed)
    {
        _directional = false;
        _targetPosition = target;
        _moveSpeed = Mathf.Max(0f, speed);
        _isMoving = true;
    }

    public void StartMoveInDirection(Vector3 direction, float speed)
    {
        _directional = true;
        _dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.right;
        _moveSpeed = Mathf.Max(0f, speed);
        _isMoving = true;
    }

    public void StopMove()
    {
        _isMoving = false;
        if (_rb2d) _rb2d.velocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!_isMoving) return;

        float dt = Time.fixedDeltaTime;

        if (_directional)
        {
            Vector3 delta = _dir * _moveSpeed * dt;
            Vector3 allowed = ComputeAllowedDelta(delta);
            ApplyDelta(allowed);
            if (allowed.sqrMagnitude < 1e-6f) StopMove();
        }
        else
        {
            Vector3 cur = transform.position;
            Vector3 toTarget = _targetPosition - cur;

            if (toTarget.sqrMagnitude <= stopEpsilon * stopEpsilon)
            {
                StopMove();
                return;
            }

            float maxThisFrame = _moveSpeed * dt;
            Vector3 delta = Vector3.ClampMagnitude(toTarget, maxThisFrame);

            Vector3 allowed = ComputeAllowedDelta(delta);
            ApplyDelta(allowed);

            if (allowed.sqrMagnitude < 1e-6f ||
                (Vector3.SqrMagnitude(_targetPosition - transform.position) <= stopEpsilon * stopEpsilon))
            {
                StopMove();
            }
        }
    }

    private Vector3 ComputeAllowedDelta(Vector3 delta)
    {
        float dist = delta.magnitude;
        if (dist <= 1e-6f) return Vector3.zero;

        Vector2 dir = (Vector2)(delta / dist);
        float allowedDist = dist;

        int hitCount = 0;

        if (_rb2d)
        {
            hitCount = _rb2d.Cast(dir, _contactFilter, _hitBuf, dist + skin);
        }
        else if (_col2d)
        {
            hitCount = _col2d.Cast(dir, _contactFilter, _hitBuf, dist + skin);
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist + skin, obstacleMask);
            if (hit.collider != null) { _hitBuf[0] = hit; hitCount = 1; }
        }

        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                var h = _hitBuf[i];
                if (h.collider == null) continue;
                float cand = Mathf.Max(0f, h.distance - skin);
                allowedDist = Mathf.Min(allowedDist, cand);
            }
        }

        return (Vector3)(dir * allowedDist);
    }

    private void ApplyDelta(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 1e-12f) return;

        Vector3 cur = _rb2d ? (Vector3)_rb2d.position : transform.position;
        Vector3 next = cur + delta;

        if (clampToMapBounds && mapBounds)
            next = ClampToMap(next);

        if (_rb2d && !_rb2d.isKinematic)
            _rb2d.MovePosition((Vector2)next);
        else
            transform.position = next;
    }


    private Vector3 ClampToMap(Vector3 worldPos)
    {
        var b = mapBounds.bounds;

        Vector3 ext = Vector3.zero;
        if (_col2d) ext = _col2d.bounds.extents;

        float minX = b.min.x + ext.x + skin;
        float maxX = b.max.x - ext.x - skin;
        float minY = b.min.y + ext.y + skin;
        float maxY = b.max.y - ext.y - skin;

        if (minX > maxX) { float mid = (b.min.x + b.max.x) * 0.5f; minX = maxX = mid; }
        if (minY > maxY) { float mid = (b.min.y + b.max.y) * 0.5f; minY = maxY = mid; }

        float x = Mathf.Clamp(worldPos.x, minX, maxX);
        float y = Mathf.Clamp(worldPos.y, minY, maxY);
        return new Vector3(x, y, worldPos.z);
    }
}
