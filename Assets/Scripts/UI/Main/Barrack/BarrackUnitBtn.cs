using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarrackUnitBtn : MonoBehaviour
{
    [SerializeField]
    Image m_unitIcon;
    [SerializeField]
    TextMeshProUGUI m_unitName;

    private UnitData m_unitData;

    public void Init(UnitData argUnitData){
        m_unitData = argUnitData;
        m_unitIcon.sprite = argUnitData.unitIcon;
        m_unitName.text = argUnitData.unitName;
    }

    /// <summary>
    /// 이 유닛의 태그 타입을 반환합니다
    /// </summary>
    /// <returns>유닛 태그 타입</returns>
    public UnitTagType GetUnitTagType()
    {
        return m_unitData != null ? m_unitData.unitTagType : UnitTagType.Melee;
    }

    public void OnClickUnitBtn(){
        Debug.Log($"OnClickUnitBtn - {m_unitData?.unitName}");
    }
}