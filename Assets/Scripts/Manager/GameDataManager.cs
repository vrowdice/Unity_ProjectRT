using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public List<FactionData> FactionData => m_factionDataList;
    public List<ResearchData> ResearchData => m_commonResearchDataList;
    public Dictionary<FactionType, FactionEntry> FactionDataDict => m_factionEntryDict;
    public Dictionary<string, ResearchEntry> CommonResearchDataDict => m_commonResearchEntryDict;
    public Dictionary<string, RequestData> RequestDataDict => m_requestDataDict;

    [SerializeField]
    private List<FactionData> m_factionDataList = new List<FactionData>();
    [SerializeField]
    private List<ResearchData> m_commonResearchDataList = new List<ResearchData>();
    [SerializeField]
    private List<RequestData> m_requestDataList = new List<RequestData>();

    private Dictionary<FactionType, FactionEntry> m_factionEntryDict = new Dictionary<FactionType, FactionEntry>();
    private Dictionary<string, ResearchEntry> m_commonResearchEntryDict = new Dictionary<string, ResearchEntry>();
    private Dictionary<string, RequestData> m_requestDataDict = new Dictionary<string, RequestData>();

    void Awake()
    {
        InitializeDict();
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
                Debug.LogError($"Duplicate faction type found: {faction.m_factionType}");
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
                Debug.LogError($"Duplicate research name found: {research.m_code}");
            }
        }

        foreach(RequestData item in m_requestDataList)
        {
            if (!m_requestDataDict.ContainsKey(item.name))
            {
                item.m_code = item.name;
                m_requestDataDict.Add(item.m_code, item);
            }
            else
            {
                Debug.LogError($"Duplicate research name found: {item.m_code}");
            }
        }
    }
}