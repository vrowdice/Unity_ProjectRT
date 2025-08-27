public class FactionEntry
{
    public FactionData m_data;
    public FactionState m_state;

    public FactionEntry(FactionData argData)
    {
        this.m_data = argData;
        this.m_state = new FactionState
        {
            m_like = 0,
        };
        
        // 연구 상태 초기화
        InitializeResearchStates();
    }
    
    /// <summary>
    /// 팩션의 연구 데이터를 기반으로 ResearchState 초기화
    /// </summary>
    private void InitializeResearchStates()
    {
        if (m_data.m_research != null)
        {
            foreach (var researchData in m_data.m_research)
            {
                if (researchData != null && !string.IsNullOrEmpty(researchData.m_code))
                {
                    var researchState = new ResearchState();
                    researchState.m_isLocked = researchData.m_isFirstLocked;
                    
                    m_state.m_researchStateDict[researchData.m_code] = researchState;
                }
            }
        }
    }
    
    /// <summary>
    /// 특정 연구의 상태 가져오기
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <returns>연구 상태 (없으면 null)</returns>
    public ResearchState GetResearchState(string researchCode)
    {
        return m_state.m_researchStateDict.TryGetValue(researchCode, out var researchState) ? researchState : null;
    }
    
    /// <summary>
    /// 특정 연구의 데이터 가져오기
    /// </summary>
    /// <param name="researchCode">연구 코드</param>
    /// <returns>연구 데이터 (없으면 null)</returns>
    public ResearchData GetResearchData(string researchCode)
    {
        if (m_data.m_research != null)
        {
            return m_data.m_research.Find(r => r.m_code == researchCode);
        }
        return null;
    }
}