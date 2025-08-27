using System.Collections.Generic;

public class BuildingState
{
    public int m_amount = 0;
    public bool m_isUnlocked = true;

    public List<ResourceAmount> m_calculatedProductionList = new List<ResourceAmount>();
}
