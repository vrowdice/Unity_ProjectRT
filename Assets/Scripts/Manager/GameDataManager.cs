using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 모든 데이터를 관리하는 매니저 클래스
/// 팩션, 연구, 건물, 이벤트, 요청 등의 데이터를 중앙에서 관리
/// </summary>
public class GameDataManager : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private List<TileMapData> m_tileMapDataList = new();
    [SerializeField] private List<EventGroupData> m_eventGroupDataList = new();
    [SerializeField] private List<FactionData> m_factionDataList = new();
    [SerializeField] private List<ResearchData> m_commonResearchDataList = new();
    [SerializeField] private List<BuildingData> m_buildingDataList = new();
    [SerializeField] private List<RequestLineTemplate> m_requestLineTemplateList = new();
    [SerializeField] private RequestLineTemplate m_contactLineTemplate;

    [Header("Common Data")]
    [SerializeField] private List<ResourceIcon> m_resourceIconList = new();
    [SerializeField] private List<TokenIcon> m_tokenIconList = new();
    [SerializeField] private List<RequestIcon> m_requestIconList = new();
    [SerializeField] private GameBalanceData m_gameBalanceData;

    // 데이터 딕셔너리들
    private readonly Dictionary<TerrainType.TYPE, TileMapData> m_tileMapDataDic = new();
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
    private TileMapState[,] m_tileMap;

    // 게임 시스템 엔트리
    private GameBalanceEntry m_gameBalanceEntry;
    private EventEntry m_eventEntry;

    // 프로퍼티들
    public Dictionary<TerrainType.TYPE, TileMapData> TileMapDataDict => m_tileMapDataDic;
    public Dictionary<FactionType.TYPE, FactionEntry> FactionEntryDict => m_factionEntryDic;
    public Dictionary<string, ResearchEntry> CommonResearchEntryDict => m_commonResearchEntryDic;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDic;
    public List<RequestState> AcceptableRequestList => m_acceptableRequestList;
    public List<RequestState> AcceptedRequestList => m_acceptedRequestList;
    public GameBalanceEntry GameBalanceEntry => m_gameBalanceEntry;
    public EventEntry EventEntry => m_eventEntry;
    public TileMapState[,] TileMap => m_tileMap;

    #region Unity Lifecycle
    void Awake()
    {
        #if UNITY_EDITOR
        if (m_tileMapDataList.Count == 0 || m_factionDataList.Count == 0 || m_buildingDataList.Count == 0)
        {
            AutoLoadData();
        }
        #endif

        InitializeGameData();
    }
    #endregion

    #region Initialization
    private void InitializeGameData()
    {
        InitDict();
        InitIconDict();
        InitBalanceEntry();
        InitEventEntry();
        GenerateTileMap();
    }

    private void InitDict()
    {
        InitTileMapDict();
        InitFactionDict();
        InitResearchDict();
        InitBuildingDict();
        InitRequestTemplateDict();
    }

    private void InitTileMapDict()
    {
        m_tileMapDataDic.Clear();
        foreach (TileMapData tileMap in m_tileMapDataList)
        {
            if (!m_tileMapDataDic.ContainsKey(tileMap.m_terrainType))
            {
                m_tileMapDataDic.Add(tileMap.m_terrainType, tileMap);
            }
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

    private void InitEventEntry()
    {
        m_eventEntry = new EventEntry(m_eventGroupDataList, this);
    }

    /// <summary>
    /// GameBalanceData의 설정을 기반으로 타일맵을 생성합니다.
    /// </summary>
    private void GenerateTileMap()
    {
        if (m_gameBalanceData == null)
        {
            Debug.LogError("GameBalanceData가 설정되지 않았습니다.");
            return;
        }

        Vector2Int mapSize = m_gameBalanceData.m_mapSize;
        int friendlySettleCount = m_gameBalanceData.m_friendlySettle;
        int enemySettleCount = m_gameBalanceData.m_enemySettle;

        // MapDataGenerator를 사용하여 맵 생성
        MapDataGenerator mapGenerator = new MapDataGenerator(
            mapSize,
            m_tileMapDataList,
            TerrainType.TYPE.Settlement, // 친화적 정착지 타입
            friendlySettleCount,
            TerrainType.TYPE.Settlement, // 적대적 정착지 타입 (같은 타입 사용)
            enemySettleCount
        );

        m_tileMap = mapGenerator.GenerateMapData();
    }

    /// <summary>
    /// 지정된 시드로 타일맵을 재생성합니다.
    /// </summary>
    /// <param name="seed">맵 생성에 사용할 시드</param>
    public void RegenerateTileMap(string seed = null)
    {
        if (m_gameBalanceData == null)
        {
            Debug.LogError("GameBalanceData가 설정되지 않았습니다.");
            return;
        }

        Vector2Int mapSize = m_gameBalanceData.m_mapSize;
        int friendlySettleCount = m_gameBalanceData.m_friendlySettle;
        int enemySettleCount = m_gameBalanceData.m_enemySettle;

        // MapDataGenerator를 사용하여 맵 생성 (시드 지정)
        MapDataGenerator mapGenerator = new MapDataGenerator(
            mapSize,
            m_tileMapDataList,
            seed,
            TerrainType.TYPE.Settlement,
            friendlySettleCount,
            TerrainType.TYPE.Settlement,
            enemySettleCount
        );

        m_tileMap = mapGenerator.GenerateMapData();
        Debug.Log($"타일맵이 재생성되었습니다. 크기: {mapSize.x}x{mapSize.y}, 시드: {seed ?? "랜덤"}");
    }

    /// <summary>
    /// 지정된 위치의 타일맵 상태를 반환합니다.
    /// </summary>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    /// <returns>타일맵 상태, 범위를 벗어나면 null</returns>
    public TileMapState GetTileMapState(int x, int y)
    {
        if (m_tileMap == null)
        {
            Debug.LogWarning("타일맵이 생성되지 않았습니다.");
            return null;
        }

        if (x >= 0 && x < m_tileMap.GetLength(0) && y >= 0 && y < m_tileMap.GetLength(1))
        {
            return m_tileMap[x, y];
        }

        Debug.LogWarning($"타일맵 좌표가 범위를 벗어났습니다: ({x}, {y})");
        return null;
    }

    /// <summary>
    /// 타일맵의 크기를 반환합니다.
    /// </summary>
    /// <returns>타일맵 크기 (Vector2Int)</returns>
    public Vector2Int GetTileMapSize()
    {
        if (m_tileMap == null)
        {
            return Vector2Int.zero;
        }

        return new Vector2Int(m_tileMap.GetLength(0), m_tileMap.GetLength(1));
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
        LoadAllDataFromAssets();
        #else
        Debug.LogWarning("Auto data loading is only available in editor.");
        #endif
    }

    public void LoadDataFromResources()
    {
        DataLoader.LoadAllDataFromResources(
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList, m_commonResearchDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData);
    }

    public void LoadMapData()
    {
        #if UNITY_EDITOR
        LoadTileMapDataFromAssets();
        #else
        Debug.LogWarning("Load Map Data is only available in editor.");
        #endif
    }

    #if UNITY_EDITOR
    private void LoadAllDataFromAssets()
    {
        DataLoader.LoadAllDataFromAssets(
            m_tileMapDataList, m_eventGroupDataList, m_factionDataList, m_commonResearchDataList,
            m_buildingDataList, m_requestLineTemplateList, m_resourceIconList,
            m_tokenIconList, m_requestIconList, ref m_gameBalanceData);
    }

    private void LoadTileMapDataFromAssets()
    {
        DataLoader.LoadTileMapDataFromAssets(m_tileMapDataList);
    }
    #endif
    #endregion

    #region Game Actions
    public void RandomBuilding(int buildingCount)
    {
        for (int i = 0; i < buildingCount; i++)
        {
            ProbabilityUtils.GetRandomElement(BuildingEntryDict).Value.m_state.m_amount++;
        }
    }

    public void AddSpecificBuilding(string buildingCode, int count = 1)
    {
        if (string.IsNullOrEmpty(buildingCode))
        {
            Debug.LogWarning("Building code is empty.");
            return;
        }

        if (m_buildingEntryDic.TryGetValue(buildingCode, out var buildingEntry))
        {
            buildingEntry.m_state.m_amount += count;
            Debug.Log($"Building '{buildingCode}' {count} added. Total count: {buildingEntry.m_state.m_amount}");
        }
        else
        {
            Debug.LogWarning($"Building code '{buildingCode}' not found.");
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
    public TileMapData GetTileMapData(TerrainType.TYPE argType)
    {
        return m_tileMapDataDic.TryGetValue(argType, out var data) ? data : null;
    }

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