using UnityEngine;
using System.Collections.Generic;

public abstract class EffectBase : ScriptableObject
{
    [Header("Effect Information")]
    public string m_name;
    public string m_description;
    
    [Header("Event Tracking")]
    [SerializeField, HideInInspector] private string m_activatedEventName = "";
    [SerializeField, HideInInspector] private bool m_isActive = false;
    
    /// <summary>
    /// 이펙트가 활성화되어 있는지 확인
    /// </summary>
    public bool IsActive => m_isActive;
    
    /// <summary>
    /// 이펙트를 활성화한 이벤트 이름
    /// </summary>
    public string ActivatedEventName => m_activatedEventName;
    
    /// <summary>
    /// 이펙트 활성화 (중복 방지 포함)
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>활성화 성공 여부</returns>
    public bool ActivateEffect(GameDataManager dataManager, string eventName)
    {
        // 이미 활성화되어 있으면 중복 방지
        if (m_isActive)
        {
            return false;
        }
        
        // 이벤트 이름이 비어있으면 활성화 거부
        if (string.IsNullOrEmpty(eventName))
        {
            return false;
        }
        
        // 실제 활성화 로직 실행
        Activate(dataManager, eventName);
        
        // 상태 업데이트
        m_activatedEventName = eventName;
        m_isActive = true;
        
        return true;
    }
    
    /// <summary>
    /// 이펙트 비활성화
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    /// <returns>비활성화 성공 여부</returns>
    public bool DeactivateEffect(GameDataManager dataManager)
    {
        // 활성화되어 있지 않으면 비활성화 불가
        if (!m_isActive)
        {
            Debug.LogWarning($"Effect '{m_name}' is not active. Cannot deactivate.");
            return false;
        }
        
        // 실제 비활성화 로직 실행
        Deactivate(dataManager);
        
        // 상태 업데이트
        string previousEventName = m_activatedEventName;
        m_activatedEventName = "";
        m_isActive = false;
        
        Debug.Log($"Effect '{m_name}' deactivated (was activated from event '{previousEventName}').");
        return true;
    }
    
    /// <summary>
    /// 이펙트 정보를 사용자에게 표시할 수 있는 문자열 반환
    /// </summary>
    /// <returns>이펙트 정보 문자열</returns>
    public virtual string GetEffectInfo()
    {
        if (!m_isActive)
        {
            return $"{m_name}: 비활성화됨";
        }
        
        return $"{m_name}: 활성화됨 (이벤트: {m_activatedEventName})\n{m_description}";
    }
    
    /// <summary>
    /// 이펙트 강제 초기화 (디버그용)
    /// </summary>
    public void ForceReset()
    {
        m_activatedEventName = "";
        m_isActive = false;
        Debug.Log($"Effect '{m_name}' force reset.");
    }
    
    // 추상 메서드들 (기존과 동일)
    public abstract void Activate(GameDataManager dataManager, string eventName);
    public abstract void Deactivate(GameDataManager dataManager);
}

