using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 이펙트 시스템을 관리하는 매니저 클래스
/// 이벤트 이펙트와 연구 이펙트를 통합 관리
/// </summary>
public class EffectManager : MonoBehaviour
{
    // 이벤트 이펙트 관리
    private List<EffectBase> m_activeEventEffects = new();
    
    // 참조
    private GameDataManager m_gameDataManager;
    
    /// <summary>
    /// 이펙트 매니저 초기화
    /// </summary>
    /// <param name="gameDataManager">게임 데이터 매니저 참조</param>
    public void Initialize(GameDataManager gameDataManager)
    {
        m_gameDataManager = gameDataManager;
    }
    
    #region Event Effect Management
    /// <summary>
    /// 이벤트 이펙트를 활성 이펙트 리스트에 추가
    /// </summary>
    /// <param name="effect">추가할 이펙트</param>
    public void AddActiveEventEffect(EffectBase effect)
    {
        if (!m_activeEventEffects.Contains(effect))
        {
            m_activeEventEffects.Add(effect);
        }
    }

    /// <summary>
    /// 이벤트 이펙트를 활성 이펙트 리스트에서 제거
    /// </summary>
    /// <param name="effect">제거할 이펙트</param>
    public void RemoveActiveEventEffect(EffectBase effect)
    {
        m_activeEventEffects.Remove(effect);
    }

    /// <summary>
    /// 모든 이벤트 이펙트 제거
    /// </summary>
    public void ClearActiveEventEffects()
    {
        m_activeEventEffects.Clear();
    }

    /// <summary>
    /// 활성화된 모든 이벤트 이펙트 목록 가져오기
    /// </summary>
    /// <returns>활성 이벤트 이펙트 리스트</returns>
    public List<EffectBase> GetActiveEventEffects()
    {
        return m_activeEventEffects;
    }

    /// <summary>
    /// 특정 이벤트로 활성화된 이펙트 목록 가져오기
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>해당 이벤트로 활성화된 이펙트 리스트</returns>
    public List<EffectBase> GetActiveEffectsByEvent(string eventName)
    {
        return m_activeEventEffects.FindAll(e => e.ActivatedEventName == eventName);
    }

