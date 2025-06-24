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
        }

        foreach(KeyValuePair<int, EventGroupData> item in m_groupDataDic)
        {
            m_groupStateDic[item.Key] = new EventGroupState(item.Key, item.Value.m_firstPercent);
        }
    }

    public void NextDate()
    {
        //�׷� ���� ��ųʸ� ������
        foreach (KeyValuePair<int ,EventGroupState> groupStateitem in m_groupStateDic)
        {
            //�׷� ���� ��ųʸ� Ȯ���� ������ ���ϴ� Ȯ�� �߰�
            groupStateitem.Value.m_percent += m_groupDataDic[groupStateitem.Key].m_dateChangePercent;

            //���� �ִ� �̺�Ʈ �����̸� ��������
            if (m_state.m_activeEffectList.Count >= m_state.m_maxEvent)
            {
                continue;
            }

            //�� Ȯ���� �̿��� �׷� �̺�Ʈ Ȱ��ȭ ���� �Ǵ�
            if (ProbabilityUtils.RollPercent(groupStateitem.Value.m_percent) == true)
            {
                //���� Ȱ��ȭ �Ǹ� ���� Ȯ���� ���ư�
                groupStateitem.Value.m_percent = m_groupDataDic[groupStateitem.Key].m_firstPercent;

                //�׷��� ���� �̺�Ʈ�� ����Ʈ Ȱ��ȭ
                EventData _eventData = ProbabilityUtils.GetRandomElement(m_groupDataDic[groupStateitem.Key].m_dataList);
                _eventData.ActivateAllEffect(m_gameDataManager, Random.Range(_eventData.m_minDuration, _eventData.m_maxDuration));
                foreach(EffectBase effectBaseItem in _eventData.m_effectList)
                {
                    m_state.m_activeEffectList.Add(effectBaseItem);
                }
            }
        }

        //ƽ���� ���ӽð��� �����ϰ� 0�̸� ��Ȱ��ȭ
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
