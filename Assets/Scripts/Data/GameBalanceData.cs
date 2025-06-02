using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameBalanceData", menuName = "Game Balance Data")]
public class GameBalanceData : ScriptableObject
{
    [System.Serializable]
    public class BalanceTypeBalance
    {
        public BalanceType m_royalRank;
        public float m_mul;

        public BalanceTypeBalance(BalanceType rank, float mulValue)
        {
            m_royalRank = rank;
            m_mul = mulValue;
        }
    }

    [System.Serializable]
    public class RequestTypeBalance
    {
        public RequestType m_type;
        public int m_randomResourceTypeCount;
        public int m_randomMinResourceAmountMul;
        public int m_randomMaxResourceAmountMul;
        public float m_likeMul;
        public float m_wealthTokenMul;
        public float m_exchangeTokenMul;

        public RequestTypeBalance(
            RequestType type,
            int randomResourceTypeCount,
            int randomMinResourceAmountMul,
            int randomMaxResourceAmountMul,
            float likeMul,
            float wealthTokenMul,
            float exchangeTokenMul)
        {
            m_type = type;
            m_randomResourceTypeCount = randomResourceTypeCount;
            m_randomMinResourceAmountMul = randomMinResourceAmountMul;
            m_randomMaxResourceAmountMul = randomMaxResourceAmountMul;
            m_likeMul = likeMul;
            m_wealthTokenMul = wealthTokenMul;
            m_exchangeTokenMul = exchangeTokenMul;
        }
    }

    [Range(0f, 1f)]
    public float m_buildingRefundRate = 0.8f; // 80% refund

    public List<BalanceTypeBalance> m_balanceTypeMulList = new List<BalanceTypeBalance>();

    public List<RequestTypeBalance> m_requestTypeBalanceList = new List<RequestTypeBalance>();

    public int m_maxRequest = 5;
}
