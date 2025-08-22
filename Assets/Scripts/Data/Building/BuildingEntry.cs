using System.Collections.Generic;

public class BuildingEntry
{
    public BuildingData m_data;
    public BuildingState m_state;

    public BuildingEntry(BuildingData argData)
    {
        this.m_data = argData;
        this.m_state = new BuildingState();
        this.m_state.m_isUnlocked = argData.m_isUnlocked;
    }

    public List<ResourceAmount> CalculateProduction(long argBuildingAmount)
    {
        List<ResourceAmount> _list = new List<ResourceAmount>();

        foreach(ResourceAmount item in m_data.m_productionList)
        {
            _list.Add(new ResourceAmount(item.m_type, item.m_amount * argBuildingAmount));
        }

        return _list;
    }

    /// <summary>
    /// m_state�� ���� ������ �� �ݵ�� �� �Լ��� ȣ��
    /// Ȥ�� ��� �� �� ���� �ʿ�
    /// </summary>
    public List<ResourceAmount> ApplyProduction()
    {
        m_state.m_calculatedProductionList = CalculateProduction(m_state.m_amount);
        return m_state.m_calculatedProductionList;
    }
}
