[System.Serializable]
public class RequestTypeBalance
{
    public RequestType m_type;
    public int m_randomResourceTypeCount;
    public int m_randomMinResourceAmountMul;
    public int m_randomMaxResourceAmountMul;
    public float m_likeMul;
    public float m_wealthTokenMul;
    public float m_exchangeTokenMul;

    public RequestTypeBalance(
        RequestType type,
        int randomResourceTypeCount,
        int randomMinResourceAmountMul,
        int randomMaxResourceAmountMul,
        float likeMul,
        float wealthTokenMul,
        float exchangeTokenMul)
    {
        m_type = type;
        m_randomResourceTypeCount = randomResourceTypeCount;
        m_randomMinResourceAmountMul = randomMinResourceAmountMul;
        m_randomMaxResourceAmountMul = randomMaxResourceAmountMul;
        m_likeMul = likeMul;
        m_wealthTokenMul = wealthTokenMul;
        m_exchangeTokenMul = exchangeTokenMul;
    }
}