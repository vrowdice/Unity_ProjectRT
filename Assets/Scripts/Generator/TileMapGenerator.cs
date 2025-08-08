using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 모든 타일 데이터를 이 하나의 클래스에서 관리합니다.
// 지형의 기본 특성(무게, 색상)과 타일의 고유 상태(전투력, 아군 여부, 실제 리소스 생산량)를 모두 포함합니다.
[Serializable]
public class MapTileData
{
    public TerrainType.TYPE terrainType;
    public int weight;
    public Color color;
    public bool isFriendlyArea = false;
    public int combatPower = 0;
    public ResourceAmount resourceAmount;
}

// 이 클래스는 맵 데이터를 생성하고 반환하는 역할만 담당합니다.
public class MapDataGenerator
{
    private Vector2Int mapSize;
    private string seed;
    private System.Random pseudoRandom;

    private List<MapTileData> terrainTemplates;
    private Dictionary<TerrainType.TYPE, MapTileData> terrainTemplatesDictionary;

    private TerrainType.TYPE friendlySettlementTerrain;
    private int friendlySettlementCount;
    private TerrainType.TYPE enemySettleTerrain;
    private int enemySettleCount;

    public MapDataGenerator(Vector2Int mapSize, List<MapTileData> templates, string seed,
        TerrainType.TYPE friendlySettle, int friendlySettleCount,
        TerrainType.TYPE enemySettle, int enemySettleCount)
    {
        this.mapSize = mapSize;
        this.terrainTemplates = templates;
        this.seed = seed;
        this.friendlySettlementTerrain = friendlySettle;
        this.friendlySettlementCount = friendlySettleCount;
        this.enemySettleTerrain = enemySettle;
        this.enemySettleCount = enemySettleCount;

        InitializeRandomSeed();
        terrainTemplatesDictionary = terrainTemplates.ToDictionary(t => t.terrainType, t => t);
    }

