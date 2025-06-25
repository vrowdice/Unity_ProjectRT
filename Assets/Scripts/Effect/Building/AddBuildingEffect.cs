using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddBuildingEffect", menuName = "Event Effects/Building/Add")]
public class AddBuildingEffect : EffectBase
{
    public int m_addCount;

    private GameDataManager m_gameDataManager = null;

    public override void Activate(GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;

        argDataManager.RandomBuilding(m_addCount);
    }

    public override void Deactivate(GameDataManager argDataManager)
    {

    }
}
