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

    private BattleSystemManager bsm;

    private void Awake()
    {
        if (TryGetComponent<CanvasGroup>(out var cg)) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }

        foreach (var g in GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
        EnableButtonRaycasts(helpButton, true);
        EnableButtonRaycasts(settingsButton, true);

        if (transform is RectTransform rt)
        { rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(0.5f, 1); rt.anchoredPosition = Vector2.zero; }
    }

    private void OnEnable()
    {
        bsm = BattleSystemManager.Instance;
        if (bsm != null) bsm.UnitsChanged += UpdateCountsNow;
        UpdateCountsNow();
    }
    private void OnDisable()
    {
        if (bsm != null) bsm.UnitsChanged -= UpdateCountsNow;
    }

    private void UpdateCountsNow()
    {
        if (!enemyCountText || !allyCountText) return;
        if (bsm == null) { enemyCountText.text = "--"; allyCountText.text = "--"; return; }
        enemyCountText.text = CountAlive(bsm.EnemyUnits).ToString();
        allyCountText.text = CountAlive(bsm.AllyUnits).ToString();
    }

    private static int CountAlive(System.Collections.Generic.IReadOnlyList<UnitBase> list)
    {
        if (list == null) return 0;
        int c = 0; for (int i = 0; i < list.Count; i++) { var u = list[i]; if (u && u.gameObject.activeInHierarchy && !u.IsDead) c++; }
        return c;
    }

    private static void EnableButtonRaycasts(Button btn, bool on)
    {
        if (!btn) return;
        foreach (var gg in btn.GetComponentsInChildren<Graphic>(true)) gg.raycastTarget = on;
        if (btn.image) btn.image.raycastTarget = on;
    }

    public void RefreshNow() => UpdateCountsNow();
}