    /// <summary>
    /// 모든 활성 이벤트 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <returns>이벤트 이펙트 정보 문자열</returns>
    public string GetAllEventEffectInfo()
    {
        if (m_activeEventEffects.Count == 0)
        {
            return "활성화된 이벤트 이펙트가 없습니다.";
        }
        
        var effectInfos = m_activeEventEffects.Select(e => e.GetEffectInfo());
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
    /// 특정 이벤트로 활성화된 이펙트들을 비활성화
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>비활성화된 이펙트 수</returns>
    public int DeactivateEffectsByEvent(string eventName)
    {
        int deactivatedCount = 0;
        var effectsToDeactivate = GetActiveEffectsByEvent(eventName).ToList();
        
        foreach (var effect in effectsToDeactivate)
        {
            if (effect.DeactivateEffect(m_gameDataManager))
            {
                m_activeEventEffects.Remove(effect);
                deactivatedCount++;
            }
        }
        
        return deactivatedCount;
    }

    /// <summary>
    /// 모든 이벤트 이펙트 비활성화
    /// </summary>
    /// <returns>비활성화된 이펙트 수</returns>
    public int DeactivateAllEventEffects()
    {
        int deactivatedCount = 0;
        var effectsToDeactivate = m_activeEventEffects.ToList();
        
        foreach (var effect in effectsToDeactivate)
        {
            if (effect.DeactivateEffect(m_gameDataManager))
            {
                m_activeEventEffects.Remove(effect);
                deactivatedCount++;
            }
        }
        
        return deactivatedCount;
    }
    #endregion

    #region Research Effect Management
    /// <summary>
    /// 모든 연구에서 활성화된 이펙트 목록 가져오기
    /// </summary>
    /// <returns>모든 연구의 활성 이펙트 리스트</returns>
    public List<EffectBase> GetAllResearchEffects()
    {
        List<EffectBase> allEffects = new List<EffectBase>();
        
        foreach (var researchEntry in m_gameDataManager.CommonResearchEntryDict.Values)
        {
            allEffects.AddRange(researchEntry.GetActiveEffects());
        }
        
        return allEffects;
    }

    /// <summary>
    /// 특정 연구의 이펙트 목록 가져오기
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <returns>해당 연구의 활성 이펙트 리스트</returns>
    public List<EffectBase> GetResearchEffects(string researchCode)
    {
        if (m_gameDataManager.CommonResearchEntryDict.TryGetValue(researchCode, out var researchEntry))
        {
            return researchEntry.GetActiveEffects();
        }
        
        return new List<EffectBase>();
    }

    /// <summary>
    /// 모든 연구 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <returns>모든 연구 이펙트 정보 문자열</returns>
    public string GetAllResearchEffectInfo()
    {
        var researchInfos = m_gameDataManager.CommonResearchEntryDict.Values
            .Where(r => r.m_state.m_isResearched)
            .Select(r => r.GetResearchEffectInfo());
        
        if (!researchInfos.Any())
        {
            return "완료된 연구가 없습니다.";
        }
        
        return string.Join("\n\n", researchInfos);
    }

    /// <summary>
    /// 특정 연구의 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <returns>연구 이펙트 정보 문자열</returns>
    public string GetResearchEffectInfo(string researchCode)
    {
        if (m_gameDataManager.CommonResearchEntryDict.TryGetValue(researchCode, out var researchEntry))
        {
            return researchEntry.GetResearchEffectInfo();
        }
        
        return $"연구 코드 '{researchCode}'를 찾을 수 없습니다.";
    }

    /// <summary>
    /// 모든 연구 이펙트 비활성화
    /// </summary>
    /// <returns>비활성화된 이펙트 수</returns>
    public int DeactivateAllResearchEffects()
    {
        int deactivatedCount = 0;
        
        foreach (var researchEntry in m_gameDataManager.CommonResearchEntryDict.Values)
        {
            if (researchEntry.m_state.m_isResearched)
            {
                researchEntry.DeactivateResearchEffects(m_gameDataManager);
                deactivatedCount += researchEntry.GetActiveEffects().Count;
            }
        }
        
        return deactivatedCount;
    }
    #endregion

    #region Combined Effect Management
    /// <summary>
    /// 모든 활성 이펙트 목록 가져오기 (이벤트 + 연구)
    /// </summary>
    /// <returns>모든 활성 이펙트 리스트</returns>
    public List<EffectBase> GetAllActiveEffects()
    {
        List<EffectBase> allEffects = new List<EffectBase>();
        
        // 이벤트 이펙트 추가
        allEffects.AddRange(GetActiveEventEffects());
        
        // 연구 이펙트 추가
        allEffects.AddRange(GetAllResearchEffects());
        
        return allEffects;
    }

    /// <summary>
    /// 모든 활성 이펙트 정보를 문자열로 반환 (이벤트 + 연구)
    /// </summary>
    /// <returns>모든 활성 이펙트 정보 문자열</returns>
    public string GetAllActiveEffectInfo()
    {
        List<string> allInfo = new List<string>();
        
        // 이벤트 이펙트 정보
        string eventEffects = GetAllEventEffectInfo();
        if (eventEffects != "활성화된 이벤트 이펙트가 없습니다.")
        {
            allInfo.Add("=== 이벤트 이펙트 ===");
            allInfo.Add(eventEffects);
        }
        
        // 연구 이펙트 정보
        string researchEffects = GetAllResearchEffectInfo();
        if (researchEffects != "완료된 연구가 없습니다.")
        {
            allInfo.Add("=== 연구 이펙트 ===");
            allInfo.Add(researchEffects);
        }
        
        if (allInfo.Count == 0)
        {
            return "활성화된 이펙트가 없습니다.";
        }
        
        return string.Join("\n\n", allInfo);
    }

    /// <summary>
    /// 모든 이펙트 비활성화 (이벤트 + 연구)
    /// </summary>
    /// <returns>비활성화된 이펙트 수</returns>
    public int DeactivateAllEffectsCombined()
    {
        int eventDeactivated = DeactivateAllEventEffects();
        int researchDeactivated = DeactivateAllResearchEffects();
        
        return eventDeactivated + researchDeactivated;
    }

    /// <summary>
    /// 모든 이펙트 강제 초기화 (디버그용)
    /// </summary>
    public void ForceResetAllEffects()
    {
        // 이벤트 이펙트 강제 초기화
        foreach (var effect in m_activeEventEffects)
        {
            effect.ForceReset();
        }
        ClearActiveEventEffects();
        
        // 연구 이펙트 강제 초기화
        foreach (var researchEntry in m_gameDataManager.CommonResearchEntryDict.Values)
        {
            foreach (var effect in researchEntry.GetActiveEffects())
            {
                effect.ForceReset();
            }
            researchEntry.ClearActiveEffects();
        }
        
        Debug.Log("All effects (event + research) force reset.");
    }
    #endregion
}