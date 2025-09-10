using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMainPanel : BasePanel
{
    protected override void OnPanelOpen()
    {
        // TitleMainPanel 초기화 로직이 필요한 경우 여기에 추가
        SetPanelName("Title Main");
    }

    protected override void OnPanelClose()
    {
        // TitleMainPanel 정리 로직이 필요한 경우 여기에 추가
    }
}
