using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrackPanel : BasePanel
{
    private const int m_barrackBuildingCode = 10003;

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Barrack");
        SetBuildingLevel(m_barrackBuildingCode);

        // 여기에 병영 패널 특화 초기화 로직을 추가할 수 있습니다
        InitializeBarrackPanel();
    }

    /// <summary>
    /// 병영 패널 초기화
    /// </summary>
    private void InitializeBarrackPanel()
    {
        // 병영 패널 특화 로직을 여기에 구현
        // 예: 유닛 목록 표시, 훈련 기능 등
    }
}
