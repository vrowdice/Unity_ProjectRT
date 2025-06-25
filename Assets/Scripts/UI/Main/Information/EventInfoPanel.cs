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

    public void Init(ActiveEvent activeEventData)
    {
        m_eventTitleText.text = activeEventData.m_eventData.m_title;
        m_timeText.text = activeEventData.m_remainingDuration.ToString();

        foreach (EffectBase effect in activeEventData.m_eventData.m_effectList)
        {
            TextMeshProUGUI _effectText = Instantiate(m_effectTextPrefeb, m_effectContent).GetComponent<TextMeshProUGUI>();
            _effectText.text = effect.m_description;
        }
    }
}
