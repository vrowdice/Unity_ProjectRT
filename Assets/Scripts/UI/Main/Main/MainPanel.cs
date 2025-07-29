using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanel : BasePanel
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPanelOpen()
    {
        // 메인 패널은 정보 패널을 표시하지 않음
        m_showInfoPanel = false;
        SetPanelName("");
        SetBuildingLevel("");
    }
}
