using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragPan : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;

    [Header("감도")]
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField] private float smooth = 15.0f;      
    [SerializeField] private bool invert = false;
    [SerializeField] private bool useInertia = true;
    [SerializeField] private float inertiaDamping = 8.0f; 

    [Header("영역")]
    [SerializeField] private BoxCollider2D mapBounds;   
    [SerializeField] private float boundsPaddingX = 0.5f;

    private bool enabledDrag;
    private bool dragging;
    private Vector3 lastScreenPos;
    private float targetX;
    private float minX, maxX;
    private float velocityX; 

    public void Enable(bool on)
    {
        enabledDrag = on;
        dragging = false;
        velocityX = 0.0f;
        if (!cam) cam = Camera.main;
        if (cam) targetX = cam.transform.position.x;
        RecalcBounds();
    }

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        if (cam) targetX = cam.transform.position.x;
        RecalcBounds();
    }

    private void LateUpdate()
    {
        if (!enabledDrag || !cam) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
        // 관성
        if (!dragging && useInertia && Mathf.Abs(velocityX) > 0.0001f)
        {
            targetX = Mathf.Clamp(targetX + velocityX * Time.unscaledDeltaTime, minX, maxX);
            velocityX = Mathf.Lerp(velocityX, 0.0f, 1.0f - Mathf.Exp(-inertiaDamping * Time.unscaledDeltaTime));
        }

        // 스무딩
        var p = cam.transform.position;
        p.x = Mathf.Lerp(p.x, targetX, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
        cam.transform.position = p;
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PointerOverUI()) return;
            dragging = true;
            lastScreenPos = Input.mousePosition;
            velocityX = 0.0f;
        }
        if (Input.GetMouseButtonUp(0)) dragging = false;
        if (!dragging) return;

        Vector3 now = Input.mousePosition;
        Vector3 delta = now - lastScreenPos;
        lastScreenPos = now;

        ApplyScreenDeltaX(delta.x, true);
    }

    private void HandleTouch()
    {
        if (Input.touchCount == 0) { dragging = false; return; }
        var t = Input.GetTouch(0); // 정책: 첫 손가락만 사용

        if (t.phase == TouchPhase.Began)
        {
            if (PointerOverUI(t.fingerId)) return;
            dragging = true;
            lastScreenPos = t.position;
            velocityX = 0.0f;
        }
        else if (t.phase == TouchPhase.Moved && dragging)
        {
            Vector3 delta = (Vector3)t.position - lastScreenPos;
            lastScreenPos = t.position;
            ApplyScreenDeltaX(delta.x, true);
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            dragging = false;
        }
    }

    private void ApplyScreenDeltaX(float deltaPixelsX, bool accumulateInertia)
    {
        if (Mathf.Approximately(deltaPixelsX, 0f)) return;

        float sign = invert ? -1.0f : 1.0f;
        float dxWorld = ScreenToWorldDeltaX(deltaPixelsX) * sensitivity * sign;
        float newTarget = Mathf.Clamp(targetX + dxWorld, minX, maxX);

        if (accumulateInertia)
        {
            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            velocityX = (newTarget - targetX) / dt;
        }
        targetX = newTarget;
    }

    // 화면 X 픽셀 이동량 → 월드 X 이동량
    private float ScreenToWorldDeltaX(float deltaPixelsX)
    {
        if (!cam) return 0f;

        if (cam.orthographic)
        {
            float halfWidth = cam.orthographicSize * cam.aspect;
            return (deltaPixelsX / Screen.width) * (halfWidth * 2f);
        }
        else
        {
            Vector3 p0 = lastScreenPos;
            Vector3 p1 = lastScreenPos + new Vector3(deltaPixelsX, 0f, 0f);
            Vector3 w0 = ScreenToWorldOnPlane(p0);
            Vector3 w1 = ScreenToWorldOnPlane(p1);
            return (w1 - w0).x;
        }
    }

    private Vector3 ScreenToWorldOnPlane(Vector3 screenPos)
    {
        Ray r = cam.ScreenPointToRay(screenPos);
        float camY = cam.transform.position.y;
        if (Mathf.Abs(r.direction.y) < 1e-6f) return r.origin; 
        float t = (camY - r.origin.y) / r.direction.y;
        return r.origin + r.direction * t;
    }

    private void RecalcBounds()
    {
        if (!cam) { minX = maxX = 0.0f; return; }

        float halfViewWidth = (cam.orthographic)
            ? cam.orthographicSize * cam.aspect
            : Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * Mathf.Abs(cam.transform.position.z) * cam.aspect;

        if (mapBounds)
        {
            var b = mapBounds.bounds;
            minX = b.min.x + halfViewWidth + boundsPaddingX;
            maxX = b.max.x - halfViewWidth - boundsPaddingX;
            if (minX > maxX) { float mid = (minX + maxX) * 0.5f; minX = maxX = mid; }
        }
        else
        {
            float x = cam.transform.position.x;
            minX = maxX = x;

            targetX = Mathf.Clamp(cam.transform.position.x, minX, maxX);
        }
    }

    private static bool PointerOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null) return false;
        return (fingerId >= 0)
            ? EventSystem.current.IsPointerOverGameObject(fingerId)
            : EventSystem.current.IsPointerOverGameObject();
    }
}
