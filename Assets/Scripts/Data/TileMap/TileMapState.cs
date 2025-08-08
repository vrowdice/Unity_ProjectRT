using UnityEngine;

public class TileMapState
{
    public TerrainType.TYPE m_terrainType;
    public Vector2Int m_index;
    public bool m_isFriendlyArea = false;
    public ResourceAmount m_resourceAmount;

    public TileMapState()
    {
        m_terrainType = TerrainType.TYPE.None;
        m_index = Vector2Int.zero;
        m_isFriendlyArea = false;
        m_resourceAmount = new ResourceAmount();
    }

    public TileMapState(TerrainType.TYPE terrainType, Vector2Int index, bool isFriendlyArea = false)
    {
        m_terrainType = terrainType;
        m_index = index;
        m_isFriendlyArea = isFriendlyArea;
        m_resourceAmount = new ResourceAmount();
    }
}
