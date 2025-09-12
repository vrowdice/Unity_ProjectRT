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

    public void Initialize(ResearchPanel argResearchPanel, FactionResearchData argResearchData, FactionResearchState argResearchState, FactionEntry argFactionEntry, string researchCode)
    {
        m_researchPanel = argResearchPanel;
        m_researchData = argResearchData;
        m_researchState = argResearchState;
        m_factionEntry = argFactionEntry;
        m_researchCode = researchCode;
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
            // GameDataManager에서 해당 팩션의 연구 엔트리를 가져와서 전달
            var factionResearchEntry = GameDataManager.Instance.GetFactionResearchEntry(m_factionEntry.m_data.m_factionType);
            if (factionResearchEntry != null)
            {
                m_researchPanel.OpenResearchDetailPanel(factionResearchEntry, m_researchCode);
            }
            else
            {
                Debug.LogError($"FactionResearchEntry not found for faction: {m_factionEntry.m_data.m_factionType}");
            }
        }
    }
}
