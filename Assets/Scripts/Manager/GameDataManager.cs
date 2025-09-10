using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 모든 데이터를 관리하는 매니저 클래스
/// 팩션, 연구, 건물, 요청 등의 데이터를 중앙에서 관리
/// 이벤트, 이펙트, 타일맵은 별도 매니저에서 관리
/// 싱글톤 패턴으로 구현되어 전역에서 접근 가능
/// </summary>
public class GameDataManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameDataManager Instance { get; private set; }
    
    private EventManager m_eventManager;
    private EffectManager m_effectManager;
    private TileMapManager m_tileMapManager;
    
    [Header("Data Paths")]
    [SerializeField] private string m_factionDataPath = "Assets/Datas/Faction";
    [SerializeField] private string m_buildingDataPath = "Assets/Datas/Building";
    [SerializeField] private string m_requestLineTemplatePath = "Assets/Datas/RequestLineTemplate";
    [SerializeField] private string m_eventGroupDataPath = "Assets/Datas/Event/EventGroup";
    [SerializeField] private string m_tileMapDataPath = "Assets/Datas/TileMap";
    
    [Header("Common Data (Manual Setup)")]
    [SerializeField] private List<ResourceIcon> m_resourceIconList = new();
    [SerializeField] private List<TokenIcon> m_tokenIconList = new();
    [SerializeField] private List<RequestIcon> m_requestIconList = new();
    [SerializeField] private GameBalanceData m_gameBalanceData;
    [SerializeField] private RequestLineTemplate m_contactLineTemplate;

    // 자동 로딩되는 데이터들 (인스펙터에서 숨김)
    private List<FactionData> m_factionDataList = new();
    private List<BuildingData> m_buildingDataList = new();
    private List<RequestLineTemplate> m_requestLineTemplateList = new();
    private List<EventGroupData> m_eventGroupDataList = new();
    private List<TileMapData> m_tileMapDataList = new();

    // 데이터 딕셔너리들
    private readonly Dictionary<FactionType.TYPE, FactionEntry> m_factionEntryDic = new();
    private readonly Dictionary<string, BuildingEntry> m_buildingEntryDic = new();
    private readonly Dictionary<ResourceType.TYPE, Sprite> m_resourceIconDic = new();
    private readonly Dictionary<TokenType.TYPE, Sprite> m_tokenIconDic = new();
    private readonly Dictionary<RequestType.TYPE, Sprite> m_requestIconDic = new();
    private readonly Dictionary<RequestType.TYPE, RequestLineTemplate> m_requestLineTemplateDic = new();

    // 팩션별 연구 및 유닛 딕셔너리들
    private readonly Dictionary<string, FactionResearchEntry> m_factionResearchEntryDic = new();
    private readonly Dictionary<string, FactionUnitEntry> m_factionUnitEntryDic = new();

    // 유닛 데이터 관리 -추가함
    private readonly Dictionary<string, UnitData> m_unitByKey = new();
    private readonly Dictionary<FactionType.TYPE, List<UnitData>> m_unitsByFaction = new();
    private readonly Dictionary<UnitTagType, List<UnitData>> m_unitsByTag = new();

    // 요청 상태 관리
    private readonly List<RequestState> m_acceptableRequestList = new();
    private readonly List<RequestState> m_acceptedRequestList = new();

    // 유닛 데이터 관리 -추가함
    private readonly List<UnitData> m_unitDataList = new();

    // 읽기 전용 접근자
    public IReadOnlyList<UnitData> AllUnits => m_unitDataList;

    // 게임 시스템 엔트리
    private GameBalanceEntry m_gameBalanceEntry;

    // 프로퍼티들
    public Dictionary<FactionType.TYPE, FactionEntry> FactionEntryDict => m_factionEntryDic;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDic;
    public Dictionary<string, FactionResearchEntry> FactionResearchEntryDict => m_factionResearchEntryDic;
    public Dictionary<string, FactionUnitEntry> FactionUnitEntryDict => m_factionUnitEntryDic;
    public List<RequestState> AcceptableRequestList => m_acceptableRequestList;
    public List<RequestState> AcceptedRequestList => m_acceptedRequestList;
    public GameBalanceEntry GameBalanceEntry => m_gameBalanceEntry;
    public EventManager EventManager => m_eventManager;
    public EffectManager EffectManager => m_effectManager;
    public TileMapManager TileMapManager => m_tileMapManager;


    #region Unity Lifecycle
    void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 게임 시작 시 자동으로 데이터 로딩
        AutoLoadData();
        InitializeGameData();
    }
    #endregion

    #region Initialization
    private void InitializeGameData()
    {
        InitDict();
        InitIconDict();
        InitBalanceEntry();
        InitializeManagers();
    }

    private void InitDict()
    {
        InitFactionDict();
        InitBuildingDict();
        InitRequestTemplateDict();
        InitFactionResearchDict();
        InitFactionUnitDict();

        // FactionData의 Units를 이용해 Unit 인덱스 구성 -추가함 (기존 로직 유지)
        BuildUnitsFromFactions();
    }

    // FactionData의 m_units 리스트를 순회하며 유닛 데이터 수집 및 인덱스 구성 - 기존 로직 유지
    private void BuildUnitsFromFactions()
    {
        m_unitDataList.Clear();
        m_unitByKey.Clear();
        m_unitsByFaction.Clear();
        m_unitsByTag.Clear();

        // m_factionDataList 는 기존 AutoLoadData()에서 채워짐
        foreach (var faction in m_factionDataList)
        {
            if (faction == null || faction.m_units == null) continue;

            foreach (var unit in faction.m_units)
            {
                if (!unit) continue;

                // 보관 (중복 방지)
                if (!m_unitDataList.Contains(unit))
                    m_unitDataList.Add(unit);

                // unitKey 인덱스
                if (!string.IsNullOrEmpty(unit.unitKey))
                {
                    if (!m_unitByKey.ContainsKey(unit.unitKey))
                        m_unitByKey.Add(unit.unitKey, unit);
                    else
                        Debug.LogWarning($"[GameDataManager] Duplicate unitKey '{unit.unitKey}' detected. First one kept.");
                }

                // 팩션 인덱스 (FactionData 기준으로 묶기)
                var fType = faction.m_factionType; // FactionData의 타입 사용
                if (!m_unitsByFaction.TryGetValue(fType, out var listByFaction))
                {
                    listByFaction = new List<UnitData>();
                    m_unitsByFaction.Add(fType, listByFaction);
                }
                if (!listByFaction.Contains(unit))
                    listByFaction.Add(unit);

                // 태그 인덱스
                if (!m_unitsByTag.TryGetValue(unit.unitTagType, out var listByTag))
                {
                    listByTag = new List<UnitData>();
                    m_unitsByTag.Add(unit.unitTagType, listByTag);
                }
                if (!listByTag.Contains(unit))
                    listByTag.Add(unit);

                // (선택) 데이터 일관성 체크: 유닛에 적힌 factionType과 FactionData의 타입이 다르면 경고
                if (unit.factionType != FactionType.TYPE.None && unit.factionType != fType)
                {
                    Debug.LogWarning($"[GameDataManager] Unit '{unit.unitName}' faction mismatch: UnitData={unit.factionType}, FactionData={fType}");
                }
            }
        }

        Debug.Log($"[GameDataManager] Units indexed from FactionData: {m_unitDataList.Count}");
    }
    private void InitializeManagers()
    {
        // 필수 매니저들 자동 생성 및 초기화
        m_eventManager = gameObject.AddComponent<EventManager>();
        m_effectManager = gameObject.AddComponent<EffectManager>();
        m_tileMapManager = gameObject.AddComponent<TileMapManager>();

        // 각 매니저에게 필요한 데이터 할당
        AssignDataToManagers();

        m_eventManager.Initialize(this);
        m_effectManager.Initialize(this);
        m_tileMapManager.Initialize(this);
    }

    private void AssignDataToManagers()
    {
        // EventManager에 이벤트 데이터 할당
        if (m_eventManager != null)
        {
            m_eventManager.SetEventGroupDataList(m_eventGroupDataList);
        }

        // TileMapManager에 타일맵 데이터 할당
        if (m_tileMapManager != null)
        {
            m_tileMapManager.SetTileMapDataList(m_tileMapDataList);
        }
    }

    private void InitFactionDict()
    {
        m_factionEntryDic.Clear();
        foreach (FactionData faction in m_factionDataList)
        {
            if (!m_factionEntryDic.ContainsKey(faction.m_factionType))
            {
                m_factionEntryDic.Add(faction.m_factionType, new FactionEntry(faction));
            }
        }
    }



    private void InitBuildingDict()
    {
        m_buildingEntryDic.Clear();
        foreach (BuildingData building in m_buildingDataList)
        {
            if (!m_buildingEntryDic.ContainsKey(building.m_code))
            {
                var buildingEntry = new BuildingEntry(building);
                // Set initial amount from BuildingData
                buildingEntry.m_state.m_amount = building.m_initialAmount;
                m_buildingEntryDic.Add(building.m_code, buildingEntry);
            }
        }
    }

    private void InitRequestTemplateDict()
    {
        m_requestLineTemplateDic.Clear();
        foreach (RequestLineTemplate item in m_requestLineTemplateList)
        {
            if (!m_requestLineTemplateDic.ContainsKey(item.m_type))
            {
                m_requestLineTemplateDic.Add(item.m_type, item);
            }
        }
    }

    private void InitFactionResearchDict()
    {
        m_factionResearchEntryDic.Clear();
        foreach (FactionData faction in m_factionDataList)
        {
            if (faction.m_research != null)
            {
                foreach (FactionResearchData research in faction.m_research)
                {
                    if (research != null && !string.IsNullOrEmpty(research.m_code))
                    {
                        if (!m_factionResearchEntryDic.ContainsKey(research.m_code))
                        {
                            m_factionResearchEntryDic.Add(research.m_code, new FactionResearchEntry(research));
                        }
                    }
                }
            }
        }
    }

    private void InitFactionUnitDict()
    {
        m_factionUnitEntryDic.Clear();
        foreach (FactionData faction in m_factionDataList)
        {
            if (faction.m_units != null)
            {
                foreach (UnitData unit in faction.m_units)
                {
                    if (unit != null && !string.IsNullOrEmpty(unit.unitKey))
                    {
                        if (!m_factionUnitEntryDic.ContainsKey(unit.unitKey))
                        {
                            m_factionUnitEntryDic.Add(unit.unitKey, new FactionUnitEntry(unit));
                        }
                    }
                }
            }
        }
    }

    private void InitIconDict()
    {
        InitIconDictionary(m_resourceIconList, m_resourceIconDic, icon => icon.m_type, icon => icon.m_icon);
        InitIconDictionary(m_tokenIconList, m_tokenIconDic, icon => icon.m_type, icon => icon.m_icon);
        InitIconDictionary(m_requestIconList, m_requestIconDic, icon => icon.m_type, icon => icon.m_icon);
    }

    private void InitIconDictionary<T, TKey>(List<T> iconList, Dictionary<TKey, Sprite> iconDict, 
        System.Func<T, TKey> keySelector, System.Func<T, Sprite> iconSelector)
    {
        iconDict.Clear();
        foreach (var entry in iconList)
        {
            var key = keySelector(entry);
            if (!iconDict.ContainsKey(key))
            {
                iconDict.Add(key, iconSelector(entry));
            }
        }
    }

    private void InitBalanceEntry()
    {
        m_gameBalanceData.InitializeDict();
        m_gameBalanceEntry = new GameBalanceEntry(m_gameBalanceData, new GameBalanceState());
        
        var firstBalance = GameBalanceEntry.m_data.GetBalanceTypeBalance(GameBalanceEntry.m_data.m_firstBalanceType);
        GameBalanceEntry.m_state.m_mainMul = firstBalance.m_mul;
        GameBalanceEntry.m_state.m_dateMul = 1.0f;
    }


    #endregion

    #region Data Loading
    public void AutoLoadData()
    {
        #if UNITY_EDITOR
        LoadAllDataFromPaths();
        #else
        // 빌드에서는 Resources에서 로딩
        LoadDataFromResources();
        #endif
    }

    public void LoadDataFromResources()
    {
        DataLoader.LoadAllDataFromResources(
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData,
            GetResourcesPath(m_factionDataPath),
            GetResourcesPath(m_buildingDataPath), GetResourcesPath(m_requestLineTemplatePath),
            GetResourcesPath(m_eventGroupDataPath), GetResourcesPath(m_tileMapDataPath));
    }

    private string GetResourcesPath(string assetPath)
    {
        // Assets/Resources/ 경로를 Resources/ 경로로 변환
        if (assetPath.StartsWith("Assets/Resources/"))
        {
            return assetPath.Substring("Assets/Resources/".Length);
        }
        else if (assetPath.StartsWith("Assets/"))
        {
            return assetPath.Substring("Assets/".Length);
        }
        return assetPath;
    }

    #if UNITY_EDITOR
    private void LoadAllDataFromAssets()
    {
        DataLoader.LoadAllDataFromAssets(
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData);
    }

    private void LoadAllDataFromPaths()
    {
        DataLoader.LoadAllDataFromPaths(
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData,
            m_factionDataPath, m_buildingDataPath, m_requestLineTemplatePath,
            m_eventGroupDataPath, m_tileMapDataPath);
    }
    #endif
    #endregion

    #region Game Actions
    public List<string> RandomBuilding(int buildingCount)
    {
        List<string> addedBuildings = new List<string>();
        
        for (int i = 0; i < buildingCount; i++)
        {
            var randomBuilding = ProbabilityUtils.GetRandomElement(BuildingEntryDict);
            if (randomBuilding.Value != null)
            {
                randomBuilding.Value.m_state.m_amount++;
                addedBuildings.Add(randomBuilding.Key);
            }
        }
        
        return addedBuildings;
    }

    public List<string> RandomUnlockedBuilding(int buildingCount)
    {
        List<string> addedBuildings = new List<string>();
        
        var unlockedBuildings = BuildingEntryDict.Where(kvp => kvp.Value.m_state.m_isUnlocked).ToList();
        
        for (int i = 0; i < buildingCount && unlockedBuildings.Count > 0; i++)
        {
            var randomBuilding = ProbabilityUtils.GetRandomElement(unlockedBuildings);
            if (randomBuilding.Value != null)
            {
                randomBuilding.Value.m_state.m_amount++;
                addedBuildings.Add(randomBuilding.Key);
            }
        }
        
        return addedBuildings;
    }

    public bool AddSpecificBuilding(string buildingCode, int count = 1)
    {
        if (string.IsNullOrEmpty(buildingCode))
        {
            Debug.LogWarning("Building code is empty.");
            return false;
        }

        if (m_buildingEntryDic.TryGetValue(buildingCode, out var buildingEntry))
        {
            buildingEntry.m_state.m_amount += count;
            Debug.Log($"Building '{buildingCode}' {count} added. Total count: {buildingEntry.m_state.m_amount}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Building code '{buildingCode}' not found.");
            return false;
        }
    }

    public List<string> UnlockRandomBuilding(int unlockCount)
    {
        List<string> unlockedBuildings = new List<string>();
        
        var lockedBuildings = BuildingEntryDict.Where(kvp => !kvp.Value.m_state.m_isUnlocked).ToList();
        
        for (int i = 0; i < unlockCount && lockedBuildings.Count > 0; i++)
        {
            var randomBuilding = ProbabilityUtils.GetRandomElement(lockedBuildings);
            if (randomBuilding.Value != null)
            {
                randomBuilding.Value.m_state.m_isUnlocked = true;
                unlockedBuildings.Add(randomBuilding.Key);
                lockedBuildings.Remove(randomBuilding);
            }
        }
        
        return unlockedBuildings;
    }

    public bool UnlockSpecificBuilding(string buildingCode, int unlockCount = 1)
    {
        if (string.IsNullOrEmpty(buildingCode))
        {
            Debug.LogWarning("Building code is empty.");
            return false;
        }

        if (m_buildingEntryDic.TryGetValue(buildingCode, out var buildingEntry))
        {
            if (!buildingEntry.m_state.m_isUnlocked)
            {
                buildingEntry.m_state.m_isUnlocked = true;
                Debug.Log($"Building '{buildingCode}' unlocked.");
                return true;
            }
            else
            {
                Debug.LogWarning($"Building '{buildingCode}' is already unlocked.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Building code '{buildingCode}' not found.");
            return false;
        }
    }

    public void MakeRandomRequest()
    {
        RequestGenerator.MakeRandomRequest(
            m_acceptableRequestList, m_factionEntryDic, m_gameBalanceEntry,
            m_contactLineTemplate, m_requestLineTemplateDic);
    }

    public void ForceContactRequest()
    {
        RequestGenerator.GenerateContactRequests(
            m_acceptableRequestList, m_factionEntryDic, m_gameBalanceEntry, m_contactLineTemplate);
    }

    public void AcceptRequest(RequestState request)
    {
        if (m_acceptableRequestList.Contains(request))
        {
            m_acceptedRequestList.Add(request);
            m_acceptableRequestList.Remove(request);
        }
    }

    public void RefuseRequest(RequestState request)
    {
        if (m_acceptableRequestList.Contains(request))
        {
            // 요청을 거절하면 팩션 호감도가 감소
            if (request.m_factionType != FactionType.TYPE.None)
            {
                var factionEntry = GetFactionEntry(request.m_factionType);
                if (factionEntry != null)
                {
                    // 요청 거절 시 호감도 절반만큼 감소 (또는 고정값)
                    int likeDecrease = Mathf.Max(1, request.m_factionAddLike / 2);
                    factionEntry.m_state.m_like = Mathf.Max(0, factionEntry.m_state.m_like - likeDecrease);
                    Debug.Log($"Faction {request.m_factionType} like decreased by {likeDecrease}. Current like: {factionEntry.m_state.m_like}");
                }
            }
            
            // 수락 가능한 요청 목록에서 제거
            m_acceptableRequestList.Remove(request);
        }
    }

    /// <summary>
    /// 팩션 우호도 변경 시 연구 잠금 상태 업데이트
    /// </summary>
    /// <param name="factionType">변경된 팩션 타입</param>
    public void UpdateResearchByFactionLike(FactionType.TYPE factionType)
    {
        // 모든 연구에서 해당 팩션 타입의 연구들 확인
        foreach (var researchKvp in m_factionResearchEntryDic)
        {
            var researchEntry = researchKvp.Value;
            if (researchEntry.m_data.m_factionType == factionType)
            {
                bool shouldUnlock = researchEntry.m_data.CheckFactionLikeRequirement(this);
                researchEntry.m_state.m_isLocked = !shouldUnlock;
            }
        }
    }
    #endregion

    #region Data Access
    public FactionEntry GetFactionEntry(FactionType.TYPE argType)
    {
        return m_factionEntryDic.TryGetValue(argType, out var entry) ? entry : null;
    }

    /// <summary>
    /// 특정 연구 코드의 데이터와 상태 찾기
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <returns>연구 엔트리 (없으면 null)</returns>
    public FactionResearchEntry GetResearchEntry(string researchCode)
    {
        if (string.IsNullOrEmpty(researchCode)) return null;
        return m_factionResearchEntryDic.TryGetValue(researchCode, out var entry) ? entry : null;
    }

    /// <summary>
    /// 특정 유닛 키의 데이터와 상태 찾기
    /// </summary>
    /// <param name="unitKey">유닛 키</param>
    /// <returns>유닛 엔트리 (없으면 null)</returns>
    public FactionUnitEntry GetUnitEntry(string unitKey)
    {
        if (string.IsNullOrEmpty(unitKey)) return null;
        return m_factionUnitEntryDic.TryGetValue(unitKey, out var entry) ? entry : null;
    }

    public BuildingEntry GetBuildingEntry(string argKey)
    {
        return !string.IsNullOrEmpty(argKey) && m_buildingEntryDic.TryGetValue(argKey, out var entry) ? entry : null;
    }

    public RequestLineTemplate GetRequestLineTemplate(RequestType.TYPE argType)
    {
        return m_requestLineTemplateDic.TryGetValue(argType, out var item) ? item : null;
    }

    public Sprite GetResourceIcon(ResourceType.TYPE type)
    {
        return m_resourceIconDic.TryGetValue(type, out var icon) ? icon : null;
    }

    public Sprite GetTokenIcon(TokenType.TYPE type)
    {
        return m_tokenIconDic.TryGetValue(type, out var icon) ? icon : null;
    }

    public Sprite GetRequestIcon(RequestType.TYPE type)
    {
        return m_requestIconDic.TryGetValue(type, out var icon) ? icon : null;
    }
    #endregion

}

#region Icon Structures
[System.Serializable]
public struct ResourceIcon
{
    public ResourceType.TYPE m_type;
    public Sprite m_icon;
}

[System.Serializable]
public struct TokenIcon
{
    public TokenType.TYPE m_type;
    public Sprite m_icon;
}

[System.Serializable]
public struct RequestIcon
{
    public RequestType.TYPE m_type;
    public Sprite m_icon;
}
#endregion