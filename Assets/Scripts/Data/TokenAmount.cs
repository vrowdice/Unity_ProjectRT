using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenAmount
{
    public TokenType.TYPE m_type;
    public long m_amount;

    public TokenAmount(TokenType.TYPE argTokenType, long argAmount)
    {
        m_type = argTokenType;
        m_amount = argAmount;
    }
}
