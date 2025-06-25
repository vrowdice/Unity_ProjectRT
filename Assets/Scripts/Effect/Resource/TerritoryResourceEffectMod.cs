using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerritoryResourceEffectMod", menuName = "Event Effects/Resource/Territory Mod")]
public class TerritoryResourceEffectMod : EffectBase
{
    public ResourceType.TYPE m_type;
    public float m_mod;

    public override void Activate(GameDataManager argDataManager)
    {
        argDataManager.EventEntry.m_state.m_territoryResourceModDic[m_type] += m_mod;
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        argDataManager.EventEntry.m_state.m_territoryResourceModDic[m_type] -= m_mod;
    }
}
