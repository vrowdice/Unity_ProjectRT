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
            m_state.m_buildingResourceModDic[argType] = 0;
        }

        foreach(KeyValuePair<int, EventGroupData> item in m_groupDataDic)
        {
            m_groupStateDic[item.Key] = new EventGroupState(item.Key, item.Value.m_firstPercent);
        }
    }

    /// <summary>
    /// 다음 날 처리 확률을 높이고 이벤트 발생
    /// </summary>
    /// <returns></returns>
    public bool AddDate()
    {
        bool _isAddEvent = false;

        //틱으로 이벤트 지속시간을 감소하고 0이면 비활성화
        for (int i = m_state.m_activeEventList.Count - 1; i >= 0; i--)
        {
            if (m_state.m_activeEventList[i].Tick(m_gameDataManager))
            {
                m_state.m_activeEventList.RemoveAt(i);
            }
        }

        //그룹 상태 딕셔너리 가져옴
        foreach (KeyValuePair<int ,EventGroupState> groupStateitem in m_groupStateDic)
        {
            //그룹 상태 딕셔너리 확률에 날마다 변하는 확률 추가
            groupStateitem.Value.m_percent += m_groupDataDic[groupStateitem.Key].m_dateChangePercent;
            Debug.Log("Event Group Key: " + groupStateitem.Key + ", Percent: " + groupStateitem.Value.m_percent);

            //만약 최대 이벤트 갯수이면 다음으로
            if (m_state.m_activeEventList.Count >= m_state.m_maxEvent)
            {
                continue;
            }

            //그 확률을 이용해 그룹 이벤트 활성화 여부 판단
            if (ProbabilityUtils.RollPercent(groupStateitem.Value.m_percent) == true)
            {
                //슬롯이 모두 차 있을 때 생성된 경우에 생성 무시하고 확률 초기화
                if (m_state.m_activeEventList.Count >= m_state.m_maxEvent)
                {
                    groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;
                    continue;
                }

                //만약 활성화 되면 원래 확률로 돌아감
                groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;

                //그룹의 랜덤 이벤트의 이펙트 활성화
                EventData _eventData = ProbabilityUtils.GetRandomElement(m_groupDataDic[groupStateitem.Key].m_dataList);
                //조건을 확인하고 조건을 만족하지 않으면 이벤트 차단
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
