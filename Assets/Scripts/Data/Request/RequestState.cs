using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestState
{
    public RequestCompleteCondition m_requestCompleteCondition;
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
        GameBalanceEntry argGameBalanceEntry,
        RequestLineTemplate argRequestLineTemplate)
    {
        m_requestCompleteCondition = new RequestCompleteCondition(
            argRequestType,
            argGameBalanceEntry.m_state.m_mainMul,
            argGameBalanceEntry.m_state.m_dateMul);

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

        m_title = ReplacePlaceholders(argRequestLineTemplate.m_titleTemplate);
        m_description = GetRandomContentTemplat(argRequestLineTemplate);
    }

    public string GetRandomContentTemplat(RequestLineTemplate argRequestLineTemplate)
    {
        if (argRequestLineTemplate.m_contentTemplates == null || argRequestLineTemplate.m_contentTemplates.Count == 0)
        {
            return string.Empty;
        }

        int _randomIndex = Random.Range(0, argRequestLineTemplate.m_contentTemplates.Count);
        string _temp = argRequestLineTemplate.m_contentTemplates[_randomIndex];

        _temp = ReplacePlaceholders(_temp);

        return _temp;
    }

    public string ReplacePlaceholders(string argStr)
    {
        string _str = argStr;

        _str = _str.Replace("{faction}", m_factionType.ToString());
        _str = _str.Replace("{resource}", ((ResourceType.TYPE)m_requestCompleteCondition.m_completeTargetInfo).ToString());
        _str = _str.Replace("{enemy}", "");
        _str = _str.Replace("{territory}", "");

        return _str;
    }
}