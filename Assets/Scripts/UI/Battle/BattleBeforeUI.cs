using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BattleBeforeUI : MonoBehaviour
{
    #region Inspector

    [Header("UI ����")]
    [SerializeField] private GameObject unitBoxPrefab;           // �� ������ ǥ���ϴ� UI ������(=UnitBox)
    [SerializeField] private Button battleStartButton;           // ���� ���� ��ư
    [SerializeField] private Button modeChangeButton;            // ��ġ/ȸ�� ��� ��ȯ ��ư
    [SerializeField] private TextMeshProUGUI modeText;           // ���� ��� ǥ�� �ؽ�Ʈ
    [SerializeField] private Button toggleViewButton;            // ����(�Ʊ�/��) ��� ��ư

    [Header("��� ����")]
    [SerializeField] private RectTransform contentParent;        // UnitBox���� ���� �θ�
    [SerializeField] private float spawnDelay = 0.05f;           // UnitBox ���� �� ��

    [Header("Unit ī����")]
    [SerializeField] private TextMeshProUGUI allyMeleeCountText;
    [SerializeField] private TextMeshProUGUI allyRangeCountText;
    [SerializeField] private TextMeshProUGUI allyDefenseCountText;

    #endregion

    #region State

    // ���� �ڽ� ���� ��ȸ ��
    // key�� ������ unitId ����. �ٸ� ���� ȣȯ ����� unitName ���.
    // �ʿ�� UnitData�� unitId�� �����ϸ� unitId ������� �ٲ㵵 ��.
    private readonly Dictionary<string, UnitBox> unitBoxMap = new();

    // �� ������ ���� UnitsChanged �ݿ� ����(��ġ-ȸ�� ���� Ÿ�̹� ����ϰ�)
    private int _suppressUnitsChangedUntilFrame = -1;

    // ��ġ�� ���� ���� ī����(���� ���� ��)
    private readonly Dictionary<UnitTagType, int> deployedLocal = new()
    {
        { UnitTagType.Melee,   0 },
        { UnitTagType.Range,   0 },
        { UnitTagType.Defense, 0 },
    };

    // ��� ����
    private bool isPlacementMode = true;
    public static bool IsInPlacementMode { get; private set; } = true;

    private CanvasGroup canvasGroup;
    private WaitForSeconds _spawnDelayWait; // GC ���Ҹ� ���� ĳ��

    #endregion

    #region Unity Lifetime

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (battleStartButton) battleStartButton.onClick.AddListener(OnBattleStart);
        if (modeChangeButton) modeChangeButton.onClick.AddListener(OnModeChange);
        if (toggleViewButton) toggleViewButton.onClick.AddListener(OnToggleView);

        // �ڷ�ƾ ������ ĳ��
        _spawnDelayWait = new WaitForSeconds(Mathf.Max(0f, spawnDelay));
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

    #endregion

    #region Public API (�ܺο��� �ʱ�ȭ ȣ��)

    public void InitDeploymentUI(List<UnitData> unitStats)
    {
        // ���� �ڽ� ����
        if (contentParent)
        {
            foreach (Transform c in contentParent)
                Destroy(c.gameObject);
        }
        unitBoxMap.Clear();

        // ���� ī���� �ʱ�ȭ
        deployedLocal[UnitTagType.Melee] = 0;
        deployedLocal[UnitTagType.Range] = 0;
        deployedLocal[UnitTagType.Defense] = 0;

        // ��� ����
        if (unitStats != null)
            StartCoroutine(SpawnUnitBoxes(unitStats));

        UpdateModeUI();
        ShowUI();

        SetCounterTexts();
        RecomputeStartButton();
    }

    /// �Ʊ� ��ġ ���� �� ���� ��ġ ���� �ٽ� �о� ī���� ����
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

    #endregion

    #region Unit List Rendering

    private IEnumerator SpawnUnitBoxes(List<UnitData> list)
    {
        // ���� �̸��� ������ ���� �� �ڽ��� ���� ����
        var map = new Dictionary<string, (UnitData stat, int count)>();
        foreach (var s in list)
        {
            if (!s) continue;
            string key = !string.IsNullOrEmpty(s.unitName) ? s.unitName : s.name; // unitId ������ ����� ��ü ����
            if (map.ContainsKey(key)) map[key] = (s, map[key].count + 1);
            else map[key] = (s, 1);
        }

        foreach (var kv in map.Values)
        {
            AddUnitToUIList(kv.stat, kv.count);
            yield return _spawnDelayWait;
        }
    }

    private void AddUnitToUIList(UnitData stat, int count)
    {
        if (!stat || !unitBoxPrefab || !contentParent) return;

        string key = !string.IsNullOrEmpty(stat.unitName) ? stat.unitName : stat.name;

        if (unitBoxMap.TryGetValue(key, out var box))
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
            unitBoxMap[key] = ub;
        }
        else
        {
            Debug.LogError("[BattleBeforeUI] unitBoxPrefab�� UnitBox ������Ʈ�� �����ϴ�.");
            Destroy(go);
        }
    }

    #endregion

    #region Mode & View Toggle

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

    #endregion

    #region Place / Recall

    /// ���� ��ġ ��û(��ư/�巡�׿��� ȣ��)
    public UnitBase RequestPlaceUnit(UnitData stat)
    {
        if (BattleSystemManager.Instance == null || stat == null) return null;

        string key = !string.IsNullOrEmpty(stat.unitName) ? stat.unitName : stat.name;

        if (!unitBoxMap.TryGetValue(key, out var box) || box.CurrentCount <= 0)
        {
            Debug.LogWarning("[BattleBeforeUI] ���� ������ ���ų� ���� �ڽ��� ã�� �� �����ϴ�.");
            return null;
        }

        // UI ���ݿ�
        box.IncreaseUnitCount(-1);
        SuppressUnitsChangedForOneFrame();

        // ���� ����
        var spawned = BattleSystemManager.Instance.RequestSpawnAlly(stat);

        if (spawned != null)
        {
            // �巡�� �ڵ鷯 �ʱ� ����
            StartCoroutine(InitializeDragHandler(spawned, stat));

            // ī����(+1)
            BumpDeployed(stat.unitTagType, +1);
        }
        else
        {
            Debug.LogWarning("[BattleBeforeUI] ���� ��ȯ ����, ī��Ʈ�� �����մϴ�.");
            box.IncreaseUnitCount(1);
            _suppressUnitsChangedUntilFrame = -1;
        }
        return spawned;
    }

    private IEnumerator InitializeDragHandler(UnitBase spawned, UnitData stat)
    {
        // ���� ���� ������ ������ ���� ����
        yield return new WaitForEndOfFrame();

        var dragHandler = spawned.GetComponent<UnitDragHandler>();
        if (dragHandler)
        {
            var spawnArea = BattleSystemManager.Instance.GetAllySpawnArea();
            dragHandler.SetReferences(stat, spawnArea, this);
        }
        else
        {
            Debug.LogError($"[BattleBeforeUI] {spawned.name}���� UnitDragHandler�� ã�� ���߽��ϴ�.");
        }
    }

    /// ���� ȸ�� ��û(�巡��/��ư)
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

    public void OnUnitRecalled(UnitData stat)
    {
        if (!stat) return;

        string key = !string.IsNullOrEmpty(stat.unitName) ? stat.unitName : stat.name;

        if (unitBoxMap.TryGetValue(key, out var box))
            box.IncreaseUnitCount(1);

        BumpDeployed(stat.unitTagType, -1);
    }

    #endregion

    #region Battle Start

    private void OnBattleStart()
    {
        HideUI();
        BattleSystemManager.Instance?.StartBattle();
    }

    public void ShowUI() => gameObject.SetActive(true);
    public void HideUI() => gameObject.SetActive(false);

    #endregion

    #region Internals (Counters, Buttons, Events)

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

        // ���� ������ ��� ��ġ�ؾ߸� ���� ���� �����ϰ� ����� �ʹٸ� �Ʒ� �ּ��� ����
        // battleStartButton.interactable = (totalRemain == 0);
    }

    private void HandleUnitsChanged()
    {
        if (Time.frameCount <= _suppressUnitsChangedUntilFrame) return;

        UpdateDeployedUnitCounters();
    }

    #endregion
}
