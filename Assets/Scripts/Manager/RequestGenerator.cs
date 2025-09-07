using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 요청 생성을 담당하는 클래스
/// </summary>
public static class RequestGenerator
{
    /// <summary>
    /// 랜덤 요청을 생성
    /// 일반 요청과 연락 요청을 모두 생성
    /// </summary>
    public static void MakeRandomRequest(
        List<RequestState> acceptableRequestList,
        Dictionary<FactionType.TYPE, FactionEntry> factionEntryDict,
        GameBalanceEntry gameBalanceEntry,
        RequestLineTemplate contactLineTemplate,
        Dictionary<RequestType.TYPE, RequestLineTemplate> requestLineTemplateDic)
    {
        acceptableRequestList.Clear();
        GenerateNormalRequests(acceptableRequestList, factionEntryDict, gameBalanceEntry, requestLineTemplateDic);
        GenerateContactRequests(acceptableRequestList, factionEntryDict, gameBalanceEntry, contactLineTemplate);
    }

    /// <summary>
    /// 일반 요청 생성
    /// </summary>
    private static void GenerateNormalRequests(
        List<RequestState> acceptableRequestList,
        Dictionary<FactionType.TYPE, FactionEntry> factionEntryDict,
        GameBalanceEntry gameBalanceEntry,
        Dictionary<RequestType.TYPE, RequestLineTemplate> requestLineTemplateDic)
    {
        List<FactionType.TYPE> haveFactionTypes = GetHaveFactionTypeList(factionEntryDict);

        // 요청 개수만큼 랜덤한 요청을 생성
        for (int i = 0; i < gameBalanceEntry.m_data.m_maxRequest; i++)
        {
            if (haveFactionTypes.Count == 0) break;

            FactionType.TYPE factionType = ProbabilityUtils.GetRandomElement(haveFactionTypes);
            int like = factionEntryDict.TryGetValue(factionType, out FactionEntry entry) ? entry.m_state.m_like : 0;

            RequestType.TYPE requestType = ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>());
            RequestLineTemplate template = requestLineTemplateDic.TryGetValue(requestType, out var lineTemplate) ? lineTemplate : null;

            acceptableRequestList.Add(new RequestState(
                false,
                GameManager.Instance.Date,
                like,
                requestType,
                factionType,
                gameBalanceEntry,
                template));

            // 같은 팩션의 요청이 중복되면 그 팩션을 제거
            haveFactionTypes.Remove(factionType);
        }
    }

    /// <summary>
    /// 연락 요청을 랜덤하게 생성
    /// 첫 번째 연락 확률과 두 번째 연락 확률이 다르게 적용
    /// </summary>
    public static void GenerateContactRequests(
        List<RequestState> acceptableRequestList,
        Dictionary<FactionType.TYPE, FactionEntry> factionEntryDict,
        GameBalanceEntry gameBalanceEntry,
        RequestLineTemplate contactLineTemplate)
    {
        List<FactionType.TYPE> factionTypes = factionEntryDict.Keys.ToList();
        factionTypes.Remove(FactionType.TYPE.None);

        if (factionTypes.Count == 0) return;

        // 첫 번째 팩션 타입 선택
        FactionType.TYPE type = ProbabilityUtils.GetRandomElement(factionTypes);

        // 첫 번째 연락 확률
        float per = gameBalanceEntry.m_state.m_noContactCount *
            gameBalanceEntry.m_data.m_noContactChangePer +
            gameBalanceEntry.m_data.m_firstContactPer;

        // 첫 번째 연락
        if (ProbabilityUtils.RollPercent(per))
        {
            RequestType.TYPE requestType = ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>());
            int like = factionEntryDict.TryGetValue(type, out FactionEntry entry) ? entry.m_state.m_like : 0;

            acceptableRequestList.Add(new RequestState(
                true,
                GameManager.Instance.Date,
                like,
                requestType,
                type,
                gameBalanceEntry,
                contactLineTemplate));

            factionTypes.Remove(type);

            // 두 번째 팩션 타입 선택
            if (factionTypes.Count > 0)
            {
                type = ProbabilityUtils.GetRandomElement(factionTypes);

                // 두 번째 연락 확률
                per = gameBalanceEntry.m_state.m_noContactCount *
                    gameBalanceEntry.m_data.m_noContactChangePer +
                    gameBalanceEntry.m_data.m_overSecondContactPer;

                // 두 번째 연락
                if (ProbabilityUtils.RollPercent(per))
                {
                    requestType = ProbabilityUtils.GetRandomElement(EnumUtils.GetAllEnumValues<RequestType.TYPE>());
                    like = factionEntryDict.TryGetValue(type, out entry) ? entry.m_state.m_like : 0;

                    acceptableRequestList.Add(new RequestState(
                        true,
                        GameManager.Instance.Date,
                        like,
                        requestType,
                        type,
                        gameBalanceEntry,
                        contactLineTemplate));
                }
            }

            gameBalanceEntry.m_state.m_noContactCount = 0;
        }
    }

    /// <summary>
    /// 보유하고 있는 팩션들의 타입 리스트를 반환
    /// </summary>
    private static List<FactionType.TYPE> GetHaveFactionTypeList(Dictionary<FactionType.TYPE, FactionEntry> factionEntryDict)
    {
        List<FactionType.TYPE> factionTypes = factionEntryDict.Keys.ToList();

        for (int i = factionTypes.Count - 1; i >= 0; i--)
        {
            FactionType.TYPE item = factionTypes[i];

            if (item == FactionType.TYPE.None)
            {
                continue;
            }

            if (factionEntryDict.TryGetValue(item, out FactionEntry entry) && entry.m_state.m_like <= 0)
            {
                factionTypes.RemoveAt(i);
            }
        }

        return factionTypes;
    }
} 