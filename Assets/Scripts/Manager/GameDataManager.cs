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
    private List<RequestState> m_acceptedRequestList = new List<RequestState>();

    private GameBalanceEntry m_gameBalanceEntry;

    public Dictionary<FactionType, FactionEntry> FactionEntryDict => m_factionEntryDict;
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

    private void ResetAcceptableRequest()
    {
        AcceptableRequestList.Clear();
    }

    /// <summary>
    /// ����Ʈ ���� �Ƿڸ� �ִ� �ΰ� �����մϴ�
    /// ù ��° ����Ʈ Ȯ���� �� ��° ����Ʈ Ȯ���� ���� ������ �ֽ��ϴ�
    /// </summary>
    private void RandomContactRequest()
    {
        List<FactionType> _factionTypes = FactionEntryDict.Keys.ToList();

        //���� �Ѽ� Ÿ�� ����
        FactionType _type = ProbabilityUtils.GetRandomElement(_factionTypes);

        //����
        float _per = GameBalanceEntry.m_state.m_noContactCount *
            GameBalanceEntry.m_data.m_noContactChangePer +
            GameBalanceEntry.m_data.m_firstContactPer;

        //ù ��° ����
        if (ProbabilityUtils.RollPercent(_per) == true)
        {
            m_acceptableRequestList.Add(new RequestState(
                true,
                GameManager.Instance.Date,
                GetFactionEntry(_type).m_state.m_like,
                ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType>()),
                _type,
                GameBalanceEntry));

            _factionTypes.Remove(_type);

            //�� ��° ���� �Ѽ� Ÿ�� ����
            _type = ProbabilityUtils.GetRandomElement(_factionTypes);

            //�� ��° ���� ����
            _per = GameBalanceEntry.m_state.m_noContactCount *
                GameBalanceEntry.m_data.m_noContactChangePer +
                GameBalanceEntry.m_data.m_overSecondContactPer;

            //�� ��° ����
            if (ProbabilityUtils.RollPercent(_per) == true)
            {
                m_acceptableRequestList.Add(new RequestState(
                    true,
                    GameManager.Instance.Date,
                    GetFactionEntry(_type).m_state.m_like,
                    ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType>()),
                    _type,
                    GameBalanceEntry));
            }

            GameBalanceEntry.m_state.m_noContactCount = 0;
        }
    }

    /// <summary>
    /// �Ϲ� �Ƿ� ����
    /// </summary>
    private void RandomNormalRequest()
    {
        List<FactionType> _haveFactionTypes = GetHaveFactionTypeList();

        //�Ƿ� ������ �ݿ��� �ִ� �Ƿڸ� �����մϴ�.
        for (int i = 0; i < GameBalanceEntry.m_data.m_maxRequest; i++)
        {
            FactionType _type = ProbabilityUtils.GetRandomElement(_haveFactionTypes);

            int _like = 0;
            if (GetFactionEntry(_type) == null)
            {
                _like = 0;
            }
            else
            {
                _like = GetFactionEntry(_type).m_state.m_like;
            }

            //�Ƿ� �߰� �κ�
            m_acceptableRequestList.Add(new RequestState(
                false,
                GameManager.Instance.Date,
                _like,
                ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType>()),
                _type,
                GameBalanceEntry));

            //���� �ϳ��� �Ѽ� �Ƿڰ� ���Դٸ� �� �Ѽ��� �����ϰ� �Ƿڸ� �����մϴ�.
            _haveFactionTypes.Remove(_type);
        }
    }

    public void MakeRandomRequest()
    {
        ResetAcceptableRequest();

        RandomNormalRequest();
        RandomContactRequest();
    }

    public void AcceptRequest(int argAcceptableRequestIndex)
    {
        m_acceptedRequestList.Add(AcceptableRequestList[argAcceptableRequestIndex]);
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

    /// <summary>
    /// ���� �� �Ѽ��� ������ ���� ���� ��� ����Ʈ���� �����մϴ�.
    /// </summary>
    /// <returns>�Ѽ� Ÿ�� ����Ʈ</returns>
    public List<FactionType> GetHaveFactionTypeList()
    {
        List<FactionType> _factionTypes = FactionEntryDict.Keys.ToList();

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

        return _factionTypes;
    }
}