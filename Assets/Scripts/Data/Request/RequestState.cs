using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestState
{
    public RequestType m_requestType;
    public FactionType m_factionType;
    public List<ResourceAmount> m_resourceReward = new List<ResourceAmount>();
    public int m_factionAddLike = 0;
    public int m_wealthToken = 0;
    public int m_exchangeToken = 0;

    public RequestState(int argDate, int argNowLike, RequestType argRequestType, FactionType argfactionType, GameBalanceEntry argGameBalanceEntry)
    {
        m_requestType = argRequestType;
        m_factionType = argfactionType;

        m_resourceReward = argGameBalanceEntry.CalRequestResources(m_requestType);
        m_factionAddLike = argGameBalanceEntry.CalRequestFactionLike(argNowLike, m_requestType);
        m_wealthToken = argGameBalanceEntry.CalRequestWealthToken(argDate, m_requestType);
        m_exchangeToken = argGameBalanceEntry.CalRequestExchangeToken(argDate, m_requestType);

        Debug.Log($"[Request Init] m_requestType: {m_requestType}");
        Debug.Log($"[Request Init] m_factionType: {m_factionType}");
        Debug.Log($"[Request Init] m_resourceReward: {m_resourceReward}");
        Debug.Log($"[Request Init] m_factionAddLike: {m_factionAddLike}");
        Debug.Log($"[Request Init] m_wealthToken: {m_wealthToken}");
        Debug.Log($"[Request Init] m_exchangeToken: {m_exchangeToken}");
    }
}