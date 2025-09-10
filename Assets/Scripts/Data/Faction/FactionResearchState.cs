using System.Collections.Generic;

public class FactionResearchState
{
    public int m_progress = -1;  // -1: 미시작, 0~: 진행도
    public bool m_isLocked = false;
    public bool m_isResearched = false;
    
    // 편의 메서드들
    public bool IsStarted => m_progress >= 0;
    public bool IsInProgress => IsStarted && !m_isResearched;
    public bool IsNotStarted => m_progress < 0;
    public bool IsCompleted => m_isResearched;
}
