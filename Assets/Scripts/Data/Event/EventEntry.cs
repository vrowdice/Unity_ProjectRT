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
        //���� ���� �ʿ�
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
    /// ���� �� ó�� Ȯ���� ���̰� �̺�Ʈ �߻�
    /// </summary>
    /// <returns></returns>
    public bool AddDate()
    {
        bool _isAddEvent = false;

        //ƽ���� �̺�Ʈ ���ӽð��� �����ϰ� 0�̸� ��Ȱ��ȭ
        for (int i = m_state.m_activeEventList.Count - 1; i >= 0; i--)
        {
            if (m_state.m_activeEventList[i].Tick(m_gameDataManager))
            {
                m_state.m_activeEventList.RemoveAt(i);
            }
        }

        //�׷� ���� ��ųʸ� ������
        foreach (KeyValuePair<int ,EventGroupState> groupStateitem in m_groupStateDic)
        {
            //�׷� ���� ��ųʸ� Ȯ���� ������ ���ϴ� Ȯ�� �߰�
            groupStateitem.Value.m_percent += m_groupDataDic[groupStateitem.Key].m_dateChangePercent;
            Debug.Log("Event Group Key: " + groupStateitem.Key + ", Percent: " + groupStateitem.Value.m_percent);

            //���� �ִ� �̺�Ʈ �����̸� ��������
            if (m_state.m_activeEventList.Count >= m_state.m_maxEvent)
            {
                continue;
            }

            //�� Ȯ���� �̿��� �׷� �̺�Ʈ Ȱ��ȭ ���� �Ǵ�
            if (ProbabilityUtils.RollPercent(groupStateitem.Value.m_percent) == true)
            {
                //������ ��� �� ���� �� ������ ��쿡 ���� �����ϰ� Ȯ�� �ʱ�ȭ
                if (m_state.m_activeEventList.Count >= m_state.m_maxEvent)
                {
                    groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;
                    continue;
                }

                //���� Ȱ��ȭ �Ǹ� ���� Ȯ���� ���ư�
                groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;

                //�׷��� ���� �̺�Ʈ�� ����Ʈ Ȱ��ȭ
                EventData _eventData = ProbabilityUtils.GetRandomElement(m_groupDataDic[groupStateitem.Key].m_dataList);
                //������ Ȯ���ϰ� ������ �������� ������ �̺�Ʈ ����
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
