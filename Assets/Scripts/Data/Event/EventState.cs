using System.Collections.Generic;

public class EventState
{
    public List<ActiveEvent> m_activeEventList = new();

    // 곱연산용 딕셔너리 (배수)
    public Dictionary<ResourceType.TYPE, float> m_territoryResourceModDic = new();
    public Dictionary<ResourceType.TYPE, float> m_buildingResourceModDic = new();
    
    // 합연산용 딕셔너리 (고정값 추가/감소)
    public Dictionary<ResourceType.TYPE, float> m_territoryResourceAddDic = new();
    public Dictionary<ResourceType.TYPE, float> m_buildingResourceAddDic = new();
    
    public int m_maxEvent;
    public float m_requestMod;
    public float m_requestTimeMod;
}
