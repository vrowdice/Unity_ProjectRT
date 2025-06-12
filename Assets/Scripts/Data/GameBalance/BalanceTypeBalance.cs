[System.Serializable]
public class BalanceTypeBalance
{
    public BalanceType.TYPE m_royalRank;
    public float m_mul;

    public BalanceTypeBalance(BalanceType.TYPE rank, float mulValue)
    {
        m_royalRank = rank;
        m_mul = mulValue;
    }
}
