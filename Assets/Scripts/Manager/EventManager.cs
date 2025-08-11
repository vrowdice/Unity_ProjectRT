using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 시스템을 관리하는 매니저 클래스
/// 이벤트 그룹, 활성 이벤트, 이벤트 상태 등을 관리
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("Event Data")]
    private List<EventGroupData> m_eventGroupDataList = new();
    
    // 이벤트 시스템 데이터
    private Dictionary<int, EventGroupData> m_eventGroupDataDic = new();
    private Dictionary<int, EventGroupState> m_eventGroupStateDic = new();
    private EventState m_eventState = new();
    
    // 참조
    private GameDataManager m_gameDataManager;
    
    // 프로퍼티
    public EventState EventState => m_eventState;
    public Dictionary<int, EventGroupData> EventGroupDataDict => m_eventGroupDataDic;
    
    /// <summary>
    /// 이벤트 매니저 초기화
    /// </summary>
    /// <param name="gameDataManager">게임 데이터 매니저 참조</param>
    public void Initialize(GameDataManager gameDataManager)
    {
        m_gameDataManager = gameDataManager;
        InitEventSystem();
    }
    
    /// <summary>
    /// 이벤트 시스템 초기화
    /// </summary>
    private void InitEventSystem()
    {
        // 이벤트 상태 초기화
        m_eventState.m_maxEvent = m_gameDataManager.GameBalanceEntry.m_data.m_firstEventSlot;

        // 이벤트 그룹 데이터 초기화
        foreach (EventGroupData item in m_eventGroupDataList)
        {
            item.Init(m_gameDataManager);
            m_eventGroupDataDic[item.m_eventGroupKey] = item;
        }

        // 리소스 수정 딕셔너리 초기화
        foreach (ResourceType.TYPE argType in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            m_eventState.m_territoryResourceModDic[argType] = 1.0f;
            m_eventState.m_buildingResourceModDic[argType] = 1.0f;
            m_eventState.m_territoryResourceAddDic[argType] = 0.0f;
            m_eventState.m_buildingResourceAddDic[argType] = 0.0f;
        }

        // 이벤트 그룹 상태 초기화
        foreach(KeyValuePair<int, EventGroupData> item in m_eventGroupDataDic)
        {
            m_eventGroupStateDic[item.Key] = new EventGroupState(item.Key, item.Value.m_firstPercent);
        }
    }
    
    /// <summary>
    /// 날짜 증가 처리 확률성 이벤트 발생
    /// </summary>
    /// <returns>이벤트 발생 여부</returns>
    public bool ProcessEventDate()
    {
        bool isAddEvent = false;

        // 진행중인 이벤트 지속시간을 감소시키고 0이면 비활성화
        for (int i = m_eventState.m_activeEventList.Count - 1; i >= 0; i--)
        {
            if (m_eventState.m_activeEventList[i].Tick(m_gameDataManager))
            {
                m_eventState.m_activeEventList.RemoveAt(i);
            }
        }

        // 그룹별 이벤트 확률을 증가시킴
        foreach (KeyValuePair<int, EventGroupState> groupStateItem in m_eventGroupStateDic)
        {
            // 그룹별 이벤트 확률을 확인하여 증가시킴
            groupStateItem.Value.m_percent += m_eventGroupDataDic[groupStateItem.Key].m_dateChangePercent;

            // 이미 진행중인 이벤트가 있으면 건너뜀
            if (m_eventState.m_activeEventList.Count >= m_eventState.m_maxEvent)
            {
                continue;
            }

            // 확률 확인하여 그룹 이벤트 활성화 여부 결정
            if (ProbabilityUtils.RollPercent(groupStateItem.Value.m_percent))
            {
                // 이벤트 슬롯이 가득 찬 경우 확률을 초기화하고 건너뜀
                if (m_eventState.m_activeEventList.Count >= m_eventState.m_maxEvent)
                {
                    groupStateItem.Value.m_percent = m_eventGroupDataDic[groupStateItem.Key].m_firstPercent;
                    continue;
                }

                // 이벤트 활성화되면 확률을 초기화
                groupStateItem.Value.m_percent = m_eventGroupDataDic[groupStateItem.Key].m_firstPercent;

                // 그룹에서 랜덤 이벤트를 선택하여 활성화
                EventData eventData = ProbabilityUtils.GetRandomElement(m_eventGroupDataDic[groupStateItem.Key].m_dataList);
                
                // 조건을 확인하고 조건이 만족되면 이벤트를 활성화
                bool conditionsMet = true;
                foreach (ConditionBase condition in eventData.m_conditionList)
                {
                    condition.Initialize(m_gameDataManager);
                    if (!condition.IsSatisfied())
                    {
                        conditionsMet = false;
                        break;
                    }
                }

                if (conditionsMet)
                {
                    ActiveEvent newEvent = eventData.Activate(m_gameDataManager);
                    m_eventState.m_activeEventList.Add(newEvent);
                    isAddEvent = true;
                }
            }
        }

        return isAddEvent;
    }

    /// <summary>
    /// 활성 이벤트 목록 가져오기
    /// </summary>
    /// <returns>활성 이벤트 리스트</returns>
    public List<ActiveEvent> GetActiveEvents()
    {
        return m_eventState.m_activeEventList;
    }

    /// <summary>
    /// 이벤트 그룹 데이터 설정 (에디터용)
    /// </summary>
    /// <param name="eventGroupDataList">이벤트 그룹 데이터 리스트</param>
    public void SetEventGroupDataList(List<EventGroupData> eventGroupDataList)
    {
        m_eventGroupDataList = eventGroupDataList;
    }
} 