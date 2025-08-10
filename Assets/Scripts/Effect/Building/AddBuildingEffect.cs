using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddBuildingEffect", menuName = "Event Effects/Building/Add")]
public class AddBuildingEffect : EffectBase
{
    [Header("Building Settings")]
    public int m_addCount = 1;
    
    [Header("Specific Building (Optional)")]
    [Tooltip("Enter building code to specify a particular building. Leave empty to add random building.")]
    public string m_specificBuildingCode = "";

    private GameDataManager m_gameDataManager = null;

    public override void Activate(GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;

        // Check if specific building is specified
        if (!string.IsNullOrEmpty(m_specificBuildingCode))
        {
            // Add specific building
            argDataManager.AddSpecificBuilding(m_specificBuildingCode, m_addCount);
        }
        else
        {
            // Add random building
            argDataManager.RandomBuilding(m_addCount);
        }
    }

    public override void Deactivate(GameDataManager argDataManager)
    {

    }
}
