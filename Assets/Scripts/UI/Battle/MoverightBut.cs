using UnityEngine;

public class MoverightBut : MonoBehaviour
{
    public void OnClick_ToggleView()
    {
        // BattleSystemManager�� ToggleView() �޼��带 ȣ���Ͽ�
        // �Ʊ�/�� ���� ȭ���� ��ȯ�մϴ�.
        BattleSystemManager.Instance?.ToggleView();
    }
}