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

    public int CalRequestFactionLike(int argNowLike, RequestType.TYPE argRequestType)
    {
        Debug.Log((int)(m_state.m_dateMul / m_state.m_mainMul + argNowLike / m_data.GetRequestTypeBalance(argRequestType).m_likeMul));

        return (int)(m_state.m_dateMul / m_state.m_mainMul + argNowLike / m_data.GetRequestTypeBalance(argRequestType).m_likeMul);
    }

    public List<ResourceAmount> CalRequestResources(RequestType.TYPE argRequestType)
    {
        List<ResourceAmount> rewards = new();
        List<ResourceType.TYPE> availableResourceTypes = new(System.Enum.GetValues(typeof(ResourceType.TYPE)) as ResourceType.TYPE[]);

        RequestTypeBalance _RequestTypeBalance = m_data.GetRequestTypeBalance(argRequestType);

        for (int i = 0; i < _RequestTypeBalance.m_randomResourceTypeCount; i++)
        {
            int randomIndex = Random.Range(0, availableResourceTypes.Count);
            ResourceType.TYPE selectedType = availableResourceTypes[randomIndex];

            long randomAmountMul = Random.Range((int)_RequestTypeBalance.m_randomMinResourceAmountMul,
                (int)_RequestTypeBalance.m_randomMaxResourceAmountMul + 1);

            randomAmountMul = (long)(((randomAmountMul * m_state.m_dateMul) * 5 / m_state.m_mainMul) * m_state.m_rewardMul);

            rewards.Add(new ResourceAmount(selectedType, randomAmountMul));
            availableResourceTypes.RemoveAt(randomIndex);
        }

        return rewards;
    }

    public List<TokenAmount> CalRequestTokens(int argDateMul, RequestType.TYPE argRequestType)
    {
        List<TokenAmount> _list = new();

        int _amount = CalRequestExchangeToken(argDateMul, argRequestType);
        if(_amount != 0)
        {
            TokenAmount _tokenAmount = new(TokenType.TYPE.ExchangeToken, _amount);
            _list.Add(_tokenAmount);
        }

        _amount = CalRequestWealthToken(argDateMul, argRequestType);
        if(_amount != 0)
        {
            TokenAmount _tokenAmount = new(TokenType.TYPE.WealthToken, _amount);
            _list.Add(_tokenAmount);
        }

        return _list;
    }

    public int CalRequestExchangeToken(int argDateMul ,RequestType.TYPE argRequestType)
    {
        RequestTypeBalance _RequestTypeBalance = m_data.GetRequestTypeBalance(argRequestType);

        if(_RequestTypeBalance.m_exchangeTokenMul == 0)
        {
            return 0;
        }

        return (int)(_RequestTypeBalance.m_exchangeTokenMul * m_state.m_mainMul + argDateMul / 10);
    }

    public int CalRequestWealthToken(int argDateMul, RequestType.TYPE argRequestType)
    {
        RequestTypeBalance _RequestTypeBalance = m_data.GetRequestTypeBalance(argRequestType);

        if (_RequestTypeBalance.m_wealthTokenMul == 0)
        {
            return 0;
        }

        return (int)(_RequestTypeBalance.m_wealthTokenMul * m_state.m_mainMul + argDateMul / 10);
    }
}
