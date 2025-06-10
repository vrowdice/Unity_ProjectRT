using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CommonResearchBtn : MonoBehaviour
{
    [SerializeField]
    Image m_iconImage = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;

    private string m_researchCode;
    private ResearchPanel m_researchPanel = null;

    public void Initialize(ResearchPanel argResearchPanel, ResearchEntry argEntry)
    {
        m_researchPanel = argResearchPanel;
        m_researchCode = argEntry.m_data.m_code;
        m_iconImage.sprite = argEntry.m_data.m_icon;
        m_nameText.text = argEntry.m_data.m_name;
        m_expText.text = argEntry.m_data.m_description;
    }
}
