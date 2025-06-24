using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventInfoPanel : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI m_eventTitleText = null;
    [SerializeField] TextMeshProUGUI m_timeText = null;
    [SerializeField] Transform m_effectContent = null;

    [SerializeField] GameObject m_effectTextPrefeb = null;

    public void Init(EventData argEventData, EventState argEventState)
    {
        m_eventTitleText.text = argEventData.m_title;
        m_timeText.text = string.Empty;
    }
}
