using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragPan : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;

    [Header("Feel")]
    [SerializeField] private float sensitivity = 1.0f;    // 드래그 감도
    [SerializeField] private float smooth = 15.0f;        
    [SerializeField] private bool invert = false;         

    [SerializeField] private float perspectivePixelsToWorld = 0.01f;

    [Header("Bounds")]
    [SerializeField] private float boundsPaddingX = 0.5f; 

    private bool enabledDrag = false;
    private bool dragging = false;
    private Vector3 lastPos;

    private float minX, maxX; 
    private float targetX;    

    public void Enable(bool on)
    {
        enabledDrag = on;
        dragging = false;
        if (cam == null) cam = Camera.main;
        if (cam != null) targetX = Mathf.Clamp(cam.transform.position.x, minX, maxX);
    }
    public void SetBounds(Transform a, Transform b)
    {
        float x1 = a ? a.position.x : 0f;
        float x2 = b ? b.position.x : 0f;
        minX = Mathf.Min(x1, x2);
        maxX = Mathf.Max(x1, x2);

        if (cam == null) cam = Camera.main;
        if (cam != null) targetX = Mathf.Clamp(cam.transform.position.x, minX, maxX);
    }

    public void SetBoundsFromCollider2D(BoxCollider2D col)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[CameraDragPan] Camera is missing.");
            return;
        }

        if (col == null)
        {
            float x = cam.transform.position.x;
            minX = maxX = x;
            targetX = x;
            return;
        }

        var b = col.bounds;

        float halfViewWidth = GetHalfViewWidthWorld();

        minX = b.min.x + halfViewWidth + boundsPaddingX;
        maxX = b.max.x - halfViewWidth - boundsPaddingX;

        if (minX > maxX)
        {
            float mid = (minX + maxX) * 0.5f;
            minX = maxX = mid;
        }

        targetX = Mathf.Clamp(cam.transform.position.x, minX, maxX);
    }

    private float GetHalfViewWidthWorld()
    {
        if (cam.orthographic)
        {
            return cam.orthographicSize * cam.aspect;
        }
        else
        {
            float distance = Mathf.Abs(cam.transform.position.z);
            float halfHeight = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
            return halfHeight * cam.aspect;
        }
    }

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) targetX = cam.transform.position.x;
    }

    private void Update()
    {
        if (!enabledDrag || cam == null) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
        var p = cam.transform.position;
        p.x = Mathf.Lerp(p.x, targetX, 1.0f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
        cam.transform.position = p;
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            dragging = true;
            lastPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0)) dragging = false;
        if (!dragging) return;

        Vector3 now = Input.mousePosition;
        Vector3 delta = now - lastPos;
        lastPos = now;

        ApplyDelta(delta.x);
    }

    private void HandleTouch()
    {
        if (Input.touchCount == 0) { dragging = false; return; }

        var t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return;

            dragging = true;
            lastPos = t.position;
        }
        else if (t.phase == TouchPhase.Moved && dragging)
        {
            Vector3 delta = (Vector3)t.position - lastPos;
            lastPos = t.position;
            ApplyDelta(delta.x);
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            dragging = false;
        }
    }

    private void ApplyDelta(float deltaPixelsX)
    {
        if (Mathf.Approximately(deltaPixelsX, 0f)) return;

        float sign = invert ? -1.0f : 1.0f;
        float worldPerPixel;

        if (cam.orthographic)
        {
            worldPerPixel = (2f * cam.orthographicSize * cam.aspect) / Screen.width;
        }
        else
        {
            worldPerPixel = perspectivePixelsToWorld;
        }

        float dxWorld = sign * deltaPixelsX * worldPerPixel * sensitivity;
        targetX = Mathf.Clamp(targetX + dxWorld, minX, maxX);
    }
}
