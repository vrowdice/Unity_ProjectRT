using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResearchCountCondition", menuName = "Conditions/Research Count")]
public class ResearchCountCondition : ConditionBase
{
    public int maxResearchCount;

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

        return m_dataManager.AcceptedRequestList.Count <= maxResearchCount;
    }
}
