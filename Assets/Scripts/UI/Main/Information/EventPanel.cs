using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] GameObject m_eventInfoPanelPrefeb = null;
    [SerializeField] Transform m_eventInfoScrollViewContentTrans = null;

    private GameDataManager m_gameDataManager = null;
    private MainUIManager m_mainUIManager = null;

    public GameDataManager GameDataManager => m_gameDataManager;
    public MainUIManager MainUIManager => m_mainUIManager;

    public void OnOpen(GameDataManager argdataManager, MainUIManager argMainUIManager)
    {
        m_gameDataManager = argdataManager;
        m_mainUIManager = argMainUIManager;

        GameObjectUtils.ClearChildren(m_eventInfoScrollViewContentTrans);

        foreach(ActiveEvent activeEvent in GameDataManager.EventEntry.m_state.m_activeEventList)
        {
            EventInfoPanel _infoPanel; 
            _infoPanel = Instantiate(m_eventInfoPanelPrefeb, m_eventInfoScrollViewContentTrans).GetComponent<EventInfoPanel>();
            _infoPanel.Init(activeEvent);
        }
    }

    public void OnClose()
    {

    }
}
