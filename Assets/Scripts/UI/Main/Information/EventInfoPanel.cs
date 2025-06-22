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

    public void Init()
    {

    }
}
