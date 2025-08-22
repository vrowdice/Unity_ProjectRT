using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEventData", menuName = "Event Data")]
public class EventData : ScriptableObject
{
    public bool m_isGood;
    public int m_minDuration = 1;
    public int m_maxDuration = 2;
    public string m_title;
    public string m_description;

    public List<ConditionBase> m_conditionList = new();
    public List<EffectBase> m_effectList = new();

    /// <summary>
    /// 이벤트 초기화
    /// </summary>
    /// <param name="argDataManager">게임 데이터 매니저</param>
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

    public ActiveEvent Activate(GameDataManager dataManager)
    {
        int duration = Random.Range(m_minDuration, m_maxDuration + 1);
        
        // 새로운 이펙트 시스템 사용
        foreach (var effect in m_effectList)
        {
            if (effect.ActivateEffect(dataManager, m_title))
            {
                // 활성화 성공 시 EffectManager에 추가
                dataManager.EffectManager.AddActiveEventEffect(effect);
            }
        }
        
        return new ActiveEvent(this, duration);
    }

    public void ActivateAllEffect(GameDataManager dataManager, int duration)
    {
        foreach(EffectBase item in m_effectList)
        {
            if (item.ActivateEffect(dataManager, m_title))
            {
                dataManager.EffectManager.AddActiveEventEffect(item);
            }
        }
    }

    public void DeactiveAllEffect(GameDataManager dataManager)
    {
        foreach (EffectBase item in m_effectList)
        {
            if (item.DeactivateEffect(dataManager))
            {
                dataManager.EffectManager.RemoveActiveEventEffect(item);
            }
        }
    }
}
