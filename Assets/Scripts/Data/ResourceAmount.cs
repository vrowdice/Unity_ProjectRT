[System.Serializable]
public class ResourceAmount
{
    public ResourceType m_type;
    public long m_amount;

    public ResourceAmount(ResourceType argResourceType, long argAmount)
    {
        m_type = argResourceType;
        m_amount = argAmount;
    }
}
