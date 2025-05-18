public class ResearchEntry
{
    public ResearchData m_data;
    public ResearchState m_state;

    public ResearchEntry(ResearchData argData)
    {
        this.m_data = argData;
        this.m_state = new ResearchState();

        m_state.m_isLocked = m_data.m_isFirstLocked;
    }
}
