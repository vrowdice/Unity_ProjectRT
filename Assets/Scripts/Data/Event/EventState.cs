using System.Collections.Generic;

public class EventState
{
    public List<EventData> m_activeEventList = new();

    public Dictionary<ResourceType.TYPE, int> m_resourceMod = new();

    public int m_requestMod;
    public int m_requestTimeMod;
}
