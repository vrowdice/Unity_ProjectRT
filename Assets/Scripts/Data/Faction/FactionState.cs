using System.Collections.Generic;

public class FactionState
{
    public int m_like = 0;
    
    // 연구 진행 상태 관리 (연구 코드 -> 연구 상태)
    public Dictionary<string, ResearchState> m_researchStateDict = new();


}
