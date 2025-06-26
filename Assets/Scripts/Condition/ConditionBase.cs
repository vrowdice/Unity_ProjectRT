using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionBase : ScriptableObject, ICondition
{
    public abstract void Initialize(GameDataManager data);
    public abstract bool IsSatisfied();
}
