using System.Collections.Generic;
using UnityEngine;

public class GameBalanceEntry
{
    public GameBalanceData m_data;
    public GameBalanceState m_state;

    public GameBalanceEntry(GameBalanceData argData, GameBalanceState argState)
    {
        m_data = argData;
        m_state = argState;

        m_state.m_mainMul = m_data.GetBalanceTypeBalance(m_data.m_firstBalanceType).m_mul;
    }

    public int CalRequestFactionLike(int argNowLike, RequestType argRequestType)
    {
        if(argRequestType == RequestType.Contact)
        {
            return 1;
        }

        return (int)(m_state.m_dateMul / m_state.m_mainMul + argNowLike / m_data.GetRequestTypeBalance(argRequestType).m_likeMul);
    }

    public List<ResourceAmount> CalRequestResources(RequestType argRequestType)
    {
        List<ResourceAmount> rewards = new List<ResourceAmount>();
        List<ResourceType> availableResourceTypes = new List<ResourceType>(System.Enum.GetValues(typeof(ResourceType)) as ResourceType[]);

        RequestTypeBalance _requestTypeBalance = m_data.GetRequestTypeBalance(argRequestType);

        for (int i = 0; i < _requestTypeBalance.m_randomResourceTypeCount; i++)
        {
            int randomIndex = Random.Range(0, availableResourceTypes.Count);
            ResourceType selectedType = availableResourceTypes[randomIndex];

            long randomAmountMul = Random.Range((int)_requestTypeBalance.m_randomMinResourceAmountMul,
                (int)_requestTypeBalance.m_randomMaxResourceAmountMul + 1);

            randomAmountMul = (long)(((randomAmountMul * m_state.m_dateMul) * 5 / m_state.m_mainMul) * m_state.m_rewardMul);

            rewards.Add(new ResourceAmount(selectedType, randomAmountMul));
            availableResourceTypes.RemoveAt(randomIndex);
        }

        return rewards;
    }

    public int CalRequestExchangeToken(int argDate ,RequestType argRequestType)
    {
        RequestTypeBalance _requestTypeBalance = m_data.GetRequestTypeBalance(argRequestType);

        if(_requestTypeBalance.m_exchangeTokenMul == 0)
        {
            return 0;
        }

        return (int)(_requestTypeBalance.m_exchangeTokenMul * m_state.m_mainMul + argDate / 10);
    }

    public int CalRequestWealthToken(int argDate, RequestType argRequestType)
    {
        RequestTypeBalance _requestTypeBalance = m_data.GetRequestTypeBalance(argRequestType);

        if (_requestTypeBalance.m_wealthTokenMul == 0)
        {
            return 0;
        }

        return (int)(_requestTypeBalance.m_wealthTokenMul * m_state.m_mainMul + argDate / 10);
    }
}
