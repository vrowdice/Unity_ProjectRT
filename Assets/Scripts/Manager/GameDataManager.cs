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
    [SerializeField]
    private List<FactionData> m_factionDataList = new();
    [SerializeField]
    private List<ResearchData> m_commonResearchDataList = new();
    [SerializeField]
    private List<BuildingData> m_buildingDataList = new();
    [SerializeField]
    private List<RequestLineTemplate> m_requestLineTemplateList = new();

    [Header("Common Data")]
    [SerializeField]
    private List<ResourceIcon> m_resourceIconList = new();
    [SerializeField]
    private List<TokenIcon> m_tokenIconList = new();
    [SerializeField]
    private List<RequestIcon> m_requestIconList = new();
    [SerializeField]
    private GameBalanceData m_gameBalanceData;

    private readonly Dictionary<FactionType.TYPE, FactionEntry> m_factionEntryDict = new();
    private readonly Dictionary<string, ResearchEntry> m_commonResearchEntryDict = new();
    private readonly Dictionary<string, BuildingEntry> m_buildingEntryDict = new();
    private readonly Dictionary<ResourceType.TYPE, Sprite> m_resourceIconDict = new();
    private readonly Dictionary<TokenType.TYPE, Sprite> m_tokenIconDict = new();
    private readonly Dictionary<RequestType.TYPE, Sprite> m_requestIconDict = new();

    private readonly List<RequestState> m_acceptableRequestList = new();
    private readonly List<RequestState> m_acceptedRequestList = new();

    private GameBalanceEntry m_gameBalanceEntry;

    public Dictionary<FactionType.TYPE, FactionEntry> FactionEntryDict => m_factionEntryDict;
    public Dictionary<string, ResearchEntry> CommonResearchEntryDict => m_commonResearchEntryDict;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDict;
    public List<RequestState> AcceptableRequestList => m_acceptableRequestList;
    public List<RequestState> AcceptedRequestList => m_acceptedRequestList;
    public GameBalanceEntry GameBalanceEntry => m_gameBalanceEntry;

    void Awake()
    {
        InitializeDict();
        InitializeIcons();
        InitializeBalanceEntry();
    }

    private void InitializeDict()
    {
        m_factionEntryDict.Clear();
        m_commonResearchEntryDict.Clear();

        foreach (FactionData faction in m_factionDataList)
        {
            if (!m_factionEntryDict.ContainsKey(faction.m_factionType))
            {
                FactionEntry _entry = new(faction);

                m_factionEntryDict.Add(faction.m_factionType, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorNoSuchType + faction.m_factionType);
            }
        }

        foreach (ResearchData research in m_commonResearchDataList)
        {
            if (!m_commonResearchEntryDict.ContainsKey(research.name))
            {
                ResearchEntry _entry = new(research);

                research.m_code = research.name;
                m_commonResearchEntryDict.Add(research.m_code, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed + research.m_code);
            }
        }
        
        foreach (BuildingData building in m_buildingDataList)
        {
            if (!m_buildingEntryDict.ContainsKey(building.name))
            {
                BuildingEntry _entry = new(building);

                building.m_code = building.name;
                m_buildingEntryDict.Add(building.m_code, _entry);
            }
            else
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed + building.name);
            }
        }
    }

    private void InitializeBalanceEntry()
    {
        m_gameBalanceData.InitializeDict();

        m_gameBalanceEntry = new GameBalanceEntry(m_gameBalanceData, new GameBalanceState());

        GameBalanceEntry.m_state.m_mainMul = GameBalanceEntry.m_data.GetBalanceTypeBalance(
            GameBalanceEntry.m_data.m_firstBalanceType).m_mul;
        GameBalanceEntry.m_state.m_dateMul = 1.0f;
    }

    private void InitializeIcons()
    {
        m_resourceIconDict.Clear();
        foreach (var entry in m_resourceIconList)
        {
            if (!m_resourceIconDict.ContainsKey(entry.m_type))
            {
                m_resourceIconDict.Add(entry.m_type, entry.m_icon);
            }
            else
            {
                Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + entry.m_type);
            }
        }

        m_tokenIconDict.Clear();
        foreach (var entry in m_tokenIconList)
        {
            if (!m_tokenIconDict.ContainsKey(entry.m_type))
            {
                m_tokenIconDict.Add(entry.m_type, entry.m_icon);
            }
            else
            {
                Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + entry.m_type);
            }
        }

        m_requestIconDict.Clear();
        {
            foreach (var entry in m_requestIconList)
            {
                if (!m_requestIconDict.ContainsKey(entry.m_type))
                {
                    m_requestIconDict.Add(entry.m_type, entry.m_icon);
                }
                else
                {
                    Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + entry.m_type);
                }
            }
        }
    }

    private void ResetAcceptableRequest()
    {
        AcceptableRequestList.Clear();
    }

    /// <summary>
    /// 컨택트 랜덤 의뢰를 최대 두개 생성합니다
    /// 첫 번째 컨택트 확률과 두 번째 컨택트 확률이 각각 정해져 있습니다
    /// </summary>
    private void RandomContactRequest()
    {
        List<FactionType.TYPE> _factionTypes = FactionEntryDict.Keys.ToList();

        //랜덤 팩션 타입 지정
        FactionType.TYPE _type = ProbabilityUtils.GetRandomElement(_factionTypes);

        //계산식
        float _per = GameBalanceEntry.m_state.m_noContactCount *
            GameBalanceEntry.m_data.m_noContactChangePer +
            GameBalanceEntry.m_data.m_firstContactPer;

        //첫 번째 컨택
        if (ProbabilityUtils.RollPercent(_per) == true)
        {
            m_acceptableRequestList.Add(new RequestState(
                true,
                GameManager.Instance.Date,
                GetFactionEntry(_type).m_state.m_like,
                ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>()),
                _type,
                GameBalanceEntry));

            _factionTypes.Remove(_type);

            //두 번째 컨택 팩션 타입 지정
            _type = ProbabilityUtils.GetRandomElement(_factionTypes);

            //두 번째 컨택 계산식
            _per = GameBalanceEntry.m_state.m_noContactCount *
                GameBalanceEntry.m_data.m_noContactChangePer +
                GameBalanceEntry.m_data.m_overSecondContactPer;

            //두 번째 컨택
            if (ProbabilityUtils.RollPercent(_per) == true)
            {
                m_acceptableRequestList.Add(new RequestState(
                    true,
                    GameManager.Instance.Date,
                    GetFactionEntry(_type).m_state.m_like,
                    ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>()),
                    _type,
                    GameBalanceEntry));
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
            FactionType.TYPE _type = ProbabilityUtils.GetRandomElement(_haveFactionTypes);

            int _like;
            if (GetFactionEntry(_type) == null)
            {
                _like = 0;
            }
            else
            {
                _like = GetFactionEntry(_type).m_state.m_like;
            }

            //의뢰 추가 부분
            m_acceptableRequestList.Add(new RequestState(
                false,
                GameManager.Instance.Date,
                _like,
                ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>()),
                _type,
                GameBalanceEntry));

            //만약 하나의 팩션 의뢰가 나왔다면 그 팩션은 제외하고 의뢰를 생성합니다.
            _haveFactionTypes.Remove(_type);
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
        if (m_factionEntryDict != null && m_factionEntryDict.TryGetValue(argType, out FactionEntry entry))
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

        if (m_commonResearchEntryDict != null && m_commonResearchEntryDict.TryGetValue(argKey, out ResearchEntry entry))
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

        if (m_buildingEntryDict != null && m_buildingEntryDict.TryGetValue(argKey, out BuildingEntry entry))
        {
            return entry;
        }

        Debug.LogWarning($"{ExceptionMessages.ErrorNoSuchType}: Building - {argKey}");
        return null;
    }

    public Sprite GetResourceIcon(ResourceType.TYPE type)
    {
        if (m_resourceIconDict.TryGetValue(type, out var icon))
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
        if (m_tokenIconDict.TryGetValue(type, out var icon))
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
        if (m_requestIconDict.TryGetValue(type, out var icon))
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