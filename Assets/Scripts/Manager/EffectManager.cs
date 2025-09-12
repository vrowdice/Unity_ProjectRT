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
        
        // 모든 팩션의 연구에서 활성화된 이펙트 수집
        foreach (var factionKvp in m_gameDataManager.FactionResearchEntryDict)
        {
            var researchEntry = factionKvp.Value;
            if (researchEntry != null)
            {
                // 팩션의 모든 연구를 확인
                foreach (var researchKvp in researchEntry.ResearchByKey)
                {
                    var researchData = researchKvp.Value;
                    var researchState = researchEntry.GetResearchStateByKey(researchKvp.Key);
                    
                    if (researchState != null && researchState.m_isResearched)
                    {
                        // 연구가 완료된 경우 해당 연구의 이펙트들을 추가
                        if (researchData.m_effects != null)
                        {
                            allEffects.AddRange(researchData.m_effects);
                        }
                    }
                }
            }
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
        var researchData = m_gameDataManager.GetResearchByKey(researchCode);
        var researchState = m_gameDataManager.GetResearchStateByKey(researchCode);
        
        if (researchData != null && researchState != null && researchState.m_isResearched)
        {
            return researchData.m_effects ?? new List<EffectBase>();
        }
        
        return new List<EffectBase>();
    }

    /// <summary>
    /// 모든 연구 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <returns>모든 연구 이펙트 정보 문자열</returns>
    public string GetAllResearchEffectInfo()
    {
        List<string> researchInfos = new List<string>();
        
        // 모든 팩션의 완료된 연구 정보 수집
        foreach (var factionKvp in m_gameDataManager.FactionResearchEntryDict)
        {
            var researchEntry = factionKvp.Value;
            if (researchEntry != null)
            {
                foreach (var researchKvp in researchEntry.ResearchByKey)
                {
                    var researchData = researchKvp.Value;
                    var researchState = researchEntry.GetResearchStateByKey(researchKvp.Key);
                    
                    if (researchState != null && researchState.m_isResearched)
                    {
                        string effectInfo = GetResearchEffectInfo(researchData, researchState);
                        if (!string.IsNullOrEmpty(effectInfo))
                        {
                            researchInfos.Add(effectInfo);
                        }
                    }
                }
            }
        }
        
        if (researchInfos.Count == 0)
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
        var researchData = m_gameDataManager.GetResearchByKey(researchCode);
        var researchState = m_gameDataManager.GetResearchStateByKey(researchCode);
        
        if (researchData != null && researchState != null)
        {
            return GetResearchEffectInfo(researchData, researchState);
        }
        
        return $"연구 코드 '{researchCode}'를 찾을 수 없습니다.";
    }

    /// <summary>
    /// 연구 데이터와 상태를 기반으로 이펙트 정보 문자열 생성
    /// </summary>
    /// <param name="researchData">연구 데이터</param>
    /// <param name="researchState">연구 상태</param>
    /// <returns>연구 이펙트 정보 문자열</returns>
    private string GetResearchEffectInfo(FactionResearchData researchData, FactionResearchState researchState)
    {
        if (!researchState.m_isResearched)
        {
            return $"{researchData.m_name}: 연구되지 않음";
        }

        if (researchData.m_effects == null || researchData.m_effects.Count == 0)
        {
            return $"{researchData.m_name}: 연구 완료 (활성 이펙트 없음)";
        }

        var effectInfos = researchData.m_effects.Select(e => e.GetEffectInfo());
        return $"{researchData.m_name}: 연구 완료\n" + string.Join("\n", effectInfos);
    }

    /// <summary>
    /// 모든 연구 이펙트 비활성화
    /// </summary>
    /// <returns>비활성화된 이펙트 수</returns>
    public int DeactivateAllResearchEffects()
    {
        int deactivatedCount = 0;
        
        // 모든 팩션의 완료된 연구에서 이펙트 비활성화
        foreach (var factionKvp in m_gameDataManager.FactionResearchEntryDict)
        {
            var researchEntry = factionKvp.Value;
            if (researchEntry != null)
            {
                foreach (var researchKvp in researchEntry.ResearchByKey)
                {
                    var researchData = researchKvp.Value;
                    var researchState = researchEntry.GetResearchStateByKey(researchKvp.Key);
                    
                    if (researchState != null && researchState.m_isResearched)
                    {
                        // 연구의 모든 이펙트 비활성화
                        if (researchData.m_effects != null)
                        {
                            foreach (var effect in researchData.m_effects)
                            {
                                if (effect.DeactivateEffect(m_gameDataManager))
                                {
                                    deactivatedCount++;
                                }
                            }
                        }
                    }
                }
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
        foreach (var factionKvp in m_gameDataManager.FactionResearchEntryDict)
        {
            var researchEntry = factionKvp.Value;
            if (researchEntry != null)
            {
                foreach (var researchKvp in researchEntry.ResearchByKey)
                {
                    var researchData = researchKvp.Value;
                    if (researchData.m_effects != null)
                    {
                        foreach (var effect in researchData.m_effects)
                        {
                            effect.ForceReset();
                        }
                    }
                }
            }
        }
        
        Debug.Log("All effects (event + research) force reset.");
    }
    #endregion
}