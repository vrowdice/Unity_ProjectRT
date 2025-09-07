using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BattleBeforeUI : MonoBehaviour
{
    #region Inspector

    [Header("UI 참조")]
    [SerializeField] private GameObject unitBoxPrefab;           // 각 유닛을 표시하는 UI 프리팹(=UnitBox)
    [SerializeField] private Button battleStartButton;           // 전투 시작 버튼
    [SerializeField] private Button modeChangeButton;            // 배치/회수 모드 전환 버튼
    [SerializeField] private TextMeshProUGUI modeText;           // 현재 모드 표시 텍스트
    [SerializeField] private Button toggleViewButton;            // 시점(아군/적) 토글 버튼

    [Header("목록 영역")]
    [SerializeField] private RectTransform contentParent;        // UnitBox들이 놓일 부모
    [SerializeField] private float spawnDelay = 0.05f;           // UnitBox 생성 간 텀

    [Header("Unit 카운터")]
    [SerializeField] private TextMeshProUGUI allyMeleeCountText;
    [SerializeField] private TextMeshProUGUI allyRangeCountText;
    [SerializeField] private TextMeshProUGUI allyDefenseCountText;

    #endregion

    #region State

    // 유닛 박스 빠른 조회 맵
    // key는 가급적 unitId 권장. 다만 이전 호환 고려해 unitName 사용.
    // 필요시 UnitData에 unitId가 존재하면 unitId 사용으로 바꿔도 됨.
    private readonly Dictionary<string, UnitBox> unitBoxMap = new();

    // 한 프레임 동안 UnitsChanged 반영 억제(배치-회수 동시 타이밍 깔끔하게)
    private int _suppressUnitsChangedUntilFrame = -1;

    // 배치된 유닛 로컬 카운터(스폰 구역 내)
    private readonly Dictionary<UnitTagType, int> deployedLocal = new()
    {
        { UnitTagType.Melee,   0 },
        { UnitTagType.Range,   0 },
        { UnitTagType.Defense, 0 },
    };

    // 모드 상태
    private bool isPlacementMode = true;
    public static bool IsInPlacementMode { get; private set; } = true;

    private CanvasGroup canvasGroup;
    private WaitForSeconds _spawnDelayWait; // GC 감소를 위한 캐시

    #endregion

    #region Unity Lifetime

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (battleStartButton) battleStartButton.onClick.AddListener(OnBattleStart);
        if (modeChangeButton) modeChangeButton.onClick.AddListener(OnModeChange);
        if (toggleViewButton) toggleViewButton.onClick.AddListener(OnToggleView);

        // 코루틴 딜레이 캐시
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

    #region Public API (외부에서 초기화 호출)

    public void InitDeploymentUI(List<UnitData> unitStats)
    {
        // 기존 박스 정리
        if (contentParent)
        {
            foreach (Transform c in contentParent)
                Destroy(c.gameObject);
        }
        unitBoxMap.Clear();

        // 로컬 카운터 초기화
        deployedLocal[UnitTagType.Melee] = 0;
        deployedLocal[UnitTagType.Range] = 0;
        deployedLocal[UnitTagType.Defense] = 0;

        // 목록 렌더
        if (unitStats != null)
            StartCoroutine(SpawnUnitBoxes(unitStats));

        UpdateModeUI();
        ShowUI();

        SetCounterTexts();
        RecomputeStartButton();
    }

    /// 아군 배치 구역 내 실제 배치 수를 다시 읽어 카운터 갱신
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
        // 같은 이름의 유닛을 묶어 한 박스에 수량 집계
        var map = new Dictionary<string, (UnitData stat, int count)>();
        foreach (var s in list)
        {
            if (!s) continue;
            string key = !string.IsNullOrEmpty(s.unitName) ? s.unitName : s.name; // unitId 있으면 여기로 교체 가능
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
            Debug.LogError("[BattleBeforeUI] unitBoxPrefab에 UnitBox 컴포넌트가 없습니다.");
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

    /// 유닛 배치 요청(버튼/드래그에서 호출)
    public UnitBase RequestPlaceUnit(UnitData stat)
    {
        if (BattleSystemManager.Instance == null || stat == null) return null;

        string key = !string.IsNullOrEmpty(stat.unitName) ? stat.unitName : stat.name;

        if (!unitBoxMap.TryGetValue(key, out var box) || box.CurrentCount <= 0)
        {
            Debug.LogWarning("[BattleBeforeUI] 남은 유닛이 없거나 유닛 박스를 찾을 수 없습니다.");
            return null;
        }

        // UI 선반영
        box.IncreaseUnitCount(-1);
        SuppressUnitsChangedForOneFrame();

        // 실제 스폰
        var spawned = BattleSystemManager.Instance.RequestSpawnAlly(stat);

        if (spawned != null)
        {
            // 드래그 핸들러 초기 설정
            StartCoroutine(InitializeDragHandler(spawned, stat));

            // 카운터(+1)
            BumpDeployed(stat.unitTagType, +1);
        }
        else
        {
            Debug.LogWarning("[BattleBeforeUI] 유닛 소환 실패, 카운트를 복구합니다.");
            box.IncreaseUnitCount(1);
            _suppressUnitsChangedUntilFrame = -1;
        }
        return spawned;
    }

    private IEnumerator InitializeDragHandler(UnitBase spawned, UnitData stat)
    {
        // 스폰 직후 프레임 끝에서 참조 연결
        yield return new WaitForEndOfFrame();

        var dragHandler = spawned.GetComponent<UnitDragHandler>();
        if (dragHandler)
        {
            var spawnArea = BattleSystemManager.Instance.GetAllySpawnArea();
            dragHandler.SetReferences(stat, spawnArea, this);
        }
        else
        {
            Debug.LogError($"[BattleBeforeUI] {spawned.name}에서 UnitDragHandler를 찾지 못했습니다.");
        }
    }

    /// 유닛 회수 요청(드래그/버튼)
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

        // 보유 병력을 모두 배치해야만 전투 시작 가능하게 만들고 싶다면 아래 주석을 해제
        // battleStartButton.interactable = (totalRemain == 0);
    }

    private void HandleUnitsChanged()
    {
        if (Time.frameCount <= _suppressUnitsChangedUntilFrame) return;

        UpdateDeployedUnitCounters();
    }

    #endregion
}
