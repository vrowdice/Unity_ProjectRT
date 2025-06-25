public class ActiveEvent
{
    public EventData m_eventData;
    public int m_remainingDuration;

    public ActiveEvent(EventData data, int duration)
    {
        m_eventData = data;
        m_remainingDuration = duration;
    }

    public bool Tick(GameDataManager dataManager)
    {
        m_remainingDuration--;
        if (m_remainingDuration <= 0)
        {
            m_eventData.DeactiveAllEffect(dataManager);
            return true; // ¸¸·áµÊ
        }
        return false;
    }
}

