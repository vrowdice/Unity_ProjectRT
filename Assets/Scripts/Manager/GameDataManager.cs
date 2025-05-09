using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public List<FactionData> FactionData => m_factionDataList;
    public List<ResearchData> ResearchData => m_commonResearchDataList;
    public Dictionary<FactionType, FactionData> FactionDataDict => m_factionDataDict;
    public Dictionary<string, ResearchData> CommonResearchDataDict => m_commonResearchDataDict;

    [SerializeField]
    private List<FactionData> m_factionDataList = new List<FactionData>();
    [SerializeField]
    private List<ResearchData> m_commonResearchDataList = new List<ResearchData>();

    private Dictionary<FactionType, FactionData> m_factionDataDict = new Dictionary<FactionType, FactionData>();
    private Dictionary<string, ResearchData> m_commonResearchDataDict = new Dictionary<string, ResearchData>();

    void Awake()
    {
        InitializeDataDict();
    }

    private void InitializeDataDict()
    {
        m_factionDataDict.Clear();
        m_commonResearchDataDict.Clear();

        foreach (var faction in m_factionDataList)
        {
            if (!m_factionDataDict.ContainsKey(faction.m_factionType))
            {
                m_factionDataDict.Add(faction.m_factionType, faction);
            }
            else
            {
                Debug.LogError($"Duplicate faction type found: {faction.m_factionType}");
            }
        }

        foreach (var research in m_commonResearchDataList)
        {
            if (!m_commonResearchDataDict.ContainsKey(research.name))
            {
                Debug.Log(research.name);
                research.m_code = research.name;
                m_commonResearchDataDict.Add(research.m_code, research);
            }
            else
            {
                Debug.LogError($"Duplicate research name found: {research.m_code}");
            }
        }
    }
}