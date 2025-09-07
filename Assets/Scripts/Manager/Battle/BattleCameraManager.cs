using System.Collections;
using UnityEngine;

public class BattleCameraManager : MonoBehaviour
{
    public static BattleCameraManager Instance { get; private set; }

    [SerializeField] private Transform attackCameraPoint;
    [SerializeField] private Transform defenseCameraPoint;
    [SerializeField] private float moveTime = 0.35f;
    [SerializeField] private AnimationCurve ease;

    private Camera mainCamera;
    private Coroutine moveCo;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCamera = Camera.main;
    }

    public void MoveToAllyPosition()
    {
        var p = BattleSystemManager.Instance?.GetAllyCameraPointByState();
        if (p) StartSmoothMove(p.position);
    }

    public void MoveToEnemyPosition()
    {
        var p = BattleSystemManager.Instance?.GetEnemyCameraPointByState();
        if (p) StartSmoothMove(p.position);
    }

    private void StartSmoothMove(Vector3 targetWorldPos)
    {
        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(MoveTo(targetWorldPos));
    }

    private IEnumerator MoveTo(Vector3 targetWorldPos)
    {
        Vector3 start = mainCamera.transform.position;
        Vector3 end = new Vector3(targetWorldPos.x, targetWorldPos.y, start.z);

        float dur = Mathf.Max(0.0001f, moveTime);
        float t = 0.0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = (ease != null) ? ease.Evaluate(t / dur) : (t / dur);
            mainCamera.transform.position = Vector3.LerpUnclamped(start, end, k);
            yield return null;
        }
        mainCamera.transform.position = end;
        moveCo = null;
    }
}
