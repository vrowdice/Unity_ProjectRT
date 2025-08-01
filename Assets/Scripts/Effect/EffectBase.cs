using UnityEngine;

public abstract class EffectBase : ScriptableObject
{
    public string m_name;
    public string m_description;
    public abstract void Activate(GameDataManager dataManager);
    public abstract void Deactivate(GameDataManager dataManager);
}

