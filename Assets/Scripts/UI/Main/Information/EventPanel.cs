using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPanel : BasePanel
{
    [SerializeField] GameObject m_eventInfoPanelPrefeb = null;
    [SerializeField] Transform m_eventInfoScrollViewContentTrans = null;

    private const int m_infoBuildingCode = 10004;

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Information");
        SetBuildingLevel(m_infoBuildingCode);

        InitializeEventPanels();
    }

    /// <summary>
    /// 이벤트 패널들 초기화
    /// </summary>
    private void InitializeEventPanels()
    {
        if (m_eventInfoScrollViewContentTrans == null)
        {
            Debug.LogError("Event info scroll view content transform is null!");
            return;
        }

        if (m_eventInfoPanelPrefeb == null)
        {
            Debug.LogError("Event info panel prefab is null!");
            return;
        }

        // 기존 패널들 제거
        GameObjectUtils.ClearChildren(m_eventInfoScrollViewContentTrans);

        // 활성 이벤트가 없으면 리턴
        if (GameDataManager?.EventEntry?.m_state?.m_activeEventList == null)
        {
            Debug.LogWarning("No active events found or EventEntry is null.");
            return;
        }

        // 이벤트 패널들 생성
        CreateEventPanels();
    }

    /// <summary>
    /// 이벤트 패널들 생성
    /// </summary>
    private void CreateEventPanels()
    {
        foreach(ActiveEvent activeEvent in GameDataManager.EventEntry.m_state.m_activeEventList)
        {
            if (activeEvent == null)
            {
                Debug.LogWarning("ActiveEvent is null, skipping...");
                continue;
            }

            GameObject panelObj = Instantiate(m_eventInfoPanelPrefeb, m_eventInfoScrollViewContentTrans);
            EventInfoPanel infoPanel = panelObj.GetComponent<EventInfoPanel>();
            
            if (infoPanel != null)
            {
                infoPanel.Init(activeEvent);
            }
            else
            {
                Debug.LogError("EventInfoPanel component not found on prefab!");
            }
        }
    }
}
