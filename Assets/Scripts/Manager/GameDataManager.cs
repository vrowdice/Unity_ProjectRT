using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public List<FactionData> FactionData => m_factionDataList;
    public Dictionary<FactionType, FactionData> FactionDataDict => m_factionDataDict;

    [SerializeField]
    private List<FactionData> m_factionDataList = new List<FactionData>();

    private Dictionary<FactionType, FactionData> m_factionDataDict = new Dictionary<FactionType, FactionData>();

    void Awake()
    {
        InitializeFactionDataDict();
    }

    private void InitializeFactionDataDict()
    {
        m_factionDataDict.Clear();

        foreach (var faction in m_factionDataList)
        {
            if (!m_factionDataDict.ContainsKey(faction.m_factionType))
            {
                m_factionDataDict.Add(faction.m_factionType, faction);
            }
            else
            {
                Debug.LogWarning($"Duplicate faction type found: {faction.m_factionType}");
            }
        }
    }
}
