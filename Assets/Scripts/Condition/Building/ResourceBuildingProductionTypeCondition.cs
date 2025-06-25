using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceBuildingCondition", menuName = "Conditions/Building/Production Type")]
public class ResourceBuildingProductionTypeCondition : ConditionBase
{
    public List<ResourceType.TYPE> m_resourceTypeConditionList = new();

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

        foreach (ResourceType.TYPE requiredType in m_resourceTypeConditionList)
        {
            bool _typeFound = false;

            foreach (KeyValuePair<string, BuildingEntry> buildingPair in m_dataManager.BuildingEntryDict)
            {
                var _building = buildingPair.Value;

                if (_building.m_state.m_amount <= 0)
                    continue;

                foreach (ResourceAmount production in _building.m_data.m_productionList)
                {
                    if (production.m_type == requiredType)
                    {
                        _typeFound = true;
                        break;
                    }
                }

                if (_typeFound)
                    break;
            }

            if (!_typeFound)
                return false;
        }

        return true;
    }
}
