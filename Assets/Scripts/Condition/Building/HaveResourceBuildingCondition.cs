using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HaveResourceBuildingCondition", menuName = "Conditions/Building/Have Building")]
public class HaveResourceBuildingCondition : ConditionBase
{
    public string m_buildingCode;

    private GameDataManager m_dataManager;

    public void Initialize(GameDataManager data)
    {
        m_dataManager = data;
    }

    public override bool IsSatisfied()
    {
        if (m_dataManager == null)
        {
            Debug.LogWarning("GameDataManager not initialized");
            return false;
        }

        return m_dataManager.BuildingEntryDict.ContainsKey(m_buildingCode);
    }
}
