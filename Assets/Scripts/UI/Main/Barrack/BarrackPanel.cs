using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrackPanel : MonoBehaviour, IUIPanel
{
    private GameDataManager m_gameDataManager = null;
    private MainUIManager m_mainUIManager = null;

    public GameDataManager GameDataManager => m_gameDataManager;
    public MainUIManager MainUIManager => m_mainUIManager;

    public void OnOpen(GameDataManager argdataManager, MainUIManager argMainUIManager)
    {
        m_gameDataManager = argdataManager;
        m_mainUIManager = argMainUIManager;


    }

    public void OnClose()
    {

    }
}
