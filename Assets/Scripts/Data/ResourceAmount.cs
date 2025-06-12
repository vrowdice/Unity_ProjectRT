[System.Serializable]
public class ResourceAmount
{
    public ResourceType.TYPE m_type;
    public long m_amount;

    public ResourceAmount(ResourceType.TYPE argResourceType, long argAmount)
    {
        m_type = argResourceType;
        m_amount = argAmount;
    }
}
