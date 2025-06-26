using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingAllResourceEffectMod", menuName = "Event Effects/Resource/All Building Mod")]
public class BuildingAllResourceEffectMod : EffectBase
{
    public float m_mod;

    public override void Activate(GameDataManager argDataManager)
    {
        foreach (ResourceType.TYPE type in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            argDataManager.EventEntry.m_state.m_territoryResourceModDic[type] += m_mod;
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        foreach (ResourceType.TYPE type in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            argDataManager.EventEntry.m_state.m_territoryResourceModDic[type] -= m_mod;
        }
    }
}
