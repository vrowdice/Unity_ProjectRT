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

    public RequestState(List<FactionType> availableFactionTypes, long minResourceAmount, long maxResourceAmount)
    {
        // 1. Randomly select a RequestType
        m_requestType = GetRandomRequestType();

        // 2. Select a FactionType from the provided list
        if (availableFactionTypes != null && availableFactionTypes.Count > 0)
        {
            m_factionType = availableFactionTypes[Random.Range(0, availableFactionTypes.Count)];
        }
        else
        {
            Debug.LogWarning("No available faction types provided. Assigning a default FactionType.");
            m_factionType = FactionType.None;
        }

        // 3. Generate two unique ResourceAmount rewards
        m_resourceReward = GenerateUniqueResourceRewards(2, minResourceAmount, maxResourceAmount);
    }

    private RequestType GetRandomRequestType()
    {
        System.Array requestTypes = System.Enum.GetValues(typeof(RequestType));
        return (RequestType)requestTypes.GetValue(Random.Range(0, requestTypes.Length));
    }

    private List<ResourceAmount> GenerateUniqueResourceRewards(int count, long minAmount, long maxAmount)
    {
        List<ResourceAmount> rewards = new List<ResourceAmount>();
        List<ResourceType> availableResourceTypes = new List<ResourceType>(System.Enum.GetValues(typeof(ResourceType)) as ResourceType[]);

        if (availableResourceTypes.Count < count)
        {
            Debug.LogWarning("Not enough unique resource types to generate the requested count of rewards. Generating fewer rewards.");
            count = availableResourceTypes.Count;
        }

        for (int i = 0; i < count; i++)
        {
            if (availableResourceTypes.Count == 0)
            {
                break; // No more unique resource types to choose from
            }

            int randomIndex = Random.Range(0, availableResourceTypes.Count);
            ResourceType selectedType = availableResourceTypes[randomIndex];
            long randomAmount = Random.Range((int)minAmount, (int)maxAmount + 1); // Cast to int for Random.Range, then back to long

            rewards.Add(new ResourceAmount(selectedType, randomAmount));
            availableResourceTypes.RemoveAt(randomIndex); // Remove to ensure uniqueness
        }

        return rewards;
    }
}