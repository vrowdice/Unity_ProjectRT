using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceEffect", menuName = "Event Effects/Resource")]
public class ResourceEffectMod : EffectBase
{
    public ResourceType.TYPE m_type;
    public int m_mod;

    public override void Apply(GameDataManager argDataManager)
    {
        argDataManager.EventEntry.m_state.m_resourceMod[m_type] = m_mod;
    }
}
