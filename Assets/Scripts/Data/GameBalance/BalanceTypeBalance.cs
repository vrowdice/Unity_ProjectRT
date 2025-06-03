[System.Serializable]
public class BalanceTypeBalance
{
    public BalanceType m_royalRank;
    public float m_mul;

    public BalanceTypeBalance(BalanceType rank, float mulValue)
    {
        m_royalRank = rank;
        m_mul = mulValue;
    }
}
