using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    [System.Serializable]
    public struct ResourceIcon
    {
        public ResourceType m_type;
        public Sprite m_icon;
    }
    [System.Serializable]
    public struct RequestIcon
    {
        public RequestType m_type;
        public Sprite m_icon;
    }

    [Header("Game Data")]
    [SerializeField]
    private List<FactionData> m_factionDataList = new List<FactionData>();
    [SerializeField]
    private List<ResearchData> m_commonResearchDataList = new List<ResearchData>();
    [SerializeField]
    private List<BuildingData> m_buildingDataList = new List<BuildingData>();

    [Header("Common Data")]
    [SerializeField]
    private List<ResourceIcon> m_resourceIconList = new();
    [SerializeField]
    private List<RequestIcon> m_requestIconList = new();
    [SerializeField]
    private GameBalanceData m_gameBalanceData;

    private Dictionary<FactionType, FactionEntry> m_factionEntryDict = new Dictionary<FactionType, FactionEntry>();
    private Dictionary<string, ResearchEntry> m_commonResearchEntryDict = new Dictionary<string, ResearchEntry>();
    private Dictionary<string, BuildingEntry> m_buildingEntryDict = new Dictionary<string, BuildingEntry>();
    private Dictionary<ResourceType, Sprite> m_resourceIconDict = new();
    private Dictionary<RequestType, Sprite> m_requestIconDict = new();

    private List<RequestState> m_acceptableRequestList = new List<RequestState>();
    private List<RequestState> m_AcceptedRequestList = new List<RequestState>();

    private GameBalanceEntry m_gameBalanceEntry;

    public Dictionary<FactionType, FactionEntry> FactionEntryDict => m_factionEntryDict;
    public Dictionary<string, ResearchEntry> CommonResearchEntryDict => m_commonResearchEntryDict;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDict;
    public List<RequestState> AcceptableRequestList => m_acceptableRequestList;
    public List<RequestState> AcceptedRequestList => m_AcceptedRequestList;
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
                FactionEntry _entry = new FactionEntry(faction);

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
                ResearchEntry _entry = new ResearchEntry(research);

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
                BuildingEntry _entry = new BuildingEntry(building);

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

    private void RandomContactRequest()
    {

    }

    /// <summary>
    /// 일반 의뢰 생성
    /// 보통은 컨텍트 의뢰를 먼저 생성한 다음에 호출합니다.
    /// </summary>
    private void RandomNormalRequest()
    {
        List<FactionType> _factionTypes = FactionEntryDict.Keys.ToList();

        //만약 그 팩션을 가지고 있지 않을 경우 리스트에서 제외합니다.
        for (int i = _factionTypes.Count - 1; i >= 0; i--)
        {
            FactionType item = _factionTypes[i];

            if (item == FactionType.None)
            {
                continue;
            }

            if (GetFactionEntry(item).m_state.m_have == false)
            {
                _factionTypes.RemoveAt(i);
            }
        }

        //이미 있는 컨택트 의뢰 갯수를 반영해 최대 의뢰를 생성합니다.
        int _requestCount = GameBalanceEntry.m_data.m_maxRequest - AcceptableRequestList.Count;
        for (int i = 0; i < _requestCount; i++)
        {
            FactionType _type = ProbabilityUtils.GetRandomElement(_factionTypes);

            int _like = 0;
            if (GetFactionEntry(_type) == null)
            {
                _like = 0;
            }
            else
            {
                _like = GetFactionEntry(_type).m_state.m_like;
            }

            List<RequestType> _requestTyps = EnumUtils.GetAllEnumValues<RequestType>();
            _requestTyps.Remove(RequestType.Contact);

            //의뢰 추가 부분
            m_acceptableRequestList.Add(new RequestState(
                GameManager.Instance.Date,
                _like,
                ProbabilityUtils.GetRandomElement(_requestTyps),
                _type,
                GameBalanceEntry));

            //만약 하나의 팩션 의뢰가 나왔다면 그 팩션은 제외하고 의뢰를 생성합니다.
            _factionTypes.Remove(_type);
        }
    }

    public void MakeRandomRequest()
    {
        RandomContactRequest();
        RandomNormalRequest();
    }

    public void AcceptRequest(int argAcceptableRequestIndex)
    {
        m_AcceptedRequestList.Add(AcceptableRequestList[argAcceptableRequestIndex]);
        m_acceptableRequestList.RemoveAt(argAcceptableRequestIndex);
    }

    public FactionEntry GetFactionEntry(FactionType argType)
    {
        if (m_factionEntryDict != null && m_factionEntryDict.TryGetValue(argType, out FactionEntry entry))
        {
            return entry;
        }

        Debug.LogWarning($"{ExceptionMessages.ErrorNoSuchType}: FactionType - {argType}");
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

    public Sprite GetResourceIcon(ResourceType type)
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

    public Sprite GetRequestIcon(RequestType type)
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

}