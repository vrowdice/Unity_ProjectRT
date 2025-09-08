using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class BarrackFactionUnitPanel : MonoBehaviour
{
    [SerializeField]
    Image m_factionIcon;
    [SerializeField]
    TextMeshProUGUI m_factionName;

    [SerializeField]
    Transform m_unitBtnContentTrans;
    [SerializeField]
    GameObject m_unitBtnPrefab;

    private FactionData m_factionData;
    private BarrackPanel m_ownerPanel;
    private List<BarrackUnitBtn> m_unitButtons = new List<BarrackUnitBtn>();

    public void Init(FactionData argFactionData, BarrackPanel ownerPanel){
        m_factionData = argFactionData;
        m_ownerPanel = ownerPanel;
        m_factionName.text = argFactionData.m_name;
        m_factionIcon.sprite = argFactionData.m_icon;

        // 기존 유닛 버튼들 정리
        ClearUnitButtons();

        foreach (var unitData in argFactionData.m_units){
            GameObject unitBtn = Instantiate(m_unitBtnPrefab, m_unitBtnContentTrans);
            BarrackUnitBtn unitBtnComponent = unitBtn.GetComponent<BarrackUnitBtn>();
            unitBtnComponent.Init(unitData, m_ownerPanel);
            
            m_unitButtons.Add(unitBtnComponent);
        }
    }

    /// <summary>
    /// 기존 유닛 버튼들을 정리합니다
    /// </summary>
    private void ClearUnitButtons()
    {
        foreach (Transform child in m_unitBtnContentTrans)
        {
            Destroy(child.gameObject);
        }
        m_unitButtons.Clear();
    }

    /// <summary>
    /// 이 패널의 팩션 타입을 반환합니다
    /// </summary>
    /// <returns>팩션 타입</returns>
    public FactionType.TYPE GetFactionType()
    {
        return m_factionData != null ? m_factionData.m_factionType : FactionType.TYPE.None;
    }

    /// <summary>
    /// 지정된 유닛 타입에 따라 유닛 버튼들을 필터링합니다
    /// </summary>
    /// <param name="filterType">필터링할 유닛 타입 (null이면 모든 타입 표시)</param>
    public void FilterUnitsByType(UnitTagType? filterType)
    {
        foreach (var unitBtn in m_unitButtons)
        {
            if (unitBtn != null)
            {
                bool shouldShow = (filterType == null) || (unitBtn.GetUnitTagType() == filterType.Value);
                unitBtn.gameObject.SetActive(shouldShow);
            }
        }
    }
}
