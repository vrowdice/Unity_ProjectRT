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
    /// <summary>
    /// 리소스 타입과 아이콘을 연결하는 구조체
    /// </summary>
    [System.Serializable]
    public struct ResourceIcon
    {
        public ResourceType.TYPE m_type;
        public Sprite m_icon;
    }
    
    /// <summary>
    /// 토큰 타입과 아이콘을 연결하는 구조체
    /// </summary>
    [System.Serializable]
    public struct TokenIcon
    {
        public TokenType.TYPE m_type;
        public Sprite m_icon;
    }
    
    /// <summary>
    /// 요청 타입과 아이콘을 연결하는 구조체
    /// </summary>
    [System.Serializable]
    public struct RequestIcon
    {
        public RequestType.TYPE m_type;
        public Sprite m_icon;
    }

    [Header("Game Data")]
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

    // 데이터 딕셔너리들 - 빠른 검색을 위해 사용
    private readonly Dictionary<FactionType.TYPE, FactionEntry> m_factionEntryDic = new();
    private readonly Dictionary<string, ResearchEntry> m_commonResearchEntryDic = new();
    private readonly Dictionary<string, BuildingEntry> m_buildingEntryDic = new();
    private readonly Dictionary<ResourceType.TYPE, Sprite> m_resourceIconDic = new();
    private readonly Dictionary<TokenType.TYPE, Sprite> m_tokenIconDic = new();
    private readonly Dictionary<RequestType.TYPE, Sprite> m_requestIconDic = new();
    private readonly Dictionary<RequestType.TYPE, RequestLineTemplate> m_requestLineTemplateDic = new();

    // 요청 상태 관리 리스트
    private readonly List<RequestState> m_acceptableRequestList = new();
    private readonly List<RequestState> m_acceptedRequestList = new();

    // 게임 밸런스 및 이벤트 엔트리
    private GameBalanceEntry m_gameBalanceEntry;
    private EventEntry m_eventEntry;

    // 프로퍼티들 - 외부에서 접근 가능
    public Dictionary<FactionType.TYPE, FactionEntry> FactionEntryDict => m_factionEntryDic;
    public Dictionary<string, ResearchEntry> CommonResearchEntryDict => m_commonResearchEntryDic;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDic;
    public List<RequestState> AcceptableRequestList => m_acceptableRequestList;
    public List<RequestState> AcceptedRequestList => m_acceptedRequestList;
    public GameBalanceEntry GameBalanceEntry => m_gameBalanceEntry;
    public EventEntry EventEntry => m_eventEntry;

    /// <summary>
    /// 게임 시작 시 모든 딕셔너리와 데이터를 초기화
    /// </summary>
    void Awake()
    {
        // 에디터에서 데이터가 설정되지 않았다면 자동으로 로드
        #if UNITY_EDITOR
        if (m_factionDataList.Count == 0 || m_buildingDataList.Count == 0)
        {
            AutoLoadData();
        }
        #endif

        InitDict();
        InitIconDict();
        InitBalanceEntry();
        InitEventEntry();
    }

    /// <summary>
    /// 에디터에서 자동으로 데이터를 로드하는 기능
    /// 런타임에서도 사용 가능하도록 구현
    /// </summary>
    [ContextMenu("Auto Data Loading")]
    public void AutoLoadData()
    {
        #if UNITY_EDITOR
        LoadAllDataFromAssets();
        #else
        Debug.LogWarning("Auto data loading is only available in editor.");
        #endif
    }

    /// <summary>
    /// Resources 폴더에서 데이터를 자동으로 로드하는 기능
    /// 빌드 후에도 사용 가능
    /// </summary>
    [ContextMenu("Load Data from Resources")]
    public void LoadDataFromResources()
    {
        DataLoader.LoadAllDataFromResources(
            m_eventGroupDataList,
            m_factionDataList,
            m_commonResearchDataList,
            m_buildingDataList,
            m_requestLineTemplateList,
            m_resourceIconList,
            m_tokenIconList,
            m_requestIconList,
            ref m_gameBalanceData);
    }



    #if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 모든 데이터를 자동으로 로드
    /// </summary>
    private void LoadAllDataFromAssets()
    {
        DataLoader.LoadAllDataFromAssets(
            m_eventGroupDataList,
            m_factionDataList,
            m_commonResearchDataList,
            m_buildingDataList,
            m_requestLineTemplateList,
            m_resourceIconList,
            m_tokenIconList,
            m_requestIconList,
            ref m_gameBalanceData);
    }

    /// <summary>
    /// 에디터에서 이벤트 그룹 데이터 자동 로딩
    /// </summary>
    private void LoadEventGroupDataFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EventGroupData");
        m_eventGroupDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EventGroupData data = UnityEditor.AssetDatabase.LoadAssetAtPath<EventGroupData>(path);
            if (data != null)
            {
                m_eventGroupDataList.Add(data);
            }
        }
        
        Debug.Log($"{m_eventGroupDataList.Count} event group data loaded.");
    }

    /// <summary>
    /// 에디터에서 팩션 데이터 자동 로딩
    /// </summary>
    private void LoadFactionDataFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FactionData");
        m_factionDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            FactionData data = UnityEditor.AssetDatabase.LoadAssetAtPath<FactionData>(path);
            if (data != null)
            {
                m_factionDataList.Add(data);
            }
        }
        
        Debug.Log($"{m_factionDataList.Count} faction data loaded.");
    }

    /// <summary>
    /// 에디터에서 공통 연구 데이터 자동 로딩
    /// </summary>
    private void LoadCommonResearchDataFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ResearchData");
        m_commonResearchDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ResearchData data = UnityEditor.AssetDatabase.LoadAssetAtPath<ResearchData>(path);
            if (data != null && data.m_factionType == FactionType.TYPE.None)
            {
                m_commonResearchDataList.Add(data);
            }
        }
        
        Debug.Log($"{m_commonResearchDataList.Count} common research data loaded.");
    }

    /// <summary>
    /// 에디터에서 건물 데이터 자동 로딩
    /// </summary>
    private void LoadBuildingDataFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BuildingData");
        m_buildingDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            BuildingData data = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>(path);
            if (data != null)
            {
                m_buildingDataList.Add(data);
            }
        }
        
        Debug.Log($"{m_buildingDataList.Count} building data loaded.");
    }

    /// <summary>
    /// 에디터에서 요청 라인 템플릿 데이터 자동 로딩
    /// </summary>
    private void LoadRequestLineTemplateDataFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RequestLineTemplate");
        m_requestLineTemplateList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            RequestLineTemplate data = UnityEditor.AssetDatabase.LoadAssetAtPath<RequestLineTemplate>(path);
            if (data != null)
            {
                m_requestLineTemplateList.Add(data);
            }
        }
        
        Debug.Log($"{m_requestLineTemplateList.Count} request line templates loaded.");
    }

    /// <summary>
    /// 에디터에서 리소스 아이콘 자동 설정
    /// </summary>
    private void LoadResourceIconsFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite");
        m_resourceIconList.Clear();
        
        var resourceTypes = System.Enum.GetValues(typeof(ResourceType.TYPE));
        
        foreach (ResourceType.TYPE resourceType in resourceTypes)
        {
            string resourceName = resourceType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(resourceName) || 
                     sprite.name.ToLower().Contains("resource") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    ResourceIcon icon = new ResourceIcon
                    {
                        m_type = resourceType,
                        m_icon = sprite
                    };
                    m_resourceIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{m_resourceIconList.Count} resource icons auto-setup completed.");
    }

    /// <summary>
    /// 에디터에서 토큰 아이콘 자동 설정
    /// </summary>
    private void LoadTokenIconsFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite");
        m_tokenIconList.Clear();
        
        var tokenTypes = System.Enum.GetValues(typeof(TokenType.TYPE));
        
        foreach (TokenType.TYPE tokenType in tokenTypes)
        {
            string tokenName = tokenType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(tokenName) || 
                     sprite.name.ToLower().Contains("token") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    TokenIcon icon = new TokenIcon
                    {
                        m_type = tokenType,
                        m_icon = sprite
                    };
                    m_tokenIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{m_tokenIconList.Count} token icons auto-setup completed.");
    }

    /// <summary>
    /// 에디터에서 요청 아이콘 자동 설정
    /// </summary>
    private void LoadRequestIconsFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite");
        m_requestIconList.Clear();
        
        var requestTypes = System.Enum.GetValues(typeof(RequestType.TYPE));
        
        foreach (RequestType.TYPE requestType in requestTypes)
        {
            string requestName = requestType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(requestName) || 
                     sprite.name.ToLower().Contains("request") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    RequestIcon icon = new RequestIcon
                    {
                        m_type = requestType,
                        m_icon = sprite
                    };
                    m_requestIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{m_requestIconList.Count} request icons auto-setup completed.");
    }

    /// <summary>
    /// 에디터에서 게임 밸런스 데이터 자동 로딩
    /// </summary>
    private void LoadGameBalanceDataFromAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameBalanceData");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameBalanceData data = UnityEditor.AssetDatabase.LoadAssetAtPath<GameBalanceData>(path);
            if (data != null)
            {
                m_gameBalanceData = data;
                Debug.Log("Game balance data loaded.");
                return;
            }
        }
        
        Debug.LogWarning("Game balance data not found.");
    }
    #endif

    /// <summary>
    /// 모든 데이터 딕셔너리를 초기화
    /// 팩션, 연구, 건물, 요청 템플릿 데이터를 딕셔너리에 등록
    /// </summary>
    private void InitDict()
    {
        m_factionEntryDic.Clear();
        m_commonResearchEntryDic.Clear();

        // 팩션 데이터 딕셔너리 초기화
        foreach (FactionData faction in m_factionDataList)
        {
            if (!m_factionEntryDic.ContainsKey(faction.m_factionType))
            {
                FactionEntry _entry = new(faction);
                m_factionEntryDic.Add(faction.m_factionType, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorNoSuchType + faction.m_factionType);
            }
        }

        // 공통 연구 데이터 딕셔너리 초기화
        foreach (ResearchData research in m_commonResearchDataList)
        {
            if (!m_commonResearchEntryDic.ContainsKey(research.name))
            {
                ResearchEntry _entry = new(research);
                m_commonResearchEntryDic.Add(research.m_code, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed + research.m_code);
            }
        }

        LockResearch();
        
        // 건물 데이터 딕셔너리 초기화
        foreach (BuildingData building in m_buildingDataList)
        {
            if (!m_buildingEntryDic.ContainsKey(building.name))
            {
                BuildingEntry _entry = new(building);
                m_buildingEntryDic.Add(building.m_code, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed + building.name);
            }
        }

        // 요청 템플릿 딕셔너리 초기화
        foreach (RequestLineTemplate item in m_requestLineTemplateList)
        {
            if (!m_buildingEntryDic.ContainsKey(item.name))
            {
                m_requestLineTemplateDic.Add(item.m_type, item);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed + item.name);
            }
        }
    }

    /// <summary>
    /// 아이콘 딕셔너리들을 초기화
    /// 리소스, 토큰, 요청 타입별 아이콘을 딕셔너리에 등록
    /// </summary>
    private void InitIconDict()
    {
        // 리소스 아이콘 딕셔너리 초기화
        m_resourceIconDic.Clear();
        foreach (var entry in m_resourceIconList)
        {
            if (!m_resourceIconDic.ContainsKey(entry.m_type))
            {
                m_resourceIconDic.Add(entry.m_type, entry.m_icon);
            }
            else
            {
                Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + entry.m_type);
            }
        }

        // 토큰 아이콘 딕셔너리 초기화
        m_tokenIconDic.Clear();
        foreach (var entry in m_tokenIconList)
        {
            if (!m_tokenIconDic.ContainsKey(entry.m_type))
            {
                m_tokenIconDic.Add(entry.m_type, entry.m_icon);
            }
            else
            {
                Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + entry.m_type);
            }
        }

        // 요청 아이콘 딕셔너리 초기화
        m_requestIconDic.Clear();
        {
            foreach (var entry in m_requestIconList)
            {
                if (!m_requestIconDic.ContainsKey(entry.m_type))
                {
                    m_requestIconDic.Add(entry.m_type, entry.m_icon);
                }
                else
                {
                    Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + entry.m_type);
                }
            }
        }
    }

    /// <summary>
    /// 게임 밸런스 엔트리를 초기화
    /// 게임의 기본 밸런스 설정을 적용
    /// </summary>
    private void InitBalanceEntry()
    {
        m_gameBalanceData.InitializeDict();

        m_gameBalanceEntry = new GameBalanceEntry(m_gameBalanceData, new GameBalanceState());

        GameBalanceEntry.m_state.m_mainMul = GameBalanceEntry.m_data.GetBalanceTypeBalance(
            GameBalanceEntry.m_data.m_firstBalanceType).m_mul;
        GameBalanceEntry.m_state.m_dateMul = 1.0f;
    }

    /// <summary>
    /// 이벤트 엔트리를 초기화
    /// 이벤트 그룹 데이터를 기반으로 이벤트 시스템 설정
    /// </summary>
    private void InitEventEntry()
    {
        m_eventEntry = new EventEntry(m_eventGroupDataList, this);
    }



    /// <summary>
    /// 연구 잠금 상태 설정
    /// </summary>
    private void LockResearch()
    {
        foreach (ResearchData research in m_commonResearchDataList)
        {
            ResearchEntry entry = m_commonResearchEntryDic[research.m_code];
            
            // m_unlocks에 있는 연구들의 잠금 해제
            if (research.m_unlocks != null)
            {
                foreach (ResearchData unlockResearch in research.m_unlocks)
                {
                    if (m_commonResearchEntryDic.TryGetValue(unlockResearch.m_code, out ResearchEntry unlockEntry))
                    {
                        unlockEntry.m_state.m_isLocked = false;
                    }
                }
            }
        }

        // m_prerequisites 조건 확인하여 잠금 설정
        foreach (ResearchData research in m_commonResearchDataList)
        {
            ResearchEntry entry = m_commonResearchEntryDic[research.m_code];
            
            // m_prerequisites 조건 확인
            if (research.m_prerequisites != null && research.m_prerequisites.Count > 0)
            {
                bool allPrerequisitesMet = true;
                
                foreach (ResearchData prerequisite in research.m_prerequisites)
                {
                    if (m_commonResearchEntryDic.TryGetValue(prerequisite.m_code, out ResearchEntry prereqEntry))
                    {
                        // 선행 연구가 완료되지 않았으면 잠금
                        if (!prereqEntry.m_state.m_isResearched)
                        {
                            allPrerequisitesMet = false;
                            break;
                        }
                    }
                }
                
                // 선행 조건이 충족되지 않으면 잠금
                if (!allPrerequisitesMet)
                {
                    entry.m_state.m_isLocked = true;
                }
            }
        }
    }

    /// <summary>
    /// 랜덤하게 건물을 생성
    /// </summary>
    /// <param name="buildingCount">생성할 건물 개수</param>
    public void RandomBuilding(int buildingCount)
    {
        for(int i = 0; i < buildingCount; i++)
        {
            ProbabilityUtils.GetRandomElement(BuildingEntryDict).Value.m_state.m_amount++;
        }
    }



    /// <summary>
    /// 랜덤 요청을 생성
    /// 일반 요청과 연락 요청을 모두 생성
    /// </summary>
    public void MakeRandomRequest()
    {
        RequestGenerator.MakeRandomRequest(
            m_acceptableRequestList,
            m_factionEntryDic,
            m_gameBalanceEntry,
            m_contactLineTemplate,
            m_requestLineTemplateDic);
    }

    /// <summary>
    /// 강제 연락 요청 생성 (GameManager에서 호출)
    /// </summary>
    public void ForceContactRequest()
    {
        RequestGenerator.GenerateContactRequests(
            m_acceptableRequestList,
            m_factionEntryDic,
            m_gameBalanceEntry,
            m_contactLineTemplate);
    }

    /// <summary>
    /// 요청을 수락
    /// 수락 가능한 요청 리스트에서 수락된 요청 리스트로 이동
    /// </summary>
    /// <param name="request">수락할 요청</param>
    public void AcceptRequest(RequestState request)
    {
        if (m_acceptableRequestList.Contains(request))
        {
            m_acceptedRequestList.Add(request);
            m_acceptableRequestList.Remove(request);
        }
    }

    /// <summary>
    /// 팩션 엔트리를 가져옴
    /// </summary>
    /// <param name="argType">팩션 타입</param>
    /// <returns>팩션 엔트리, 없으면 null</returns>
    public FactionEntry GetFactionEntry(FactionType.TYPE argType)
    {
        if (m_factionEntryDic != null && m_factionEntryDic.TryGetValue(argType, out FactionEntry entry))
        {
            return entry;
        }

        Debug.LogWarning($"{ExceptionMessages.ErrorNoSuchType}: FactionType.TYPE - {argType}");
        return null;
    }

    /// <summary>
    /// 공통 연구 엔트리를 가져옴
    /// </summary>
    /// <param name="argKey">연구 코드</param>
    /// <returns>연구 엔트리, 없으면 null</returns>
    public ResearchEntry GetCommonResearchEntry(string argKey)
    {
        if (string.IsNullOrEmpty(argKey))
        {
            return null;
        }

        if (m_commonResearchEntryDic != null && m_commonResearchEntryDic.TryGetValue(argKey, out ResearchEntry entry))
        {
            return entry;
        }

        Debug.LogWarning($"{ExceptionMessages.ErrorNoSuchType}: CommonResearch - {argKey}");
        return null;
    }

    /// <summary>
    /// 건물 엔트리를 가져옴
    /// </summary>
    /// <param name="argKey">건물 코드</param>
    /// <returns>건물 엔트리, 없으면 null</returns>
    public BuildingEntry GetBuildingEntry(string argKey)
    {
        if (string.IsNullOrEmpty(argKey))
        {
            return null;
        }

        if (m_buildingEntryDic != null && m_buildingEntryDic.TryGetValue(argKey, out BuildingEntry entry))
        {
            return entry;
        }

        Debug.LogWarning($"{ExceptionMessages.ErrorNoSuchType}: Building - {argKey}");
        return null;
    }

    /// <summary>
    /// 요청 라인 템플릿을 가져옴
    /// </summary>
    public RequestLineTemplate GetRequestLineTemplate(RequestType.TYPE argType) => 
        m_requestLineTemplateDic.TryGetValue(argType, out RequestLineTemplate item) ? item : null;

    /// <summary>
    /// 리소스 아이콘을 가져옴
    /// </summary>
    public Sprite GetResourceIcon(ResourceType.TYPE type) => 
        m_resourceIconDic.TryGetValue(type, out var icon) ? icon : null;

    /// <summary>
    /// 토큰 아이콘을 가져옴
    /// </summary>
    public Sprite GetTokenIcon(TokenType.TYPE type) => 
        m_tokenIconDic.TryGetValue(type, out var icon) ? icon : null;

    /// <summary>
    /// 요청 아이콘을 가져옴
    /// </summary>
    public Sprite GetRequestIcon(RequestType.TYPE type) => 
        m_requestIconDic.TryGetValue(type, out var icon) ? icon : null;


}