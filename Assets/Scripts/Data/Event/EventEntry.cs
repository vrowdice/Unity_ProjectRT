using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventEntry
{
    private GameDataManager m_gameDataManager = null;

    public Dictionary<int, EventGroupData> m_groupDataDic = new();
    public Dictionary<int, EventGroupState> m_groupStateDic = new();
    public EventState m_state = new();

    public EventEntry(List<EventGroupData> argDataList, GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;
        //이벤트 슬롯 필요
        m_state.m_maxEvent = m_gameDataManager.GameBalanceEntry.m_data.m_firstEventSlot;

        foreach (EventGroupData item in argDataList)
        {
            item.Init(argDataManager);
            m_groupDataDic[item.m_eventGroupKey] = item;
        }

        foreach (ResourceType.TYPE argType in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            m_state.m_territoryResourceModDic[argType] = 1.0f;
            m_state.m_buildingResourceModDic[argType] = 1.0f;
        }

        foreach(KeyValuePair<int, EventGroupData> item in m_groupDataDic)
        {
            m_groupStateDic[item.Key] = new EventGroupState(item.Key, item.Value.m_firstPercent);
        }
    }

    /// <summary>
    /// 날짜 증가 처리 확률성 이벤트 발생
    /// </summary>
    /// <returns></returns>
    public bool AddDate()
    {
        bool _isAddEvent = false;

        //진행중인 이벤트 지속시간을 감소시키고 0이면 비활성화
        for (int i = m_state.m_activeEventList.Count - 1; i >= 0; i--)
        {
            if (m_state.m_activeEventList[i].Tick(m_gameDataManager))
            {
                m_state.m_activeEventList.RemoveAt(i);
            }
        }

        //그룹별 이벤트 확률을 증가시킴
        foreach (KeyValuePair<int ,EventGroupState> groupStateitem in m_groupStateDic)
        {
            //그룹별 이벤트 확률을 확인하여 증가시킴
            groupStateitem.Value.m_percent += m_groupDataDic[groupStateitem.Key].m_dateChangePercent;
            Debug.Log("Event Group Key: " + groupStateitem.Key + ", Percent: " + groupStateitem.Value.m_percent);

            //이미 진행중인 이벤트가 있으면 건너뜀
            if (m_state.m_activeEventList.Count >= m_state.m_maxEvent)
            {
                continue;
            }

            //확률 확인하여 그룹 이벤트 활성화 여부 결정
            if (ProbabilityUtils.RollPercent(groupStateitem.Value.m_percent) == true)
            {
                //이벤트 슬롯이 가득 찬 경우 확률을 초기화하고 건너뜀
                if (m_state.m_activeEventList.Count >= m_state.m_maxEvent)
                {
                    groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;
                    continue;
                }

                //이벤트 활성화되면 확률을 초기화
                groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;

                //그룹에서 랜덤 이벤트를 선택하여 활성화
                EventData _eventData = ProbabilityUtils.GetRandomElement(m_groupDataDic[groupStateitem.Key].m_dataList);
                //조건을 확인하고 조건이 만족되면 이벤트를 활성화
                foreach (ConditionBase condition in _eventData.m_conditionList)
                {
                    condition.Initialize(m_gameDataManager);
                    if(condition.IsSatisfied() != false)
                    {
                        continue;
                    }
                }
                ActiveEvent _newEvent = _eventData.Activate(m_gameDataManager);
                m_state.m_activeEventList.Add(_newEvent);

                _isAddEvent = true;
            }
        }

        return _isAddEvent;
    }
}