    private void InitializeRandomSeed()
    {
        if (string.IsNullOrEmpty(seed))
        {
            seed = DateTime.Now.ToBinary().ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
    }

    /// <summary>
    /// 새로운 맵 데이터를 생성하여 반환합니다.
    /// </summary>
    public MapTileData[,] GenerateMapData()
    {
        MapTileData[,] mapData = new MapTileData[mapSize.x, mapSize.y];

        InitializeMap(mapData);
        GenerateRoads(mapData);
        PlaceSpecialSettlements(mapData);
        AssignTerrainsToRemainingTiles(mapData);
        ApplyAdjacencyBonuses(mapData);
        PlaceEnemyDataAndResources(mapData);

        return mapData;
    }

    private void InitializeMap(MapTileData[,] mapData)
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                mapData[x, y] = new MapTileData();
            }
        }
    }

    private void GenerateRoads(MapTileData[,] mapData)
    {
        if (mapSize.x < 5 || mapSize.y < 5) return;

        Vector2Int startPoint = new Vector2Int(0, 0);
        Vector2Int endPoint = new Vector2Int(mapSize.x - 1, mapSize.y - 1);

        Vector2Int pivot1 = new Vector2Int(
            pseudoRandom.Next(mapSize.x / 4, mapSize.x * 3 / 4),
            pseudoRandom.Next(mapSize.y / 2, mapSize.y - 1)
        );
        GenerateSingleWindingRoad(mapData, startPoint, pivot1);
        GenerateSingleWindingRoad(mapData, pivot1, endPoint);

        Vector2Int pivot2 = new Vector2Int(
            pseudoRandom.Next(mapSize.x / 4, mapSize.x * 3 / 4),
            pseudoRandom.Next(0, mapSize.y / 2)
        );
        GenerateSingleWindingRoad(mapData, startPoint, pivot2);
        GenerateSingleWindingRoad(mapData, pivot2, endPoint);
    }

    private void GenerateSingleWindingRoad(MapTileData[,] mapData, Vector2Int start, Vector2Int end)
    {
        int currentX = start.x;
        int currentY = start.y;

        while (currentX != end.x || currentY != end.y)
        {
            MarkTileAsRoad(mapData, currentX, currentY);

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

            if (IsRoad(mapData, nextX, nextY))
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
        MarkTileAsRoad(mapData, end.x, end.y);
    }

    private bool IsRoad(MapTileData[,] mapData, int x, int y)
    {
        if (x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y)
        {
            return mapData[x, y].terrainType == TerrainType.TYPE.Road;
        }
        return false;
    }

    private void MarkTileAsRoad(MapTileData[,] mapData, int x, int y)
    {
        if (x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y)
        {
            if (mapData[x, y].terrainType == TerrainType.TYPE.None)
            {
                ApplyTerrainTemplate(mapData, x, y, TerrainType.TYPE.Road);
            }
        }
    }

    private void PlaceSpecialSettlements(MapTileData[,] mapData)
    {
        // 아군 정착지 배치
        ApplyTerrainTemplate(mapData, 0, 0, friendlySettlementTerrain);
        mapData[0, 0].isFriendlyArea = true;
        ExpandSettlement(mapData, friendlySettlementTerrain, friendlySettlementCount, (x, y) => x < mapSize.x / 2 && y < mapSize.y / 2, true);

        // 적군 정착지 배치
        ApplyTerrainTemplate(mapData, mapSize.x - 1, mapSize.y - 1, enemySettleTerrain);
        mapData[mapSize.x - 1, mapSize.y - 1].isFriendlyArea = false;
        ExpandSettlement(mapData, enemySettleTerrain, enemySettleCount, (x, y) => x >= mapSize.x / 2 && y >= mapSize.y / 2, false);
    }

    private void ExpandSettlement(MapTileData[,] mapData, TerrainType.TYPE typeToExpand, int targetCount, Func<int, int, bool> areaConstraint, bool isFriendly)
    {
        HashSet<Vector2Int> currentTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> frontier = new HashSet<Vector2Int>();
        bool isFallbackMode = false;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (mapData[x, y].terrainType == typeToExpand)
                {
                    currentTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        if (currentTiles.Count == 0) return;

        foreach (var tile in currentTiles)
        {
            AddNeighborsToFrontier(mapData, tile, areaConstraint, frontier, currentTiles);
        }

        while (currentTiles.Count < targetCount)
        {
            if (frontier.Count == 0)
            {
                if (!isFallbackMode)
                {
                    isFallbackMode = true;
                    foreach (var tile in currentTiles)
                    {
                        AddNeighborsToFrontier(mapData, tile, (x, y) => true, frontier, currentTiles);
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

            ApplyTerrainTemplate(mapData, chosen.x, chosen.y, typeToExpand);
            mapData[chosen.x, chosen.y].isFriendlyArea = isFriendly;
            currentTiles.Add(chosen);

            AddNeighborsToFrontier(mapData, chosen, isFallbackMode ? (x, y) => true : areaConstraint, frontier, currentTiles);
        }
    }

    private void AddNeighborsToFrontier(MapTileData[,] mapData, Vector2Int tile, Func<int, int, bool> areaConstraint, HashSet<Vector2Int> frontier, HashSet<Vector2Int> currentTiles)
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
                !frontier.Contains(neighbor) &&
                mapData[neighbor.x, neighbor.y].terrainType == TerrainType.TYPE.None)
            {
                frontier.Add(neighbor);
            }
        }
    }

    private void AssignTerrainsToRemainingTiles(MapTileData[,] mapData)
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (mapData[x, y].terrainType == TerrainType.TYPE.None)
                {
                    TerrainType.TYPE chosenTerrainType = GetTerrainTypeWithNeighborInfluence(mapData, x, y);
                    ApplyTerrainTemplate(mapData, x, y, chosenTerrainType);
                }
            }
        }
    }

    private TerrainType.TYPE GetTerrainTypeWithNeighborInfluence(MapTileData[,] mapData, int x, int y)
    {
        var weightedList = terrainTemplates
            .Where(t => t.terrainType != friendlySettlementTerrain && t.terrainType != enemySettleTerrain && t.terrainType != TerrainType.TYPE.Road && t.terrainType != TerrainType.TYPE.None)
            .Select(t => new { t.terrainType, t.weight })
            .ToList();

        Vector2Int[] neighbors = { new Vector2Int(x + 1, y), new Vector2Int(x - 1, y), new Vector2Int(x, y + 1), new Vector2Int(x, y - 1) };
        foreach (var neighbor in neighbors)
        {
            if (neighbor.x >= 0 && neighbor.x < mapSize.x && neighbor.y >= 0 && neighbor.y < mapSize.y)
            {
                TerrainType.TYPE neighborTerrain = mapData[neighbor.x, neighbor.y].terrainType;
                var terrainToBoost = weightedList.FirstOrDefault(t => t.terrainType == neighborTerrain);
                if (terrainToBoost != null)
                {
                    int index = weightedList.FindIndex(t => t.terrainType == neighborTerrain);
                    weightedList[index] = new { terrainToBoost.terrainType, weight = (int)(terrainToBoost.weight * 1.5f) };
                }
            }
        }

        int totalWeight = weightedList.Sum(t => t.weight);

        if (totalWeight <= 0) return TerrainType.TYPE.None;

        int randomValue = pseudoRandom.Next(0, totalWeight);

        foreach (var terrain in weightedList)
        {
            if (randomValue < terrain.weight)
            {
                return terrain.terrainType;
            }
            randomValue -= terrain.weight;
        }

        return TerrainType.TYPE.None;
    }

    private void ApplyAdjacencyBonuses(MapTileData[,] mapData)
    {
        // 리소스 관련 필드가 제거되었으므로 이 함수는 더 이상 필요하지 않습니다.
    }

    private void PlaceEnemyDataAndResources(MapTileData[,] mapData)
    {
        // 리소스 관련 필드가 제거되었으므로 이 함수는 더 이상 필요하지 않습니다.
    }

    // 새로운 헬퍼 메서드: 템플릿 데이터를 타일 데이터에 복사
    private void ApplyTerrainTemplate(MapTileData[,] mapData, int x, int y, TerrainType.TYPE type)
    {
        if (terrainTemplatesDictionary.TryGetValue(type, out var template))
        {
            mapData[x, y].terrainType = template.terrainType;
            mapData[x, y].weight = template.weight;
            mapData[x, y].color = template.color;
        }
        else
        {
            Debug.LogError($"지형 템플릿을 찾을 수 없습니다: {type}");
        }
    }
}