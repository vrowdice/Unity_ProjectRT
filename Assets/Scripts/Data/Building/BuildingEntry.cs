public class BuildingEntry
{
    public BuildingData m_data;
    public BuildingState m_state;

    public BuildingEntry(BuildingData argData)
    {
        this.m_data = argData;
        this.m_state = new BuildingState();
    }
}
