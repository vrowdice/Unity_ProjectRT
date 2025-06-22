using UnityEngine;

public abstract class EffectBase : ScriptableObject
{
    public abstract void Apply(GameDataManager argDataManager);
}
