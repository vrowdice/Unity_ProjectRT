using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceEffect", menuName = "Event Effects/Resource/Building Mod")]
public class BuildingResourceEffectMod : EffectBase
{
    public ResourceType.TYPE m_type;
    public float m_mod;

    public override void Activate(GameDataManager argDataManager)
    {
        argDataManager.EventEntry.m_state.m_buildingResourceModDic[m_type] += m_mod;
    }

    public override void Deactivate(GameDataManager argDataManager)
    {
        argDataManager.EventEntry.m_state.m_buildingResourceModDic[m_type] -= m_mod;
    }
}
