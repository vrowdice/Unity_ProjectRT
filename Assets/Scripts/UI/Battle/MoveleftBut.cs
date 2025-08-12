using UnityEngine;

public class MoveleftBut : MonoBehaviour
{
    public void OnClick_ToggleView()
    {
        BattleSystemManager.Instance?.ToggleView();
    }
}