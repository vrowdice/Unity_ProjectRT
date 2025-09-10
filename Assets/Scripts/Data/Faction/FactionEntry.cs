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
    }
}

