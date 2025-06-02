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
    private List<RequestState> m_inProgressRequestList = new List<RequestState>();

    public List<FactionData> FactionData => m_factionDataList;
    public List<ResearchData> ResearchData => m_commonResearchDataList;
    public Dictionary<FactionType, FactionEntry> FactionEntryDict => m_factionEntryDict;
    public Dictionary<string, ResearchEntry> CommonResearchEntryDict => m_commonResearchEntryDict;
    public Dictionary<string, BuildingEntry> BuildingEntryDict => m_buildingEntryDict;
    public GameBalanceData GameBalanceData => m_gameBalanceData;

    void Awake()
    {
        InitializeDict();
        InitializeIcons();
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

    private void GetRandomRequest()
    {

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