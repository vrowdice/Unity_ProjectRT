using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 팩션별 연구 엔트리 - 팩션의 모든 연구들과 상태를 함께 관리
/// </summary>
public class FactionResearchEntry
{
    // 팩션의 모든 연구 데이터와 상태
    private readonly Dictionary<string, FactionResearchData> m_researchByKey = new();
    private readonly Dictionary<string, FactionResearchState> m_researchStateByKey = new();
    private readonly List<FactionResearchData> m_allResearch = new();
    
    // 활성화된 이펙트들을 추적하는 리스트 (팩션 전체)
    private readonly Dictionary<string, List<EffectBase>> m_activeEffectsByResearch = new();

    public FactionType.TYPE FactionType { get; private set; }

    // 읽기 전용 접근자
    public IReadOnlyList<FactionResearchData> AllResearch => m_allResearch;
    public IReadOnlyDictionary<string, FactionResearchData> ResearchByKey => m_researchByKey;
    public IReadOnlyDictionary<string, FactionResearchState> ResearchStateByKey => m_researchStateByKey;

    public FactionResearchEntry(FactionType.TYPE factionType)
    {
        FactionType = factionType;
    }

    /// <summary>
    /// 연구를 추가합니다.
    /// </summary>
    public void AddResearch(FactionResearchData research)
    {
        if (research == null || string.IsNullOrEmpty(research.m_code)) return;

        // 중복 방지
        if (!m_allResearch.Contains(research))
            m_allResearch.Add(research);

        // 키별 인덱스
        if (!m_researchByKey.ContainsKey(research.m_code))
        {
            m_researchByKey.Add(research.m_code, research);
            
            // 상태 초기화
            var state = new FactionResearchState();
            state.m_isLocked = research.m_isFirstLocked;
            m_researchStateByKey.Add(research.m_code, state);
            
            // 이펙트 리스트 초기화
            m_activeEffectsByResearch.Add(research.m_code, new List<EffectBase>());
        }
    }

    /// <summary>
    /// 키로 연구 데이터를 검색합니다.
    /// </summary>
    public FactionResearchData GetResearchByKey(string researchCode)
    {
        if (string.IsNullOrEmpty(researchCode)) return null;
        return m_researchByKey.TryGetValue(researchCode, out var research) ? research : null;
    }

    /// <summary>
    /// 키로 연구 상태를 검색합니다.
    /// </summary>
    public FactionResearchState GetResearchStateByKey(string researchCode)
    {
        if (string.IsNullOrEmpty(researchCode)) return null;
        return m_researchStateByKey.TryGetValue(researchCode, out var state) ? state : null;
    }

    /// <summary>
    /// 연구 완료 시 이펙트 활성화
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void ActivateResearchEffects(string researchCode, GameDataManager dataManager)
    {
        var research = GetResearchByKey(researchCode);
        if (research != null && m_activeEffectsByResearch.TryGetValue(researchCode, out var effectList))
        {
            research.ActivateAllEffect(dataManager, this);
        }
    }

    /// <summary>
    /// 연구 이펙트 비활성화
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void DeactivateResearchEffects(string researchCode, GameDataManager dataManager)
    {
        var research = GetResearchByKey(researchCode);
        if (research != null)
        {
            research.DeactivateAllEffect(dataManager, this);
        }
    }

    /// <summary>
    /// 특정 연구의 활성화된 이펙트 목록 가져오기
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <returns>활성 이펙트 리스트</returns>
    public List<EffectBase> GetActiveEffects(string researchCode)
    {
        return m_activeEffectsByResearch.TryGetValue(researchCode, out var effects) ? effects : new List<EffectBase>();
    }

    /// <summary>
    /// 팩션의 모든 활성 이펙트 가져오기
    /// </summary>
    public List<EffectBase> GetAllActiveEffects()
    {
        var allEffects = new List<EffectBase>();
        foreach (var effectList in m_activeEffectsByResearch.Values)
        {
            allEffects.AddRange(effectList);
        }
        return allEffects;
    }

    /// <summary>
    /// 이펙트를 활성 이펙트 리스트에 추가 (내부용)
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <param name="effect">추가할 이펙트</param>
    public void AddActiveEffect(string researchCode, EffectBase effect)
    {
        if (m_activeEffectsByResearch.TryGetValue(researchCode, out var effectList))
        {
            if (!effectList.Contains(effect))
            {
                effectList.Add(effect);
            }
        }
    }

    /// <summary>
    /// 이펙트를 활성 이펙트 리스트에서 제거 (내부용)
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <param name="effect">제거할 이펙트</param>
    public void RemoveActiveEffect(string researchCode, EffectBase effect)
    {
        if (m_activeEffectsByResearch.TryGetValue(researchCode, out var effectList))
        {
            effectList.Remove(effect);
        }
    }

    /// <summary>
    /// 특정 연구의 모든 활성 이펙트 제거 (내부용)
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    public void ClearActiveEffects(string researchCode)
    {
        if (m_activeEffectsByResearch.TryGetValue(researchCode, out var effectList))
        {
            effectList.Clear();
        }
    }

    /// <summary>
    /// 연구 완료 처리
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void CompleteResearch(string researchCode, GameDataManager dataManager)
    {
        var state = GetResearchStateByKey(researchCode);
        if (state != null && !state.m_isResearched)
        {
            state.m_isResearched = true;
            ActivateResearchEffects(researchCode, dataManager);
        }
    }

    /// <summary>
    /// 연구 되돌리기 (디버그용)
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void UndoResearch(string researchCode, GameDataManager dataManager)
    {
        var state = GetResearchStateByKey(researchCode);
        if (state != null && state.m_isResearched)
        {
            DeactivateResearchEffects(researchCode, dataManager);
            state.m_isResearched = false;
        }
    }

    /// <summary>
    /// 연구 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <returns>연구 이펙트 정보 문자열</returns>
    public string GetResearchEffectInfo(string researchCode)
    {
        var research = GetResearchByKey(researchCode);
        var state = GetResearchStateByKey(researchCode);
        
        if (research == null || state == null)
            return "연구를 찾을 수 없음";

        if (!state.m_isResearched)
        {
            return $"{research.m_name}: 연구되지 않음";
        }

        var activeEffects = GetActiveEffects(researchCode);
        if (activeEffects.Count == 0)
        {
            return $"{research.m_name}: 연구 완료 (활성 이펙트 없음)";
        }

        var effectInfos = activeEffects.Select(e => e.GetEffectInfo());
        return $"{research.m_name}: 연구 완료\n" + string.Join("\n", effectInfos);
    }

    /// <summary>
    /// 모든 연구를 초기화합니다.
    /// </summary>
    public void Clear()
    {
        m_researchByKey.Clear();
        m_researchStateByKey.Clear();
        m_allResearch.Clear();
        m_activeEffectsByResearch.Clear();
    }
}
