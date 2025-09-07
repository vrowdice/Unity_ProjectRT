using UnityEngine;

public class MoverightBut : MonoBehaviour
{
    public void OnClick_ToggleView()
    {
        // BattleSystemManager의 ToggleView() 메서드를 호출하여
        // 아군/적 진영 화면을 전환합니다.
        BattleSystemManager.Instance?.ToggleView();
    }
}