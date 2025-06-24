using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEventGroupData", menuName = "Event Group Data")]
public class EventGroupData : ScriptableObject
{
    public int m_eventGroupKey = 0;
    public float m_firstPercent = 5;
    public float m_dateChangePercent = 1;
    public List<EventData> m_dataList = new();
    
    public void Init(GameDataManager argDataManager)
    {
        foreach(EventData item in m_dataList)
        {
            item.Init(argDataManager);
        }
    }
}
