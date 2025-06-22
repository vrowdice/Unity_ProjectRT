using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEventData", menuName = "Event Data")]
public class EventData : ScriptableObject
{
    public EventType.TYPE m_type;
    public bool m_isGood;
    public string m_title;
    public string m_description;

    public List<ConditionBase> m_conditionList = new();

    /// <summary>
    /// 생성 후 반드시 호출
    /// </summary>
    /// <param name="argDataManager">데이터 메니저</param>
    public void Initialize(GameDataManager argDataManager)
    {
        foreach (var condition in m_conditionList)
        {
            if (condition is IInitializableCondition init)
            {
                init.Initialize(argDataManager);
            }
        }
    }
}
