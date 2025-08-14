using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 모든 데이터를 관리하는 매니저 클래스
/// 팩션, 연구, 건물, 요청 등의 데이터를 중앙에서 관리
/// 이벤트, 이펙트, 타일맵은 별도 매니저에서 관리
/// </summary>
public class GameDataManager : MonoBehaviour
{
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
    private List<ResearchData> m_commonResearchDataList = new();
    private List<BuildingData> m_buildingDataList = new();
    private List<RequestLineTemplate> m_requestLineTemplateList = new();
    private List<EventGroupData> m_eventGroupDataList = new();
    private List<TileMapData> m_tileMapDataList = new();

    // 데이터 딕셔너리들
    private readonly Dictionary<FactionType.TYPE, FactionEntry> m_factionEntryDic = new();
    private readonly Dictionary<string, ResearchEntry> m_commonResearchEntryDic = new();
    private readonly Dictionary<string, BuildingEntry> m_buildingEntryDic = new();
    private readonly Dictionary<ResourceType.TYPE, Sprite> m_resourceIconDic = new();
    private readonly Dictionary<TokenType.TYPE, Sprite> m_tokenIconDic = new();
    private readonly Dictionary<RequestType.TYPE, Sprite> m_requestIconDic = new();
    private readonly Dictionary<RequestType.TYPE, RequestLineTemplate> m_requestLineTemplateDic = new();

    // 요청 상태 관리
    private readonly List<RequestState> m_acceptableRequestList = new();
    private readonly List<RequestState> m_acceptedRequestList = new();

    // 게임 시스템 엔트리
    private GameBalanceEntry m_gameBalanceEntry;

    // 프로퍼티들
    public Dictionary<FactionType.TYPE, FactionEntry> FactionEntryDict => m_factionEntryDic;
    public Dictionary<string, ResearchEntry> CommonResearchEntryDict => m_commonResearchEntryDic;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDic;
    public List<RequestState> AcceptableRequestList => m_acceptableRequestList;
    public List<RequestState> AcceptedRequestList => m_acceptedRequestList;
    public GameBalanceEntry GameBalanceEntry => m_gameBalanceEntry;
    public EventManager EventManager => m_eventManager;
    public EffectManager EffectManager => m_effectManager;
    public TileMapManager TileMapManager => m_tileMapManager;

    #region Unity Lifecycle
    void Awake()
    {
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
        InitResearchDict();
        InitBuildingDict();
        InitRequestTemplateDict();
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

    private void InitResearchDict()
    {
        m_commonResearchEntryDic.Clear();
        foreach (ResearchData research in m_commonResearchDataList)
        {
            if (!m_commonResearchEntryDic.ContainsKey(research.m_code))
            {
                m_commonResearchEntryDic.Add(research.m_code, new ResearchEntry(research));
            }
        }
        LockResearch();
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

    private void LockResearch()
    {
        // 먼저 모든 연구를 잠금 해제
        foreach (var research in m_commonResearchDataList)
        {
            if (research.m_unlocks != null)
            {
                foreach (var unlockResearch in research.m_unlocks)
                {
                    if (m_commonResearchEntryDic.TryGetValue(unlockResearch.m_code, out var unlockEntry))
                    {
                        unlockEntry.m_state.m_isLocked = false;
                    }
                }
            }
        }

        // 선행 조건 확인하여 잠금 설정
        foreach (var research in m_commonResearchDataList)
        {
            if (research.m_prerequisites != null && research.m_prerequisites.Count > 0)
            {
                bool allPrerequisitesMet = research.m_prerequisites.All(prereq => 
                    m_commonResearchEntryDic.TryGetValue(prereq.m_code, out var prereqEntry) && 
                    prereqEntry.m_state.m_isResearched);

                if (!allPrerequisitesMet)
                {
                    m_commonResearchEntryDic[research.m_code].m_state.m_isLocked = true;
                }
            }
        }
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
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList, m_commonResearchDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData,
            GetResourcesPath(m_factionDataPath), "Research/Common",
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
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList, m_commonResearchDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData);
    }

    private void LoadAllDataFromPaths()
    {
        DataLoader.LoadAllDataFromPaths(
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList, m_commonResearchDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData,
            m_factionDataPath, "Assets/Datas/Research/Common", m_buildingDataPath, m_requestLineTemplatePath,
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
    #endregion

    #region Data Access
    public FactionEntry GetFactionEntry(FactionType.TYPE argType)
    {
        return m_factionEntryDic.TryGetValue(argType, out var entry) ? entry : null;
    }

    public ResearchEntry GetCommonResearchEntry(string argKey)
    {
        return !string.IsNullOrEmpty(argKey) && m_commonResearchEntryDic.TryGetValue(argKey, out var entry) ? entry : null;
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