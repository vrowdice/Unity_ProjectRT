using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private float refreshInterval = 0.25f;

    private BattleSystemManager bsm;
    private Coroutine loop;

    private void Awake()
    {
        // 화면 클릭 방해하지 않도록 기본적으로 모든 Graphic의 raycastTarget 끄기
        // (단, 버튼 하위 Graphic은 다시 켜줌)
        var allGraphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in allGraphics) g.raycastTarget = false;

        EnableButtonRaycasts(helpButton, true);
        EnableButtonRaycasts(settingsButton, true);

        // 전체 루트는 상호작용 필요 없으니 꺼둠(버튼은 자체적으로 켜져 있음)
        var cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        bsm = BattleSystemManager.Instance;
        if (bsm != null) bsm.UnitsChanged += UpdateCountsNow;

        UpdateCountsNow();
        if (loop == null) loop = StartCoroutine(RefreshLoop());
    }

    private void OnDisable()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
        if (bsm != null) bsm.UnitsChanged -= UpdateCountsNow;
    }

    private IEnumerator RefreshLoop()
    {
        var wait = new WaitForSeconds(refreshInterval);
        while (true)
        {
            UpdateCountsNow();
            yield return wait;
        }
    }

    private void UpdateCountsNow()
    {
        if (bsm == null)
        {
            if (enemyCountText) enemyCountText.text = "--";
            if (allyCountText) allyCountText.text = "--";
            return;
        }

        int enemy = CountAlive(bsm.EnemyUnits);
        int ally = CountAlive(bsm.AllyUnits);

        if (enemyCountText) enemyCountText.text = enemy.ToString();
        if (allyCountText) allyCountText.text = ally.ToString();
    }

    private int CountAlive(List<UnitBase> list)
    {
        int c = 0;
        foreach (var u in list)
            if (u && u.gameObject.activeSelf && !u.IsDead) c++;
        return c;
    }

    private void EnableButtonRaycasts(Button btn, bool on)
    {
        if (!btn) return;
        var g = btn.GetComponentsInChildren<Graphic>(true);
        foreach (var gg in g) gg.raycastTarget = on;
    }
}
