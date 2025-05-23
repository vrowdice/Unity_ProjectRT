using System.Collections.Generic;

public class BuildingEntry
{
    public BuildingData m_data;
    public BuildingState m_state;

    public BuildingEntry(BuildingData argData)
    {
        this.m_data = argData;
        this.m_state = new BuildingState();
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
    /// m_state의 값을 변경한 후 반드시 이 함수를 호출
    /// 혹은 계산 후 값 변경 필요
    /// </summary>
    public List<ResourceAmount> ApplyProduction()
    {
        m_state.m_calculatedProductionList = CalculateProduction(m_state.m_amount);
        return m_state.m_calculatedProductionList;
    }
}
