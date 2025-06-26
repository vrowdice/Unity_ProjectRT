using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerritoryAllResourceEffectMod", menuName = "Event Effects/Resource/All Territory Mod")]
public class TerritoryAllResourceEffectMod : EffectBase
{
    public float m_mod;

    public override void Activate(GameDataManager argDataManager)
    {
        foreach (ResourceType.TYPE type in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            argDataManager.EventEntry.m_state.m_buildingResourceModDic[type] += m_mod;
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        foreach (ResourceType.TYPE type in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            argDataManager.EventEntry.m_state.m_buildingResourceModDic[type] -= m_mod;
        }
    }
}
