using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResearchBtn : MonoBehaviour
{
    [SerializeField]
    Image m_iconImage = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;

    private string m_researchCode;
    private ResearchPanel m_researchPanel = null;
    private FactionResearchData m_researchData = null;
    private FactionResearchState m_researchState = null;
    private FactionEntry m_factionEntry = null;

    public void Initialize(ResearchPanel argResearchPanel, FactionResearchData argResearchData, FactionResearchState argResearchState, FactionEntry argFactionEntry)
    {
        m_researchPanel = argResearchPanel;
        m_researchData = argResearchData;
        m_researchState = argResearchState;
        m_factionEntry = argFactionEntry;
        m_researchCode = argResearchData.m_code;
        m_iconImage.sprite = argResearchData.m_icon;
        m_nameText.text = argResearchData.m_name;
        m_expText.text = argResearchData.m_description;
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (Unity Inspector에서 연결)
    /// </summary>
    public void OnButtonClick()
    {
        if (m_researchPanel != null && m_researchData != null && m_researchState != null && m_factionEntry != null)
        {
            // 임시로 ResearchEntry를 생성해서 전달 (OpenResearchDetailPanel이 수정되기 전까지)
            var tempEntry = new FactionResearchEntry(m_researchData);
            tempEntry.m_state = m_researchState;
            m_researchPanel.OpenResearchDetailPanel(tempEntry);
        }
    }
}
