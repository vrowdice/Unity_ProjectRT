using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
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

    private readonly Dictionary<FactionType.TYPE, FactionEntry> m_factionEntryDic = new();
    private readonly Dictionary<string, ResearchEntry> m_commonResearchEntryDic = new();
    private readonly Dictionary<string, BuildingEntry> m_buildingEntryDic = new();
    private readonly Dictionary<ResourceType.TYPE, Sprite> m_resourceIconDic = new();
    private readonly Dictionary<TokenType.TYPE, Sprite> m_tokenIconDic = new();
    private readonly Dictionary<RequestType.TYPE, Sprite> m_requestIconDic = new();
    private readonly Dictionary<RequestType.TYPE, RequestLineTemplate> m_requestLineTemplateDic = new();

    private readonly List<RequestState> m_acceptableRequestList = new();
    private readonly List<RequestState> m_acceptedRequestList = new();

    private GameBalanceEntry m_gameBalanceEntry;
    private EventEntry m_eventEntry;

    public Dictionary<FactionType.TYPE, FactionEntry> FactionEntryDict => m_factionEntryDic;
    public Dictionary<string, ResearchEntry> CommonResearchEntryDict => m_commonResearchEntryDic;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDic;
    public List<RequestState> AcceptableRequestList => m_acceptableRequestList;
    public List<RequestState> AcceptedRequestList => m_acceptedRequestList;
    public GameBalanceEntry GameBalanceEntry => m_gameBalanceEntry;
    public EventEntry EventEntry => m_eventEntry;

    void Awake()
    {
        InitDict();
        InitIconDict();
        InitBalanceEntry();
        InitEventEntry();
    }

    private void InitDict()
    {
        m_factionEntryDic.Clear();
        m_commonResearchEntryDic.Clear();

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

        foreach (ResearchData research in m_commonResearchDataList)
        {
            if (!m_commonResearchEntryDic.ContainsKey(research.name))
            {
                ResearchEntry _entry = new(research);

                research.m_code = research.name;
                m_commonResearchEntryDic.Add(research.m_code, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed + research.m_code);
            }
        }
        
        foreach (BuildingData building in m_buildingDataList)
        {
            if (!m_buildingEntryDic.ContainsKey(building.name))
            {
                BuildingEntry _entry = new(building);

                building.m_code = building.name;
                m_buildingEntryDic.Add(building.m_code, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed + building.name);
            }
        }

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

    private void InitIconDict()
    {
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

    private void InitBalanceEntry()
    {
        m_gameBalanceData.InitializeDict();

        m_gameBalanceEntry = new GameBalanceEntry(m_gameBalanceData, new GameBalanceState());

        GameBalanceEntry.m_state.m_mainMul = GameBalanceEntry.m_data.GetBalanceTypeBalance(
            GameBalanceEntry.m_data.m_firstBalanceType).m_mul;
        GameBalanceEntry.m_state.m_dateMul = 1.0f;
    }

    private void InitEventEntry()
    {
        m_eventEntry = new EventEntry(m_eventGroupDataList, this);
    }

    private void ResetAcceptableRequest()
    {
        AcceptableRequestList.Clear();
    }

    /// <summary>
    /// 컨택트 랜덤 의뢰를 최대 두개 생성합니다
    /// 첫 번째 컨택트 확률과 두 번째 컨택트 확률이 각각 정해져 있습니다
    /// </summary>
    public void RandomContactRequest()
    {
        List<FactionType.TYPE> _factionTypes = FactionEntryDict.Keys.ToList();
        _factionTypes.Remove(FactionType.TYPE.None);

        //랜덤 팩션 타입 지정
        FactionType.TYPE _type = ProbabilityUtils.GetRandomElement(_factionTypes);

        //계산식
        float _per = GameBalanceEntry.m_state.m_noContactCount *
            GameBalanceEntry.m_data.m_noContactChangePer +
            GameBalanceEntry.m_data.m_firstContactPer;

        //첫 번째 컨택
        if (ProbabilityUtils.RollPercent(_per) == true)
        {
            RequestType.TYPE _requestType = ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>());
            m_acceptableRequestList.Add(new RequestState(
                true,
                GameManager.Instance.Date,
                GetFactionEntry(_type).m_state.m_like,
                ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>()),
                _type,
                GameBalanceEntry,
                m_contactLineTemplate));

            _factionTypes.Remove(_type);

            //두 번째 컨택 팩션 타입 지정
            _type = ProbabilityUtils.GetRandomElement(_factionTypes);

            //두 번째 컨택 계산식
            _per = GameBalanceEntry.m_state.m_noContactCount *
                GameBalanceEntry.m_data.m_noContactChangePer +
                GameBalanceEntry.m_data.m_overSecondContactPer;

            //두 번째 컨택
            _requestType = ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>());
            if (ProbabilityUtils.RollPercent(_per) == true)
            {
                m_acceptableRequestList.Add(new RequestState(
                    true,
                    GameManager.Instance.Date,
                    GetFactionEntry(_type).m_state.m_like,
                    _requestType,
                    _type,
                    GameBalanceEntry,
                    m_contactLineTemplate));
            }

            GameBalanceEntry.m_state.m_noContactCount = 0;
        }
    }

    /// <summary>
    /// 일반 의뢰 생성
    /// </summary>
    private void RandomNormalRequest()
    {
        List<FactionType.TYPE> _haveFactionTypes = GetHaveFactionTypeList();

        //의뢰 갯수를 반영해 최대 의뢰를 생성합니다.
        for (int i = 0; i < GameBalanceEntry.m_data.m_maxRequest; i++)
        {
            FactionType.TYPE _factionType = ProbabilityUtils.GetRandomElement(_haveFactionTypes);

            int _like;
            if (GetFactionEntry(_factionType) == null)
            {
                _like = 0;
            }
            else
            {
                _like = GetFactionEntry(_factionType).m_state.m_like;
            }

            //의뢰 추가 부분
            RequestType.TYPE _requestType = ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>());
            m_acceptableRequestList.Add(new RequestState(
                false,
                GameManager.Instance.Date,
                _like,
                _requestType,
                _factionType,
                GameBalanceEntry,
                GetRequestLineTemplate(_requestType)));

            //만약 하나의 팩션 의뢰가 나왔다면 그 팩션은 제외하고 의뢰를 생성합니다.
            _haveFactionTypes.Remove(_factionType);
        }
    }

    public void MakeRandomRequest()
    {
        ResetAcceptableRequest();

        RandomNormalRequest();
        RandomContactRequest();
    }

    public void AcceptRequest(RequestState request)
    {
        if (m_acceptableRequestList.Contains(request))
        {
            m_acceptedRequestList.Add(request);
            m_acceptableRequestList.Remove(request);
        }
    }

    public FactionEntry GetFactionEntry(FactionType.TYPE argType)
    {
        if (m_factionEntryDic != null && m_factionEntryDic.TryGetValue(argType, out FactionEntry entry))
        {
            return entry;
        }

        Debug.LogWarning($"{ExceptionMessages.ErrorNoSuchType}: FactionType.TYPE - {argType}");
        return null;
    }

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

    public RequestLineTemplate GetRequestLineTemplate(RequestType.TYPE argType)
    {
        if (m_requestLineTemplateDic.TryGetValue(argType, out RequestLineTemplate item))
        {
            return item;
        }
        else
        {
            Debug.LogWarning(ExceptionMessages.ErrorNoSuchType);
            return null;
        }
    }

    public Sprite GetResourceIcon(ResourceType.TYPE type)
    {
        if (m_resourceIconDic.TryGetValue(type, out var icon))
        {
            return icon;
        }
        else
        {
            Debug.LogWarning(ExceptionMessages.ErrorNoSuchType);
            return null;
        }
    }

    public Sprite GetTokenIcon(TokenType.TYPE type)
    {
        if (m_tokenIconDic.TryGetValue(type, out var icon))
        {
            return icon;
        }
        else
        {
            Debug.LogWarning(ExceptionMessages.ErrorNoSuchType);
            return null;
        }
    }

    public Sprite GetRequestIcon(RequestType.TYPE type)
    {
        if (m_requestIconDic.TryGetValue(type, out var icon))
        {
            return icon;
        }
        else
        {
            Debug.LogWarning(ExceptionMessages.ErrorNoSuchType);
            return null;
        }
    }

    /// <summary>
    /// 만약 그 팩션을 가지고 있지 않을 경우 리스트에서 제외합니다.
    /// </summary>
    /// <returns>팩션 타입 리스트</returns>
    public List<FactionType.TYPE> GetHaveFactionTypeList()
    {
        List<FactionType.TYPE> _factionTypes = FactionEntryDict.Keys.ToList();

        for (int i = _factionTypes.Count - 1; i >= 0; i--)
        {
            FactionType.TYPE item = _factionTypes[i];

            if (item == FactionType.TYPE.None)
            {
                continue;
            }

            if (GetFactionEntry(item).m_state.m_have == false)
            {
                _factionTypes.RemoveAt(i);
            }
        }

        return _factionTypes;
    }
}