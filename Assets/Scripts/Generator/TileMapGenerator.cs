using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileMapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject defaultTilePrefab;
    [SerializeField] private Vector2Int mapSize = new Vector2Int(10, 10);

    public string seed;
    public bool useRandomSeed;

    // Use a single class for all tile data
    public MapTileData[,] MapData;

    private System.Random pseudoRandom;

    [Header("Faction Settings")]
    [SerializeField] private Color roadColor = Color.gray;

    [Header("Friendly Faction")]
    [SerializeField] private TerrainType friendlySettlementTerrain = TerrainType.Settlement;
    [SerializeField] private Color friendlyColor = Color.green;
    [SerializeField] private int friendlySettlementCount = 7;
    [SerializeField] private Vector2Int friendlySettleMaxSize = new Vector2Int(3, 3);

    [Header("Enemy Faction")]
    [SerializeField] private TerrainType enemySettleTerrain = TerrainType.Settlement;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private int enemySettleCount = 15;
    [SerializeField] private Vector2Int enemySettleMaxSize = new Vector2Int(5, 4);

    [Serializable]
    public class ResourceYields
    {
        public float wood;
        public float iron;
        public float food;
        public float research;
    }

    // This class defines the properties for a terrain type. It's now the primary source of truth.
    [Serializable]
    public class TerrainProperties
    {
        public TerrainType terrainType;
        public int weight;
        public Color color;
        public ResourceYields resourceYields;
    }

    [Header("Terrain Settings")]
    [SerializeField]
    private List<TerrainProperties> terrainPropertiesList = new List<TerrainProperties>
    {
        new TerrainProperties { terrainType = TerrainType.Settlement, weight = 0, color = Color.blue, resourceYields = new ResourceYields { wood = 1.0f, iron = 0.75f, food = 1.0f, research = 0.3f } },
        new TerrainProperties { terrainType = TerrainType.Forest, weight = 15, color = new Color(0.1f, 0.5f, 0.1f), resourceYields = new ResourceYields { wood = 1.0f, iron = 0.2f, food = 0.5f, research = 0.1f } },
        new TerrainProperties { terrainType = TerrainType.Grove, weight = 10, color = new Color(0.2f, 0.6f, 0.2f), resourceYields = new ResourceYields { wood = 0.75f, iron = 0.2f, food = 0.75f, research = 0.1f } },
        new TerrainProperties { terrainType = TerrainType.AncientRuins, weight = 7, color = new Color(0.7f, 0.7f, 0.8f), resourceYields = new ResourceYields { wood = 0.0f, iron = 0.75f, food = 0.0f, research = 1.0f } },
        new TerrainProperties { terrainType = TerrainType.Plain, weight = 10, color = Color.green, resourceYields = new ResourceYields { wood = 0.5f, iron = 0.2f, food = 0.75f, research = 0.25f } },
        new TerrainProperties { terrainType = TerrainType.MushroomZone, weight = 7, color = new Color(0.6f, 0.4f, 0.8f), resourceYields = new ResourceYields { wood = 0.5f, iron = 0.2f, food = 1.0f, research = 0.3f } },
        new TerrainProperties { terrainType = TerrainType.GoldLake, weight = 7, color = new Color(0.7f, 0.6f, 0.2f), resourceYields = new ResourceYields { wood = 0.2f, iron = 0.75f, food = 0.75f, research = 0.2f } },
        new TerrainProperties { terrainType = TerrainType.Snowfield, weight = 5, color = new Color(0.9f, 0.9f, 0.9f), resourceYields = new ResourceYields { wood = 0.5f, iron = 0.3f, food = 0.3f, research = 0.4f } },
        new TerrainProperties { terrainType = TerrainType.RockPlain, weight = 7, color = new Color(0.4f, 0.4f, 0.4f), resourceYields = new ResourceYields { wood = 0.0f, iron = 0.8f, food = 0.2f, research = 0.5f } },
        new TerrainProperties { terrainType = TerrainType.Mountain, weight = 10, color = new Color(0.5f, 0.5f, 0.5f), resourceYields = new ResourceYields() },
        new TerrainProperties { terrainType = TerrainType.River, weight = 10, color = Color.blue, resourceYields = new ResourceYields() },
        new TerrainProperties { terrainType = TerrainType.Volcano, weight = 10, color = new Color(0.4f, 0.2f, 0.0f), resourceYields = new ResourceYields() }
    };

    // A dictionary for quick lookup of TerrainProperties by type
    private Dictionary<TerrainType, TerrainProperties> terrainPropertiesDictionary;

    // Use Awake to initialize the dictionary once
    private void Awake()
    {
        terrainPropertiesDictionary = terrainPropertiesList.ToDictionary(t => t.terrainType, t => t);
    }

    void Start()
    {
        InitializeRandomSeed();
        GenerateMiniMap();
        DrawMapTiles();
    }

    [ContextMenu("Generate New Map")]
    public void GenerateNewMap()
    {
        ClearExistingMap();
        InitializeRandomSeed();
        GenerateMiniMap();
        DrawMapTiles();
    }

    private void ClearExistingMap()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void InitializeRandomSeed()
    {
        if (useRandomSeed || string.IsNullOrEmpty(seed))
        {
            seed = DateTime.Now.ToBinary().ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
    }

    private void GenerateMiniMap()
    {
        MapData = new MapTileData[mapSize.x, mapSize.y];
        InitializeMap();
        GenerateRoads();
        PlaceSpecialSettlements();
        AssignTerrainsToRemainingTiles();
        ApplyAdjacencyBonuses();
        PlaceEnemyDataAndResources();
    }

    private void InitializeMap()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                MapData[x, y] = new MapTileData();
                MapData[x, y].terrainProperties = null; // Set to null initially
            }
        }
    }

    private void GenerateRoads()
    {
        if (mapSize.x < 5 || mapSize.y < 5) return;

        Vector2Int startPoint = new Vector2Int(0, 0);
        Vector2Int endPoint = new Vector2Int(mapSize.x - 1, mapSize.y - 1);

        Vector2Int pivot1 = new Vector2Int(
            pseudoRandom.Next(mapSize.x / 4, mapSize.x * 3 / 4),
            pseudoRandom.Next(mapSize.y / 2, mapSize.y - 1)
        );
        GenerateSingleWindingRoad(startPoint, pivot1);
        GenerateSingleWindingRoad(pivot1, endPoint);

        Vector2Int pivot2 = new Vector2Int(
            pseudoRandom.Next(mapSize.x / 4, mapSize.x * 3 / 4),
            pseudoRandom.Next(0, mapSize.y / 2)
        );
        GenerateSingleWindingRoad(startPoint, pivot2);
        GenerateSingleWindingRoad(pivot2, endPoint);
    }

    private void GenerateSingleWindingRoad(Vector2Int start, Vector2Int end)
    {
        int currentX = start.x;
        int currentY = start.y;

        while (currentX != end.x || currentY != end.y)
        {
            MarkTileAsRoad(currentX, currentY);

            int dx = end.x - currentX;
            int dy = end.y - currentY;
            bool moveX = (Math.Abs(dx) > Math.Abs(dy)) ? true : false;

            if (pseudoRandom.Next(0, 2) == 0)
            {
                moveX = !moveX;
            }

            int nextX = currentX;
            int nextY = currentY;

            if (moveX && currentX != end.x)
            {
                nextX += (end.x > currentX) ? 1 : -1;
            }
            else if (currentY != end.y)
            {
                nextY += (end.y > currentY) ? 1 : -1;
            }
            else if (currentX != end.x)
            {
                nextX += (end.x > currentX) ? 1 : -1;
            }

            if (IsRoad(nextX, nextY))
            {
                if (moveX)
                {
                    if (currentY != end.y)
                    {
                        currentY += (end.y > currentY) ? 1 : -1;
                    }
                    else
                    {
                        currentX += (end.x > currentX) ? 1 : -1;
                    }
                }
                else
                {
                    if (currentX != end.x)
                    {
                        currentX += (end.x > currentX) ? 1 : -1;
                    }
                    else
                    {
                        currentY += (end.y > currentY) ? 1 : -1;
                    }
                }
            }
            else
            {
                currentX = nextX;
                currentY = nextY;
            }
        }
        MarkTileAsRoad(end.x, end.y);
    }

    private bool IsRoad(int x, int y)
    {
        if (x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y)
        {
            return MapData[x, y].terrainProperties != null && MapData[x, y].terrainProperties.terrainType == TerrainType.Road;
        }
        return false;
    }

    private void MarkTileAsRoad(int x, int y)
    {
        if (x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y)
        {
            if (MapData[x, y].terrainProperties == null)
            {
                MapData[x, y].terrainProperties = terrainPropertiesDictionary[TerrainType.Road];
            }
        }
    }

    private void PlaceSpecialSettlements()
    {
        // 시작 타일 지정
        MapData[0, 0].terrainProperties = terrainPropertiesDictionary[friendlySettlementTerrain];
        MapData[0, 0].isFriendlyArea = true;

        ExpandSettlement(friendlySettlementTerrain, friendlySettlementCount, (x, y) => x < 3 && y < 3, true);

        MapData[mapSize.x - 1, mapSize.y - 1].terrainProperties = terrainPropertiesDictionary[enemySettleTerrain];
        MapData[mapSize.x - 1, mapSize.y - 1].isFriendlyArea = false;

        ExpandSettlement(enemySettleTerrain, enemySettleCount, (x, y) => x >= mapSize.x - 5 && y >= mapSize.y - 4, false);
    }

    private void ExpandSettlement(TerrainType typeToExpand, int targetCount, Func<int, int, bool> areaConstraint, bool isFriendly)
    {
        HashSet<Vector2Int> currentTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> frontier = new HashSet<Vector2Int>();
        bool isFallbackMode = false;

        // 초기 확장된 타일 수집
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (MapData[x, y].terrainProperties != null &&
                    MapData[x, y].terrainProperties.terrainType == typeToExpand)
                {
                    currentTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        if (currentTiles.Count == 0)
            return;

        // frontier 초기화
        foreach (var tile in currentTiles)
        {
            AddNeighborsToFrontier(tile, areaConstraint, frontier, currentTiles);
        }

        while (currentTiles.Count < targetCount)
        {
            if (frontier.Count == 0)
            {
                if (!isFallbackMode)
                {
                    isFallbackMode = true;

                    // 모든 인접 타일을 다시 스캔하여 constraint 없이 추가
                    foreach (var tile in currentTiles)
                    {
                        AddNeighborsToFrontier(tile, (x, y) => true, frontier, currentTiles);
                    }

                    if (frontier.Count == 0)
                    {
                        Debug.LogWarning($"[{typeToExpand}] 인접 타일 부족 → 목표 {targetCount} 중 {currentTiles.Count}개 생성됨.");
                        break;
                    }
                }
                else
                {
                    Debug.LogWarning($"[{typeToExpand}] 확장 불가 → 목표 {targetCount} 중 {currentTiles.Count}개 생성됨.");
                    break;
                }
            }

            List<Vector2Int> frontierList = frontier.ToList();
            int index = pseudoRandom.Next(0, frontierList.Count);
            Vector2Int chosen = frontierList[index];
            frontier.Remove(chosen);

            MapData[chosen.x, chosen.y].terrainProperties = terrainPropertiesDictionary[typeToExpand];
            MapData[chosen.x, chosen.y].isFriendlyArea = isFriendly;
            currentTiles.Add(chosen);

            // fallback일 경우에는 constraint 없이 추가
            AddNeighborsToFrontier(chosen, isFallbackMode ? (x, y) => true : areaConstraint, frontier, currentTiles);
        }
    }

    private void AddNeighborsToFrontier(Vector2Int tile, Func<int, int, bool> areaConstraint, HashSet<Vector2Int> frontier, HashSet<Vector2Int> currentTiles)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = tile + dir;

            if (neighbor.x >= 0 && neighbor.x < mapSize.x &&
                neighbor.y >= 0 && neighbor.y < mapSize.y &&
                areaConstraint(neighbor.x, neighbor.y) &&
                !currentTiles.Contains(neighbor) &&
                !frontier.Contains(neighbor))
            {
                frontier.Add(neighbor);
            }
        }
    }

    private void AssignTerrainsToRemainingTiles()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (MapData[x, y].terrainProperties == null)
                {
                    TerrainProperties chosenTerrainData = GetTerrainTypeWithNeighborInfluence(x, y);
                    MapData[x, y].terrainProperties = chosenTerrainData;
                    MapData[x, y].resourceYields = new ResourceYields
                    {
                        wood = chosenTerrainData.resourceYields.wood,
                        iron = chosenTerrainData.resourceYields.iron,
                        food = chosenTerrainData.resourceYields.food,
                        research = chosenTerrainData.resourceYields.research
                    };
                }
            }
        }
    }

    private TerrainProperties GetTerrainTypeWithNeighborInfluence(int x, int y)
    {
        var weightedList = terrainPropertiesList
            .Where(t => t.terrainType != friendlySettlementTerrain && t.terrainType != enemySettleTerrain && t.terrainType != TerrainType.Road)
            .Select(t => new { t.terrainType, weight = t.weight })
            .ToList();

        Vector2Int[] neighbors = { new Vector2Int(x + 1, y), new Vector2Int(x - 1, y), new Vector2Int(x, y + 1), new Vector2Int(x, y - 1) };
        foreach (var neighbor in neighbors)
        {
            if (neighbor.x >= 0 && neighbor.x < mapSize.x && neighbor.y >= 0 && neighbor.y < mapSize.y)
            {
                TerrainProperties neighborTerrainProperties = MapData[neighbor.x, neighbor.y].terrainProperties;
                if (neighborTerrainProperties != null)
                {
                    TerrainType neighborTerrain = neighborTerrainProperties.terrainType;
                    var terrainToBoost = weightedList.FirstOrDefault(t => t.terrainType == neighborTerrain);
                    if (terrainToBoost != null)
                    {
                        int index = weightedList.FindIndex(t => t.terrainType == neighborTerrain);
                        weightedList[index] = new { terrainToBoost.terrainType, weight = (int)(terrainToBoost.weight * 1.5f) };
                    }
                }
            }
        }

        int totalWeight = weightedList.Sum(t => t.weight);

        if (totalWeight <= 0) return GetTerrainProperties(TerrainType.None);

        int randomValue = pseudoRandom.Next(0, totalWeight);

        foreach (var terrain in weightedList)
        {
            if (randomValue < terrain.weight)
            {
                return GetTerrainProperties(terrain.terrainType);
            }
            randomValue -= terrain.weight;
        }

        return GetTerrainProperties(TerrainType.None);
    }

    private void ApplyAdjacencyBonuses()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (MapData[x, y].terrainProperties.terrainType == TerrainType.Mountain ||
                    MapData[x, y].terrainProperties.terrainType == TerrainType.River ||
                    MapData[x, y].terrainProperties.terrainType == TerrainType.Volcano)
                {
                    Vector2Int[] neighbors = { new Vector2Int(x + 1, y), new Vector2Int(x - 1, y), new Vector2Int(x, y + 1), new Vector2Int(x, y - 1) };

                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.x >= 0 && neighbor.x < mapSize.x && neighbor.y >= 0 && neighbor.y < mapSize.y)
                        {
                            if (MapData[neighbor.x, neighbor.y].resourceYields == null)
                            {
                                MapData[neighbor.x, neighbor.y].resourceYields = new ResourceYields();
                            }
                            switch (MapData[x, y].terrainProperties.terrainType)
                            {
                                case TerrainType.Mountain:
                                    MapData[neighbor.x, neighbor.y].resourceYields.iron += 0.3f;
                                    break;
                                case TerrainType.River:
                                    MapData[neighbor.x, neighbor.y].resourceYields.food += 0.15f;
                                    break;
                                case TerrainType.Volcano:
                                    MapData[neighbor.x, neighbor.y].resourceYields.iron += 0.2f;
                                    MapData[neighbor.x, neighbor.y].resourceYields.food += 0.2f;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void PlaceEnemyDataAndResources()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (MapData[x, y].terrainProperties.terrainType == enemySettleTerrain)
                {
                    MapData[x, y].combatPower = (int)(MapData[x, y].resourceYields.wood * 10 + MapData[x, y].resourceYields.iron * 15 + MapData[x, y].resourceYields.food * 5 + MapData[x, y].resourceYields.research * 8);

                    if (MapData[x, y].combatPower > 30) MapData[x, y].combatPower = 30;
                }
            }
        }
    }

    private void DrawMapTiles()
    {
        if (defaultTilePrefab == null)
        {
            Debug.LogError("Default Tile Prefab is not assigned!");
            return;
        }

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                Vector3 pos = new Vector3(x, -y, 0);

                GameObject tileInstance = Instantiate(defaultTilePrefab, pos, Quaternion.identity, gameObject.transform);
                SpriteRenderer spriteRenderer = tileInstance.GetComponent<SpriteRenderer>();

                if (spriteRenderer == null)
                {
                    continue;
                }

                Color targetColor = GetTileColor(x, y);
                spriteRenderer.color = targetColor;
            }
        }
    }

    private Color GetTileColor(int x, int y)
    {
        if (MapData[x, y].terrainProperties.terrainType == enemySettleTerrain)
        {
            return enemyColor;
        }

        if (MapData[x, y].isFriendlyArea)
        {
            return friendlyColor;
        }

        return MapData[x, y].terrainProperties.color;
    }

    // Helper method to retrieve properties by type
    private TerrainProperties GetTerrainProperties(TerrainType terrainType)
    {
        if (terrainPropertiesDictionary.TryGetValue(terrainType, out var properties))
        {
            return properties;
        }
        return null;
    }
}

// MapTileData now holds a reference to its terrain's properties, plus a few unique runtime variables.
public class MapTileData
{
    public TileMapGenerator.TerrainProperties terrainProperties;
    public TileMapGenerator.ResourceYields resourceYields = new TileMapGenerator.ResourceYields();
    public int combatPower;
    public bool isFriendlyArea = false; // Moved here as it's specific to an instance of a tile
}

public enum TerrainType
{
    None,
    Settlement,
    Road,
    Forest,
    Grove,
    AncientRuins,
    Plain,
    MushroomZone,
    GoldLake,
    Snowfield,
    RockPlain,
    Mountain,
    River,
    Volcano
}