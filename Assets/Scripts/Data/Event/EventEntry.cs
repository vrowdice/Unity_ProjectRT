using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventEntry : MonoBehaviour
{
    public List<EventData> m_dataList;
    public EventState m_state;

    public EventEntry()
    {
        foreach (ResourceType.TYPE argType in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            m_state.m_resourceMod[argType] = 0;
        }
    }
}
