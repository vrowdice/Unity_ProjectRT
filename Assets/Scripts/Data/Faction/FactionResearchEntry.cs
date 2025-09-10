using System.Collections.Generic;
using System.Linq;

public class FactionResearchEntry
{
    public FactionResearchData m_data;
    public FactionResearchState m_state;
    
    // 활성화된 이펙트들을 추적하는 리스트 (ResearchEntry에 추가)
    private List<EffectBase> m_activeEffects = new();

    public FactionResearchEntry(FactionResearchData argData)
    {
        this.m_data = argData;
        this.m_state = new FactionResearchState();

        m_state.m_isLocked = m_data.m_isFirstLocked;
    }

    /// <summary>
    /// 연구 완료 시 이펙트 활성화
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void ActivateResearchEffects(GameDataManager dataManager)
    {
        if (m_data != null)
        {
            m_data.ActivateAllEffect(dataManager, this);
        }
    }

    /// <summary>
    /// 연구 이펙트 비활성화
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void DeactivateResearchEffects(GameDataManager dataManager)
    {
        if (m_data != null)
        {
            m_data.DeactivateAllEffect(dataManager, this);
        }
    }

    /// <summary>
    /// 활성화된 이펙트 목록 가져오기
    /// </summary>
    /// <returns>활성 이펙트 리스트</returns>
    public List<EffectBase> GetActiveEffects()
    {
        return m_activeEffects;
    }

    /// <summary>
    /// 이펙트를 활성 이펙트 리스트에 추가 (내부용)
    /// </summary>
    /// <param name="effect">추가할 이펙트</param>
    public void AddActiveEffect(EffectBase effect)
    {
        if (!m_activeEffects.Contains(effect))
        {
            m_activeEffects.Add(effect);
        }
    }

    /// <summary>
    /// 이펙트를 활성 이펙트 리스트에서 제거 (내부용)
    /// </summary>
    /// <param name="effect">제거할 이펙트</param>
    public void RemoveActiveEffect(EffectBase effect)
    {
        m_activeEffects.Remove(effect);
    }

    /// <summary>
    /// 모든 활성 이펙트 제거 (내부용)
    /// </summary>
    public void ClearActiveEffects()
    {
        m_activeEffects.Clear();
    }

    /// <summary>
    /// 연구 이펙트 정보를 문자열로 반환
    /// </summary>
    /// <returns>연구 이펙트 정보 문자열</returns>
    public string GetResearchEffectInfo()
    {
        if (!m_state.m_isResearched)
        {
            return $"{m_data.m_name}: 연구되지 않음";
        }

        if (m_activeEffects.Count == 0)
        {
            return $"{m_data.m_name}: 연구 완료 (활성 이펙트 없음)";
        }

        var effectInfos = m_activeEffects.Select(e => e.GetEffectInfo());
        return $"{m_data.m_name}: 연구 완료\n" + string.Join("\n", effectInfos);
    }

    /// <summary>
    /// 연구 완료 처리
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void CompleteResearch(GameDataManager dataManager)
    {
        if (!m_state.m_isResearched)
        {
            m_state.m_isResearched = true;
            ActivateResearchEffects(dataManager);
        }
    }

    /// <summary>
    /// 연구 되돌리기 (디버그용)
    /// </summary>
    /// <param name="dataManager">게임 데이터 매니저</param>
    public void UndoResearch(GameDataManager dataManager)
    {
        if (m_state.m_isResearched)
        {
            DeactivateResearchEffects(dataManager);
            m_state.m_isResearched = false;
        }
    }
}
