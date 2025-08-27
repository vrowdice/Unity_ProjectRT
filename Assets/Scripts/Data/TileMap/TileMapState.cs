using UnityEngine;

public class TileMapState
{
    public TerrainType.TYPE m_terrainType;
    public Vector2Int m_index;
    public bool m_isFriendlyArea = false;
    public bool m_isRoad = false;
    public GameResources m_resources;

    public TileMapState()
    {
        m_terrainType = TerrainType.TYPE.None;
        m_index = Vector2Int.zero;
        m_isFriendlyArea = false;
        m_isRoad = false;
        m_resources = new GameResources();
    }

    public TileMapState(TerrainType.TYPE terrainType, Vector2Int index, bool isFriendlyArea = false)
    {
        m_terrainType = terrainType;
        m_index = index;
        m_isFriendlyArea = isFriendlyArea;
        m_isRoad = false;
        m_resources = new GameResources();
    }
}
