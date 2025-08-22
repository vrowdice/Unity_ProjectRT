using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanel : BasePanel
{
    [Header("MainPanel")]
    [SerializeField]
    MinimapPanel m_minimapPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        // 초기화 로직이 필요한 경우 여기에 추가
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPanelOpen()
    {
        SetPanelName("");
        SetBuildingLevel("");
    }

    public void ClickSkipBtn()
    {

    }

    public void ClickMinimapBtn()
    {
        if (m_minimapPanel != null)
        {
            m_minimapPanel.OpenMinimap(m_gameDataManager, m_mainUIManager);
        }
        else
        {
            Debug.LogError("MinimapPanel is not assigned.");
        }
    }
}
