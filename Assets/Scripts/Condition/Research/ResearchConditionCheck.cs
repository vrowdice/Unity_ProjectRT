using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchConditionCheck
{
    public static ICondition ResearchCountAtMost(GameDataManager data, int count)
    {
        return new CustomCondition(() => data.AcceptedRequestList.Count <= count);
    }
}
