using System.Collections;
using System.Collections.Generic;

public class RequestState
{
    RequestType m_requestType = new RequestType();
    FactionType m_factionType = new FactionType();
    List<ResourceAmount> m_resourceReward = new List<ResourceAmount>();
    int m_factionAddLike = 0;
    int m_wealthToken = 0;
    int m_exchangeToken = 0;
}
