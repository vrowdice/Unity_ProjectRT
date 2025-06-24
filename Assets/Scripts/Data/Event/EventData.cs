using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEventData", menuName = "Event Data")]
public class EventData : ScriptableObject
{
    public EventType.TYPE m_type;
    public bool m_isGood;
    public int m_eventGroupKey;
    public int m_minDuration = 1;
    public int m_maxDuration = 2;
    public string m_title;
    public string m_description;

    public List<ConditionBase> m_conditionList = new();
    public List<EffectBase> m_effectList = new();

    /// <summary>
    /// 생성 후 반드시 호출
    /// </summary>
    /// <param name="argDataManager">데이터 메니저</param>
    public void Init(GameDataManager argDataManager)
    {
        foreach (var condition in m_conditionList)
        {
            if (condition is IInitializableCondition init)
            {
                init.Initialize(argDataManager);
            }
        }
    }

    public void ActivateAllEffect(GameDataManager dataManager, int duration)
    {
        foreach(EffectBase item in m_effectList)
        {
            item.Init(duration);

            item.Activate(dataManager);
        }
    }

    public void DeactiveAllEffect(GameDataManager dataManager)
    {
        foreach (EffectBase item in m_effectList)
        {
            item.Deactivate(dataManager);
        }
    }
}
