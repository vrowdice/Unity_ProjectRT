using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BarrackFactionToggle : MonoBehaviour
{
    [SerializeField]
    Image m_factionIcon;
    [SerializeField]
    Toggle m_toggle; // Toggle 컴포넌트

    private FactionData m_factionData;
    private Action<FactionType.TYPE> m_onFactionSelected;

    private void Awake()
    {
        // Toggle 컴포넌트가 없다면 자동으로 추가
        if (m_toggle == null)
        {
            m_toggle = GetComponent<Toggle>();
        }

        // Toggle 이벤트 연결
        if (m_toggle != null)
        {
            m_toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    public void Init(FactionData argFactionData){
        m_factionData = argFactionData;
        m_factionIcon.sprite = argFactionData.m_icon;
    }

    /// <summary>
    /// 팩션 선택 콜백 설정
    /// </summary>
    /// <param name="callback">팩션이 선택되었을 때 호출될 콜백</param>
    public void SetOnFactionSelected(Action<FactionType.TYPE> callback)
    {
        m_onFactionSelected = callback;
    }

    /// <summary>
    /// 토글 그룹 설정
    /// </summary>
    /// <param name="toggleGroup">설정할 토글 그룹</param>
    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        if (m_toggle != null)
        {
            m_toggle.group = toggleGroup;
        }
    }

    /// <summary>
    /// 토글 값이 변경되었을 때 호출되는 콜백
    /// </summary>
    /// <param name="isOn">토글 상태</param>
    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            OnClickFactionToggle();
        }
    }

    public void OnClickFactionToggle(){
        Debug.Log($"OnClickFactionToggle - {m_factionData.m_name}");
        
        // 콜백이 설정되어 있다면 호출
        m_onFactionSelected?.Invoke(m_factionData.m_factionType);
    }
}
