using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestState
{
    public RequestType.TYPE m_requestType;
    public FactionType.TYPE m_factionType;
    public List<ResourceAmount> m_resourceRewardList = new();
    public List<TokenAmount> m_tokenRewardList = new();
    public bool m_isContact = false;
    public int m_factionAddLike = 0;
    public string m_title = string.Empty;
    public string m_description = string.Empty;

    public RequestState(
        bool argIsContact,
        int argDate,
        int argNowLike,
        RequestType.TYPE argRequestType,
        FactionType.TYPE argFactionType,
        GameBalanceEntry argGameBalanceEntry)
    {
        m_isContact = argIsContact;
        m_requestType = argRequestType;
        m_factionType = argFactionType;

        m_resourceRewardList = argGameBalanceEntry.CalRequestResources(m_requestType);
        m_tokenRewardList = argGameBalanceEntry.CalRequestTokens(argDate, m_requestType);

        if(argIsContact == true)
        {
            m_factionAddLike = 10;
        }
        else
        {
            if(argFactionType != FactionType.TYPE.None)
            {
                m_factionAddLike = argGameBalanceEntry.CalRequestFactionLike(argNowLike, m_requestType);
            }
            else
            {
                m_factionAddLike = 0;
            }
        }
    }
}