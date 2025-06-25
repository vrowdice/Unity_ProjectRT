using System.Collections.Generic;

public class EventState
{
    public List<ActiveEvent> m_activeEventList = new();

    public Dictionary<ResourceType.TYPE, float> m_territoryResourceModDic = new();
    public Dictionary<ResourceType.TYPE, float> m_buildingResourceModDic = new();
    public int m_maxEvent;
    public float m_requestMod;
    public float m_requestTimeMod;
}
