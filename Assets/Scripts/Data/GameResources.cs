using System.Collections.Generic;

[System.Serializable]
public class GameResources
{
    public long m_wood;
    public long m_iron;
    public long m_food;
    public long m_tech;

    public GameResources()
    {
        m_wood = 0;
        m_iron = 0;
        m_food = 0;
        m_tech = 0;
    }

    public GameResources(IEnumerable<ResourceAmount> amounts)
        : this()
    {
        Add(amounts);
    }

    public GameResources(GameResources other)
    {
        m_wood = other?.m_wood ?? 0;
        m_iron = other?.m_iron ?? 0;
        m_food = other?.m_food ?? 0;
        m_tech = other?.m_tech ?? 0;
    }

    public void Clear()
    {
        m_wood = 0;
        m_iron = 0;
        m_food = 0;
        m_tech = 0;
    }

    public long GetAmount(ResourceType.TYPE type)
    {
        switch (type)
        {
            case ResourceType.TYPE.Wood: return m_wood;
            case ResourceType.TYPE.Iron: return m_iron;
            case ResourceType.TYPE.Food: return m_food;
            case ResourceType.TYPE.Tech: return m_tech;
            default: return 0;
        }
    }

    public void SetAmount(ResourceType.TYPE type, long amount)
    {
        if (amount < 0) amount = 0;
        switch (type)
        {
            case ResourceType.TYPE.Wood: m_wood = amount; break;
            case ResourceType.TYPE.Iron: m_iron = amount; break;
            case ResourceType.TYPE.Food: m_food = amount; break;
            case ResourceType.TYPE.Tech: m_tech = amount; break;
        }
    }

    public void Add(ResourceType.TYPE type, long delta)
    {
        if (delta == 0) return;
        long current = GetAmount(type);
        SetAmount(type, current + delta);
    }

    public void Add(IEnumerable<ResourceAmount> rewards)
    {
        if (rewards == null) return;
        foreach (var item in rewards)
        {
            if (item == null) continue;
            Add(item.m_type, item.m_amount);
        }
    }

    public bool CanAfford(IEnumerable<ResourceAmount> costs)
    {
        if (costs == null) return true;
        foreach (var cost in costs)
        {
            if (cost == null) continue;
            if (GetAmount(cost.m_type) < cost.m_amount) return false;
        }
        return true;
    }

    public bool TrySpend(IEnumerable<ResourceAmount> costs)
    {
        if (!CanAfford(costs)) return false;
        foreach (var cost in costs)
        {
            if (cost == null) continue;
            Add(cost.m_type, -cost.m_amount);
        }
        return true;
    }

    public List<ResourceAmount> ToList()
    {
        return new List<ResourceAmount>
        {
            new ResourceAmount(ResourceType.TYPE.Wood, m_wood),
            new ResourceAmount(ResourceType.TYPE.Iron, m_iron),
            new ResourceAmount(ResourceType.TYPE.Food, m_food),
            new ResourceAmount(ResourceType.TYPE.Tech, m_tech),
        };
    }

    public static GameResources FromList(IEnumerable<ResourceAmount> amounts)
    {
        return new GameResources(amounts);
    }
} 