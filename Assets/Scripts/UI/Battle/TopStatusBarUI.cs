using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10)]
[DisallowMultipleComponent]
public class TopStatusBarUI : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private TMP_Text allyCountText;
    [SerializeField] private Image enemySideIcon;
    [SerializeField] private Image allySideIcon;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button settingsButton;

    [Header("옵션")]
    private float refreshInterval = 0.25f;

    private BattleSystemManager bsm;
    private Coroutine loop;
    private WaitForSeconds cachedWait;

    private void Awake()
    {
        if (TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        foreach (var g in GetComponentsInChildren<Graphic>(true))
            g.raycastTarget = false;

        EnableButtonRaycasts(helpButton, true);
        EnableButtonRaycasts(settingsButton, true);

        // 루트 RectTransform 앵커 보정(상단 고정, 가로 스트레치)
        if (transform is RectTransform rt)
        {
            rt.anchorMin = new Vector2(0.0f, 1.0f);
            rt.anchorMax = new Vector2(1.0f, 1.0f);
            rt.pivot = new Vector2(0.5f, 1.0f);
            rt.anchoredPosition = Vector2.zero;
        }
    }

    private void OnEnable()
    {
        bsm = BattleSystemManager.Instance;
        if (bsm != null) bsm.UnitsChanged += UpdateCountsNow;

        cachedWait = (refreshInterval > 0.0f) ? new WaitForSeconds(refreshInterval) : null;

        UpdateCountsNow();

        if (loop == null && refreshInterval > 0.0f)
            loop = StartCoroutine(RefreshLoop());
    }

    private void OnDisable()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
        if (bsm != null) bsm.UnitsChanged -= UpdateCountsNow;
    }

    private IEnumerator RefreshLoop()
    {
        while (enabled && gameObject.activeInHierarchy && refreshInterval > 0.0f)
        {
            UpdateCountsNow();
            yield return cachedWait ?? new WaitForSeconds(refreshInterval);
        }
        loop = null;
    }

    private void UpdateCountsNow()
    {
        if (!enemyCountText || !allyCountText)
            return;

        if (bsm == null)
        {
            enemyCountText.text = "--";
            allyCountText.text = "--";
            return;
        }

        enemyCountText.text = CountAlive(bsm.EnemyUnits).ToString();
        allyCountText.text = CountAlive(bsm.AllyUnits).ToString();
    }

    private static int CountAlive(IReadOnlyList<UnitBase> list)
    {
        if (list == null) return 0;

        int c = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var u = list[i];
            if (u && u.gameObject.activeInHierarchy && !u.IsDead)
                c++;
        }
        return c;
    }

    private static void EnableButtonRaycasts(Button btn, bool on)
    {
        if (!btn) return;
        foreach (var gg in btn.GetComponentsInChildren<Graphic>(true))
            gg.raycastTarget = on;
        if (btn.image) btn.image.raycastTarget = on;
    }

    public void RefreshNow() => UpdateCountsNow();
}
