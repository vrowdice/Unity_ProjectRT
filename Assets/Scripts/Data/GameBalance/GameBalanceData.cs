using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameBalanceData", menuName = "Game Balance Data")]
public class GameBalanceData : ScriptableObject
{
    [Header("Common")]
    public float m_dateBalanceMul = 1.03f;
    public int m_maxDate = 200;

    [Header("Balance Type")]
    public List<BalanceTypeBalance> m_balanceTypeMulList = new();
    public BalanceType.TYPE m_firstBalanceType = BalanceType.TYPE.King;

    [Header("Building")]
    [Range(0f, 1f)]
    public float m_buildingRefundRate = 0.8f; // 80% refund

    [Header("Request")]
    public List<RequestTypeBalance> m_requestTypeBalanceList = new();
    public List<int> m_forcedContactRequestList = new();
    public int m_maxRequest = 5;
    public int m_makeRequestDate = 5;
    //첫 번째 컨텍을 할 확률
    public float m_firstContactPer = 70.0f;
    //한 의뢰 사이클의 두 번째 이상 컨택이 생길 확률
    public float m_overSecondContactPer = 25.0f;
    //두번 이상 컨텍이 없을 경우 오르는 확률
    public float m_noContactChangePer = 15.0f;

    [Header("Event")]
    public int m_firstEventSlot = 2;
    public int m_maxEventSlot = 2;

    private Dictionary<BalanceType.TYPE, BalanceTypeBalance> m_balanceTypeDictionary;
    private Dictionary<RequestType.TYPE, RequestTypeBalance> m_requestTypeDictionary;

    public void InitializeDict()
    {
        m_balanceTypeDictionary = new Dictionary<BalanceType.TYPE, BalanceTypeBalance>();
        foreach (var balanceEntry in m_balanceTypeMulList)
        {
            if (!m_balanceTypeDictionary.ContainsKey(balanceEntry.m_royalRank))
            {
                m_balanceTypeDictionary.Add(balanceEntry.m_royalRank, balanceEntry);
            }
            else
            {
                Debug.LogWarning(ExceptionMessages.ErrorDuplicateEntryNotAllowed + balanceEntry.m_royalRank);
            }
        }

        m_requestTypeDictionary = new Dictionary<RequestType.TYPE, RequestTypeBalance>();
        foreach (var requestEntry in m_requestTypeBalanceList)
        {
            if (!m_requestTypeDictionary.ContainsKey(requestEntry.m_type))
            {
                m_requestTypeDictionary.Add(requestEntry.m_type, requestEntry);
            }
            else
            {
                Debug.LogWarning(ExceptionMessages.ErrorDuplicateEntryNotAllowed + requestEntry.m_type);
            }
        }
    }

    public BalanceTypeBalance GetBalanceTypeBalance(BalanceType.TYPE type)
    {
        if (m_balanceTypeDictionary.TryGetValue(type, out BalanceTypeBalance balance))
        {
            return balance;
        }
        Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + type);
        return null;
    }
    public RequestTypeBalance GetRequestTypeBalance(RequestType.TYPE type)
    {
        if (m_requestTypeDictionary.TryGetValue(type, out RequestTypeBalance request))
        {
            return request;
        }
        Debug.LogWarning(ExceptionMessages.ErrorNoSuchType + type);
        return null;
    }
}
