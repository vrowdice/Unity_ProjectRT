using UnityEngine;

[CreateAssetMenu(fileName = "NewGameBalanceData", menuName = "Game Balance Data")]
public class GameBalanceData : ScriptableObject
{
    [Header("Global Refund Settings")]
    [Range(0f, 1f)]
    public float m_buildingRefundRate = 0.8f; // 80% refund

    [Header("Other Global Balances")]
    public float m_resourceGainMultiplier = 1f;
    public float m_buildingUpgradeCostMultiplier = 1f;

    // Add more global balancing fields as needed
}
