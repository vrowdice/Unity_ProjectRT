using UnityEngine;

[CreateAssetMenu(fileName = "NewTileMapData", menuName = "TileMapData", order = 1)]
public class TileMapData : ScriptableObject
{
    public TerrainType.TYPE m_terrainType;
    public bool m_isMoveable;
    public float m_weight;
    public float m_closeTileMul;
    public float m_woodMul;
    public float m_iromMul;
    public float m_foodMul;
    public float m_techMul;
    public Color m_color;
}
