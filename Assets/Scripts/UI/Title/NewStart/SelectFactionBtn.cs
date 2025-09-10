using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectFactionBtn : MonoBehaviour
{
    [SerializeField]
    Image m_image = null;

    private FactionData m_factionData = null;
    private SelectFactionPanel m_selectFactionPanel = null;

    public void Initialize(FactionData argFactionData, SelectFactionPanel argParentPanel)
    {
        m_factionData = argFactionData;
        m_selectFactionPanel = argParentPanel;

        // 버튼 이미지: 팩션 아이콘이 명확치 않아 유닛 아이콘을 대표 이미지로 시도
        if (m_image != null && m_factionData != null && m_factionData.m_units != null)
        {
            foreach (var unit in m_factionData.m_units)
            {
                if (unit != null && unit.unitIcon != null)
                {
                    m_image.sprite = unit.unitIcon;
                    m_image.preserveAspect = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (Unity Inspector에서 연결)
    /// </summary>
    public void OnClick()
    {
        if (m_selectFactionPanel != null && m_factionData != null)
        {
            m_selectFactionPanel.ShowFactionInfo(m_factionData);
        }
    }
}
