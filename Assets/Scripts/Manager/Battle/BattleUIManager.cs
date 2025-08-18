using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    [Header("배치 UI 프리팹")]
    [SerializeField] private GameObject allyUI;
    [SerializeField] private GameObject enemyUI;

    [Header("Top Bar")]
    [SerializeField] private TopStatusBarUI topBarPrefab;
    [SerializeField] private int topBarSortingOrder = 100;

    private BattleBeforeUI allyUIInstance;
    private GameObject enemyUIInstance;
    private BattleBeforeUI enemyUIBattleScript;

    // 캐시
    private TopStatusBarUI topBar;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// 아군 배치 UI 초기화
    public void InitializeUI(List<UnitStatBase> allyUnitStats)
    {
        if (allyUI && !allyUIInstance)
        {
            var go = Instantiate(allyUI, transform);
            allyUIInstance = go.GetComponent<BattleBeforeUI>();
            if (allyUIInstance) allyUIInstance.InitDeploymentUI(allyUnitStats);
            go.SetActive(true);
        }
        else if (allyUIInstance)
        {
            allyUIInstance.InitDeploymentUI(allyUnitStats);
            allyUIInstance.gameObject.SetActive(true);
        }
        HideTopBar(); 
    }

    public void SwitchToAllyUI()
    {
        if (enemyUIInstance) enemyUIInstance.SetActive(false);
        if (enemyUIInstance && enemyUIBattleScript) enemyUIBattleScript.gameObject.SetActive(false);

        if (allyUIInstance)
        {
            allyUIInstance.gameObject.SetActive(true);
            allyUIInstance.UpdateDeployedUnitCounters();
        }
    }
    public void SwitchToEnemyUI()
    {
        if (!enemyUIInstance && enemyUI)
        {
            enemyUIInstance = Instantiate(enemyUI, transform);
            enemyUIBattleScript = enemyUIInstance.GetComponent<BattleBeforeUI>() ??
                                  enemyUIInstance.GetComponentInChildren<BattleBeforeUI>();
            enemyUIInstance.SetActive(false);
        }
        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);
        if (enemyUIInstance)
        {
            enemyUIInstance.SetActive(true);
            enemyUIBattleScript?.UpdateDeployedUnitCounters();
        }
    }

    public void HideAllUI()
    {
        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);
        if (enemyUIInstance) enemyUIInstance.SetActive(false);
    }
    public void HideAllUI(bool exceptTopBar)
    {
        HideAllUI();
        if (!exceptTopBar) HideTopBar();
    }

    public void HideTopBar()
    {
        if (topBar == null) topBar = FindObjectOfType<TopStatusBarUI>(true);
        if (topBar != null) topBar.gameObject.SetActive(false);
    }

    public void ShowTopBar()
    {
        if (EnsureTopBar(createIfMissing: true))
            topBar.gameObject.SetActive(true);
    }

    private bool EnsureTopBar(bool createIfMissing)
    {
        if (topBar == null)
            topBar = FindObjectOfType<TopStatusBarUI>(true);

        if (topBar == null && createIfMissing && topBarPrefab != null)
            topBar = Instantiate(topBarPrefab, transform);

        if (topBar == null) return false;

        var cg = topBar.GetComponent<CanvasGroup>();
        if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }

        var canvas = topBar.GetComponentInParent<Canvas>();
        if (canvas) { canvas.overrideSorting = true; canvas.sortingOrder = topBarSortingOrder; }

        if (topBar.transform is RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
        }
        return true;
    }
}
