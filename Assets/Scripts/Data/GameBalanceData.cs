using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameBalanceData", menuName = "Game Balance Data")]
public class GameBalanceData : ScriptableObject
{
    [System.Serializable]
    public class BalanceTypeMul
    {
        public BalanceType m_royalRank; // Corrected to RoyalRank
        public float m_mul;

        // Constructor for easy initialization
        public BalanceTypeMul(BalanceType rank, float mulValue)
        {
            m_royalRank = rank;
            m_mul = mulValue;
        }
    }

    [Range(0f, 1f)]
    public float m_buildingRefundRate = 0.8f; // 80% refund

    public List<BalanceTypeMul> m_balanceTypeMulList = new List<BalanceTypeMul>();

    public int m_maxRequest = 5;

    // Add more global balancing fields as needed

    // Method to initialize the list (called by the custom editor)
    public void InitializeBalanceTypeMulList()
    {
        // Only initialize if the list is empty or significantly out of sync
        // This prevents overwriting manually set values
        if (m_balanceTypeMulList == null || m_balanceTypeMulList.Count == 0 || m_balanceTypeMulList.Count != BalanceType.GetValues(typeof(BalanceType)).Length)
        {
            m_balanceTypeMulList = new List<BalanceTypeMul>(); // Ensure it's not null and cleared

            foreach (BalanceType rank in BalanceType.GetValues(typeof(BalanceType)))
            {
                // Check if this rank already exists to avoid duplicates if re-initializing
                if (!m_balanceTypeMulList.Exists(item => item.m_royalRank == rank))
                {
                    m_balanceTypeMulList.Add(new BalanceTypeMul(rank, 1.0f));
                }
            }
        }
        else
        {
            // Optional: If the list isn't empty, check for missing ranks and add them
            foreach (BalanceType rank in BalanceType.GetValues(typeof(BalanceType)))
            {
                if (!m_balanceTypeMulList.Exists(item => item.m_royalRank == rank))
                {
                    m_balanceTypeMulList.Add(new BalanceTypeMul(rank, 1.0f));
                }
            }
        }
    }
}
