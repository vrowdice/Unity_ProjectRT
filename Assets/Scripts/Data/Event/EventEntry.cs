using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventEntry : MonoBehaviour
{
    private GameDataManager m_gameDataManager = null;

    public Dictionary<int, EventGroupData> m_groupDataDic = new();
    public Dictionary<int, EventGroupState> m_groupStateDic = new();
    public EventState m_state;

    public EventEntry(List<EventGroupData> argDataList, GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;
        m_state = new EventState();
        //추후 변경 필요
        m_state.m_maxEvent = m_gameDataManager.GameBalanceEntry.m_data.m_firstEventSlot;

        foreach (EventGroupData item in argDataList)
        {
            item.Init(argDataManager);
            m_groupDataDic[item.m_eventGroupKey] = item;
        }

        foreach (ResourceType.TYPE argType in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            m_state.m_territoryResourceModDic[argType] = 0;
        }

        foreach(KeyValuePair<int, EventGroupData> item in m_groupDataDic)
        {
            m_groupStateDic[item.Key] = new EventGroupState(item.Key, item.Value.m_firstPercent);
        }
    }

    public void NextDate()
    {
        //그룹 상태 딕셔너리 가져옴
        foreach (KeyValuePair<int ,EventGroupState> groupStateitem in m_groupStateDic)
        {
            //그룹 상태 딕셔너리 확률에 날마다 변하는 확률 추가
            groupStateitem.Value.m_percent += m_groupDataDic[groupStateitem.Key].m_dateChangePercent;

            //만약 최대 이벤트 갯수이면 다음으로
            if (m_state.m_activeEffectList.Count >= m_state.m_maxEvent)
            {
                continue;
            }

            //그 확률을 이용해 그룹 이벤트 활성화 여부 판단
            if (ProbabilityUtils.RollPercent(groupStateitem.Value.m_percent) == true)
            {
                //만약 활성화 되면 원래 확률로 돌아감
                groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;

                //그룹의 랜덤 이벤트의 이펙트 활성화
                EventData _eventData = ProbabilityUtils.GetRandomElement(m_groupDataDic[groupStateitem.Key].m_dataList);
                _eventData.ActivateAllEffect(m_gameDataManager, Random.Range(_eventData.m_minDuration, _eventData.m_maxDuration));
                foreach(EffectBase effectBaseItem in _eventData.m_effectList)
                {
                    m_state.m_activeEffectList.Add(effectBaseItem);
                }
            }
        }

        //틱으로 지속시간을 감소하고 0이면 비활성화
        for (int i = m_state.m_activeEffectList.Count - 1; i >= 0; i--)
        {
            var effect = m_state.m_activeEffectList[i];
            if (effect.Tick(m_gameDataManager))
            {
                m_state.m_activeEffectList.RemoveAt(i);
            }
        }
    }
}
