using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 이펙트 관리를 담당하는 매니저 클래스
/// 이벤트 이름 기반 중복 방지와 사용자 정보 제공 기능 포함
/// </summary>
public class EffectManager : MonoBehaviour
{
    [Header("Active Effects")]
    [SerializeField] private List<EffectBase> m_activeEffects = new List<EffectBase>();
    
    [Header("Effect Display")]
    [SerializeField] private bool m_showEffectInfo = true;
    
    private GameDataManager m_gameDataManager;
    
    void Awake()
    {
        m_gameDataManager = FindObjectOfType<GameDataManager>();
        if (m_gameDataManager == null)
        {
            Debug.LogError("GameDataManager not found!");
        }
    }
    
    /// <summary>
    /// 이펙트 활성화
    /// </summary>
    /// <param name="effect">활성화할 이펙트</param>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>활성화 성공 여부</returns>
    public bool ActivateEffect(EffectBase effect, string eventName)
    {
        if (effect == null)
        {
            Debug.LogError("Effect is null!");
            return false;
        }
        
        if (m_gameDataManager == null)
        {
            Debug.LogError("GameDataManager is null!");
            return false;
        }
        
        // 이펙트 활성화 시도
        if (effect.ActivateEffect(m_gameDataManager, eventName))
        {
            // 활성화 성공 시 리스트에 추가
            if (!m_activeEffects.Contains(effect))
            {
                m_activeEffects.Add(effect);
            }
            
            if (m_showEffectInfo)
            {
                Debug.Log($"Effect activated: {effect.GetEffectInfo()}");
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 이펙트 비활성화
    /// </summary>
    /// <param name="effect">비활성화할 이펙트</param>
    /// <returns>비활성화 성공 여부</returns>
    public bool DeactivateEffect(EffectBase effect)
    {
        if (effect == null)
        {
            Debug.LogError("Effect is null!");
            return false;
        }
        
        if (m_gameDataManager == null)
        {
            Debug.LogError("GameDataManager is null!");
            return false;
        }
        
        // 이펙트 비활성화 시도
        if (effect.DeactivateEffect(m_gameDataManager))
        {
            // 비활성화 성공 시 리스트에서 제거
            m_activeEffects.Remove(effect);
            
            if (m_showEffectInfo)
            {
                Debug.Log($"Effect deactivated: {effect.m_name}");
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 특정 이벤트로 활성화된 이펙트들을 비활성화
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>비활성화된 이펙트 수</returns>
    public int DeactivateEffectsByEvent(string eventName)
    {
        int deactivatedCount = 0;
        var effectsToDeactivate = m_activeEffects.Where(e => e.ActivatedEventName == eventName).ToList();
        
        foreach (var effect in effectsToDeactivate)
        {
            if (DeactivateEffect(effect))
            {
                deactivatedCount++;
            }
        }
        
        return deactivatedCount;
    }
    
    /// <summary>
    /// 모든 활성 이펙트 비활성화
    /// </summary>
    /// <returns>비활성화된 이펙트 수</returns>
    public int DeactivateAllEffects()
    {
        int deactivatedCount = 0;
        var effectsToDeactivate = m_activeEffects.ToList();
        
        foreach (var effect in effectsToDeactivate)
        {
            if (DeactivateEffect(effect))
            {
                deactivatedCount++;
            }
        }
        
        return deactivatedCount;
    }
    
    /// <summary>
    /// 활성 이펙트 목록 가져오기
    /// </summary>
    /// <returns>활성 이펙트 리스트</returns>
    public List<EffectBase> GetActiveEffects()
    {
        return m_activeEffects.ToList();
    }
    
    /// <summary>
    /// 특정 이벤트로 활성화된 이펙트 목록 가져오기
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>해당 이벤트로 활성화된 이펙트 리스트</returns>
    public List<EffectBase> GetActiveEffectsByEvent(string eventName)
    {
        return m_activeEffects.Where(e => e.ActivatedEventName == eventName).ToList();
    }
    
    /// <summary>
    /// 모든 활성 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <returns>이펙트 정보 문자열</returns>
    public string GetAllEffectInfo()
    {
        if (m_activeEffects.Count == 0)
        {
            return "활성화된 이펙트가 없습니다.";
        }
        
        var effectInfos = m_activeEffects.Select(e => e.GetEffectInfo());
        return string.Join("\n\n", effectInfos);
    }
    
    /// <summary>
    /// 특정 이벤트의 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>이벤트 이펙트 정보 문자열</returns>
    public string GetEventEffectInfo(string eventName)
    {
        var eventEffects = GetActiveEffectsByEvent(eventName);
        
        if (eventEffects.Count == 0)
        {
            return $"'{eventName}' 이벤트의 활성 이펙트가 없습니다.";
        }
        
        var effectInfos = eventEffects.Select(e => e.GetEffectInfo());
        return $"'{eventName}' 이벤트 이펙트:\n" + string.Join("\n\n", effectInfos);
    }
    
    /// <summary>
    /// 이펙트 정보 표시 설정
    /// </summary>
    /// <param name="show">표시 여부</param>
    public void SetShowEffectInfo(bool show)
    {
        m_showEffectInfo = show;
    }
    
    /// <summary>
    /// 디버그용: 모든 이펙트 강제 초기화
    /// </summary>
    [ContextMenu("Force Reset All Effects")]
    public void ForceResetAllEffects()
    {
        foreach (var effect in m_activeEffects)
        {
            effect.ForceReset();
        }
        m_activeEffects.Clear();
        Debug.Log("All effects force reset.");
    }
} 