using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectFactionUnitBtn : MonoBehaviour
{
    [SerializeField]
    Image m_image = null;

    private UnitData m_unitData = null;
    private SelectFactionPanel m_selectFactionPanel = null;

    public void Initialize(UnitData argUnitData, SelectFactionPanel argParentPanel)
    {
        m_unitData = argUnitData;
        m_selectFactionPanel = argParentPanel;

        if (m_image != null && m_unitData != null && m_unitData.unitIcon != null)
        {
            m_image.sprite = m_unitData.unitIcon;
            m_image.preserveAspect = true;
        }
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (Unity Inspector에서 연결)
    /// </summary>
    public void OnClick()
    {
        if (m_selectFactionPanel != null && m_unitData != null)
        {
            m_selectFactionPanel.ShowUnitDetailPanel(m_unitData);
        }
    }
}
