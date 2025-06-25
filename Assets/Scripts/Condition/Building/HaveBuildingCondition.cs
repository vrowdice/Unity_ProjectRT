using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "HaveBuildingCondition", menuName = "Conditions/Building/Have Building Type")]
public class HaveBuildingCondition : ConditionBase
{
    public BuildingType.TYPE m_buildingType;

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

        return m_dataManager.BuildingEntryDict.Values.Any(buildingEntry =>
            buildingEntry.m_state.m_amount > 0 &&
            buildingEntry.m_data.m_buildingType == m_buildingType);
    }
}
