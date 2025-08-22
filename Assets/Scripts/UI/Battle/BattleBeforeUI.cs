using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleBeforeUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject unitBoxPrefab;
    [SerializeField] private Button battleStartButton;
    [SerializeField] private Button modeChangeButton;
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private Button toggleViewButton;

    [Header("목록 영역")]
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private float spawnDelay = 0.05f;

    [Header("Unit 카운터")]
    [SerializeField] private TextMeshProUGUI allyMeleeCountText;
    [SerializeField] private TextMeshProUGUI allyRangeCountText;
    [SerializeField] private TextMeshProUGUI allyDefenseCountText;

    private readonly Dictionary<string, UnitBox> unitBoxMap = new();

    private int _suppressUnitsChangedUntilFrame = -1;

    private readonly Dictionary<UnitTagType, int> deployedLocal = new()
    {
        { UnitTagType.Melee,   0 },
        { UnitTagType.Range,   0 },
        { UnitTagType.Defense, 0 },
    };

    private bool isPlacementMode = true;
    public static bool IsInPlacementMode { get; private set; } = true;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (battleStartButton) battleStartButton.onClick.AddListener(OnBattleStart);
        if (modeChangeButton) modeChangeButton.onClick.AddListener(OnModeChange);
        if (toggleViewButton) toggleViewButton.onClick.AddListener(OnToggleView);
    }

    private void OnEnable()
    {
        var mgr = BattleSystemManager.Instance;
        if (mgr != null) mgr.UnitsChanged += HandleUnitsChanged;

        SetCounterTexts();
        RecomputeStartButton();
    }

    private void OnDisable()
    {
        var mgr = BattleSystemManager.Instance;
        if (mgr != null) mgr.UnitsChanged -= HandleUnitsChanged;
    }

    public void InitDeploymentUI(List<UnitStatBase> unitStats)
    {
        if (contentParent)
        {
            foreach (Transform c in contentParent) Destroy(c.gameObject);
        }
        unitBoxMap.Clear();

        // 로컬 카운터 초기화
        deployedLocal[UnitTagType.Melee] = 0;
        deployedLocal[UnitTagType.Range] = 0;
        deployedLocal[UnitTagType.Defense] = 0;

        if (unitStats != null)
            StartCoroutine(SpawnUnitBoxes(unitStats));

        UpdateModeUI();
        ShowUI();

        SetCounterTexts();
        RecomputeStartButton();
    }

    public void UpdateDeployedUnitCounters()
    {
        var mgr = BattleSystemManager.Instance;
        if (mgr == null) return;

        var counts = mgr.GetCountsInSpawnAreas();

        deployedLocal[UnitTagType.Melee] = (counts != null && counts.TryGetValue(UnitTagType.Melee, out var m)) ? m : 0;
        deployedLocal[UnitTagType.Range] = (counts != null && counts.TryGetValue(UnitTagType.Range, out var r)) ? r : 0;
        deployedLocal[UnitTagType.Defense] = (counts != null && counts.TryGetValue(UnitTagType.Defense, out var d)) ? d : 0;

        SetCounterTexts();
        RecomputeStartButton();
    }

    private IEnumerator SpawnUnitBoxes(List<UnitStatBase> list)
    {
        var map = new Dictionary<string, (UnitStatBase stat, int count)>();
        foreach (var s in list)
        {
            if (!s) continue;
            if (map.ContainsKey(s.unitName)) map[s.unitName] = (s, map[s.unitName].count + 1);
            else map[s.unitName] = (s, 1);
        }

        foreach (var kv in map.Values)
        {
            AddUnitToUIList(kv.stat, kv.count);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void AddUnitToUIList(UnitStatBase stat, int count)
    {
        if (!stat || !unitBoxPrefab || !contentParent) return;

        if (unitBoxMap.TryGetValue(stat.unitName, out var box))
        {
            box.IncreaseUnitCount(count);
            return;
        }

        var go = Instantiate(unitBoxPrefab, contentParent);
        go.transform.localScale = Vector3.one;

        var ub = go.GetComponent<UnitBox>();
        if (ub)
        {
            ub.Init(stat, count, this);
            unitBoxMap[stat.unitName] = ub;
        }
    }

    private void OnModeChange()
    {
        isPlacementMode = !isPlacementMode;
        IsInPlacementMode = isPlacementMode;
        UpdateModeUI();

        SetCounterTexts();
        RecomputeStartButton();
    }

    private void UpdateModeUI()
    {
        if (modeText) modeText.text = isPlacementMode ? "Placement mode" : "Recovery mode";
        if (modeChangeButton)
        {
            var img = modeChangeButton.GetComponent<Image>();
            if (img) img.color = isPlacementMode ? new Color(0.2f, 0.6f, 1.0f) : new Color(1.0f, 0.8f, 0.2f);
        }
    }

    private void OnToggleView()
    {
        BattleSystemManager.Instance?.ToggleView();

        SetCounterTexts();
        RecomputeStartButton();
    }

    // 배치 / 회수
    public UnitBase RequestPlaceUnit(UnitStatBase stat)
    {
        if (BattleSystemManager.Instance == null || stat == null) return null;

        if (!unitBoxMap.TryGetValue(stat.unitName, out var box) || box.CurrentCount <= 0)
        {
            Debug.LogWarning("남은 유닛이 없거나 유닛 박스를 찾을 수 없습니다.");
            return null;
        }

        box.IncreaseUnitCount(-1);

        SuppressUnitsChangedForOneFrame();

        var spawned = BattleSystemManager.Instance.RequestSpawnAlly(stat);

        if (spawned != null)
        {
            StartCoroutine(InitializeDragHandler(spawned, stat));

            BumpDeployed(stat.unitTagType, +1);
        }
        else
        {
            Debug.LogWarning("유닛 소환 실패, 카운트를 복구합니다.");
            box.IncreaseUnitCount(1);
            _suppressUnitsChangedUntilFrame = -1; 
        }
        return spawned;
    }

    private IEnumerator InitializeDragHandler(UnitBase spawned, UnitStatBase stat)
    {
        yield return new WaitForEndOfFrame();

        var dragHandler = spawned.GetComponent<UnitDragHandler>();
        if (dragHandler)
        {
            var spawnArea = BattleSystemManager.Instance.GetAllySpawnArea();
            dragHandler.SetReferences(stat, spawnArea, this);
        }
        else
        {
            Debug.LogError($"[DragHandler] {spawned.name} 유닛에서 UnitDragHandler를 찾을 수 없습니다.");
        }
    }

    public bool RequestRecallUnit(UnitBase unit)
    {
        if (!unit || !BattleSystemManager.Instance) return false;

        SuppressUnitsChangedForOneFrame();

        bool ok = BattleSystemManager.Instance.RecallAlly(unit);
        if (ok)
        {
            OnUnitRecalled(unit.UnitStat);
        }
        return ok;
    }

    /// 드래그핸들러/버튼 등에서 회수 시 ui를 먼저 즉시 갱신하기 위해 호출.
    public void OnUnitRecalled(UnitStatBase stat)
    {
        if (!stat) return;

        if (unitBoxMap.TryGetValue(stat.unitName, out var box))
            box.IncreaseUnitCount(1); 

        BumpDeployed(stat.unitTagType, -1);   
    }

    // 전투 시작

    private void OnBattleStart()
    {
        HideUI();
        BattleSystemManager.Instance?.StartBattle();
    }

    public void ShowUI() => gameObject.SetActive(true);
    public void HideUI() => gameObject.SetActive(false);

    // 내부 유틸

    public void SuppressUnitsChangedForOneFrame()
    {
        _suppressUnitsChangedUntilFrame = Time.frameCount + 1;
    }

    private void BumpDeployed(UnitTagType tag, int delta)
    {
        if (!deployedLocal.ContainsKey(tag)) return;

        deployedLocal[tag] = Mathf.Max(0, deployedLocal[tag] + delta);

        SetCounterTexts();    
        RecomputeStartButton();
    }

    private void SetCounterTexts()
    {
        WriteAndFlush(allyMeleeCountText, deployedLocal[UnitTagType.Melee]);
        WriteAndFlush(allyRangeCountText, deployedLocal[UnitTagType.Range]);
        WriteAndFlush(allyDefenseCountText, deployedLocal[UnitTagType.Defense]);
    }

    private static void WriteAndFlush(TextMeshProUGUI t, int v)
    {
        if (!t) return;
        t.SetText("{0}", v);
        t.ForceMeshUpdate();        
        Canvas.ForceUpdateCanvases();
    }

    private void RecomputeStartButton()
    {
        if (!battleStartButton) return;

        int totalRemain = 0;
        foreach (var kv in unitBoxMap)
        {
            var box = kv.Value;
            if (!box) continue;
            totalRemain += Mathf.Max(0, box.CurrentCount);
        }

        battleStartButton.interactable = (totalRemain == 0);
    }

    private void HandleUnitsChanged()
    {
        if (Time.frameCount <= _suppressUnitsChangedUntilFrame) return;

        UpdateDeployedUnitCounters();
    }
}
