using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapDataGenerator
{
    private Vector2Int mapSize;
    private string seed;
    private System.Random pseudoRandom;

    private List<TileMapData> terrainTemplates;
    private Dictionary<TerrainType.TYPE, TileMapData> terrainTemplatesDictionary;

    private TerrainType.TYPE friendlySettlementTerrain;
    private int friendlySettlementCount;
    private TerrainType.TYPE enemySettleTerrain;
    private int enemySettleCount;

    // Balance params
    private int friendlySettleResourceBase;
    private int normalTileResourceBase;
    private int enemyTileResourceBase;
    private float settleMul;
    private float mainMul;

    public MapDataGenerator(Vector2Int mapSize, List<TileMapData> templates, string seed,
        TerrainType.TYPE friendlySettle, int friendlySettleCount,
        TerrainType.TYPE enemySettle, int enemySettleCount,
        int friendlyBase, int normalBase, int enemyBase,
        float settleMul, float mainMul)
    {
        this.mapSize = mapSize;
        this.terrainTemplates = templates;
        this.seed = seed;
        this.friendlySettlementTerrain = friendlySettle;
        this.friendlySettlementCount = friendlySettleCount;
        this.enemySettleTerrain = enemySettle;
        this.enemySettleCount = enemySettleCount;
        this.friendlySettleResourceBase = friendlyBase;
        this.normalTileResourceBase = normalBase;
        this.enemyTileResourceBase = enemyBase;
        this.settleMul = settleMul;
        this.mainMul = Mathf.Max(0.0001f, mainMul);

        InitializeRandomSeed();
        terrainTemplatesDictionary = terrainTemplates.ToDictionary(t => t.m_terrainType, t => t);
    }

    // seed를 인자로 받지 않는 생성자를 추가하여,
    // 외부에서 시드를 지정하지 않으면 내부적으로 무작위 시드를 사용하도록 합니다.
    public MapDataGenerator(Vector2Int mapSize, List<TileMapData> templates,
        TerrainType.TYPE friendlySettle, int friendlySettleCount,
        TerrainType.TYPE enemySettle, int enemySettleCount,
        int friendlyBase, int normalBase, int enemyBase,
        float settleMul, float mainMul)
        : this(mapSize, templates, null, friendlySettle, friendlySettleCount, enemySettle, enemySettleCount,
               friendlyBase, normalBase, enemyBase, settleMul, mainMul)
    {
    }

    private void InitializeRandomSeed()
    {
        //
        if (string.IsNullOrEmpty(seed))
        {
            seed = DateTime.Now.ToBinary().ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
        Debug.Log($"Map generation seed: {seed}");
    }

    public TileMapState[,] GenerateMapData()
    {
        TileMapState[,] mapData = new TileMapState[mapSize.x, mapSize.y];

        InitializeMap(mapData);
        GenerateRoads(mapData);
        PlaceSpecialSettlements(mapData);
        AssignTerrainsToAllTiles(mapData);
        AssignResourcesToAllTiles(mapData);

        return mapData;
    }

    private void InitializeMap(TileMapState[,] mapData)
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                mapData[x, y] = new TileMapState();
                mapData[x, y].m_index = new Vector2Int(x, y);
                mapData[x, y].m_terrainType = TerrainType.TYPE.None;
                mapData[x, y].m_isFriendlyArea = false;
            }
        }
    }

    private void GenerateRoads(TileMapState[,] mapData)
    {
        if (mapSize.x < 5 || mapSize.y < 5) return;

        Vector2Int startPoint = new Vector2Int(0, 0);
        Vector2Int endPoint = new Vector2Int(mapSize.x - 1, mapSize.y - 1);

        // 첫 번째 도로 경로 생성
        Vector2Int pivot1 = new Vector2Int(
            pseudoRandom.Next(mapSize.x / 4, mapSize.x * 3 / 4),
            pseudoRandom.Next(mapSize.y / 2, mapSize.y - 1)
        );
        GenerateSingleWindingRoad(mapData, startPoint, pivot1);
        GenerateSingleWindingRoad(mapData, pivot1, endPoint);

        // 두 번째 도로 경로 생성 (무조건 생성)
        Vector2Int pivot2 = new Vector2Int(
            pseudoRandom.Next(mapSize.x / 4, mapSize.x * 3 / 4),
            pseudoRandom.Next(0, mapSize.y / 2)
        );
        
        // 두 번째 도로는 기존 도로와 겹치지 않도록 생성
        GenerateNonOverlappingRoad(mapData, startPoint, pivot2, endPoint);
    }

    private void GenerateSingleWindingRoad(TileMapState[,] mapData, Vector2Int start, Vector2Int end)
    {
        int currentX = start.x;
        int currentY = start.y;

        while (currentX != end.x || currentY != end.y)
        {
            MarkTileAsRoad(mapData, currentX, currentY);

            int dx = end.x - currentX;
            int dy = end.y - currentY;
            bool moveX = (Math.Abs(dx) > Math.Abs(dy));

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

    private bool IsRoad(TileMapState[,] mapData, int x, int y)
    {
        if (x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y)
        {
            return mapData[x, y].m_isRoad;
        }
        return false;
    }

    private void GenerateNonOverlappingRoad(TileMapState[,] mapData, Vector2Int start, Vector2Int pivot, Vector2Int end)
    {
        // 시작점에서 피벗까지의 경로 생성 (기존 도로와 겹치지 않도록)
        GenerateNonOverlappingPath(mapData, start, pivot);
        
        // 피벗에서 끝점까지의 경로 생성 (기존 도로와 겹치지 않도록)
        GenerateNonOverlappingPath(mapData, pivot, end);
    }

    private void GenerateNonOverlappingPath(TileMapState[,] mapData, Vector2Int start, Vector2Int end)
    {
        int currentX = start.x;
        int currentY = start.y;
        int maxAttempts = 100; // 무한 루프 방지
        int attempts = 0;

        while ((currentX != end.x || currentY != end.y) && attempts < maxAttempts)
        {
            attempts++;
            
            // 현재 위치가 도로가 아니면 도로로 표시
            if (!IsRoad(mapData, currentX, currentY))
            {
                MarkTileAsRoad(mapData, currentX, currentY);
            }

            int dx = end.x - currentX;
            int dy = end.y - currentY;
            
            // 다음 이동 방향 결정
            Vector2Int nextMove = GetNextMoveAvoidingRoads(mapData, currentX, currentY, dx, dy);
            
            currentX = nextMove.x;
            currentY = nextMove.y;
        }
        
        // 끝점도 도로로 표시
        if (!IsRoad(mapData, end.x, end.y))
        {
            MarkTileAsRoad(mapData, end.x, end.y);
        }
    }

    private Vector2Int GetNextMoveAvoidingRoads(TileMapState[,] mapData, int currentX, int currentY, int dx, int dy)
    {
        // 우선순위: 1. 목표 방향으로 직진, 2. 대각선 이동, 3. 우회
        Vector2Int[] possibleMoves = new Vector2Int[]
        {
            new Vector2Int(currentX + Math.Sign(dx), currentY), // X 방향
            new Vector2Int(currentX, currentY + Math.Sign(dy)), // Y 방향
            new Vector2Int(currentX + Math.Sign(dx), currentY + Math.Sign(dy)), // 대각선
            new Vector2Int(currentX - Math.Sign(dy), currentY + Math.Sign(dx)), // 우회 1
            new Vector2Int(currentX + Math.Sign(dy), currentY - Math.Sign(dx))  // 우회 2
        };

        foreach (var move in possibleMoves)
        {
            if (IsValidMove(mapData, move.x, move.y) && 
                !IsRoad(mapData, move.x, move.y) && 
                !IsImpassableTerrain(mapData[move.x, move.y].m_terrainType))
            {
                return move;
            }
        }

        // 모든 방향이 막혀있으면 기존 도로를 사용하되, 통과 가능한 지형 중에서 목표에 가까워지는 방향 선택
        foreach (var move in possibleMoves)
        {
            if (IsValidMove(mapData, move.x, move.y) && 
                !IsImpassableTerrain(mapData[move.x, move.y].m_terrainType))
            {
                return move;
            }
        }

        // 마지막 수단: 통과 불가능한 지형이라도 유효한 범위 내에서 이동
        foreach (var move in possibleMoves)
        {
            if (IsValidMove(mapData, move.x, move.y))
            {
                return move;
            }
        }

        // 마지막 수단: 현재 위치 유지
        return new Vector2Int(currentX, currentY);
    }

    private bool IsValidMove(TileMapState[,] mapData, int x, int y)
    {
        return x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y;
    }

    private bool IsImpassableTerrain(TerrainType.TYPE terrainType)
    {
        return terrainType == TerrainType.TYPE.Mountain || 
               terrainType == TerrainType.TYPE.River || 
               terrainType == TerrainType.TYPE.Volcano;
    }



    private void MarkTileAsRoad(TileMapState[,] mapData, int x, int y)
    {
        if (x >= 0 && x < mapSize.x && y >= 0 && y < mapSize.y)
        {
            // 통과할 수 없는 지형에는 길을 설정할 수 없음
            if (!IsImpassableTerrain(mapData[x, y].m_terrainType))
            {
                mapData[x, y].m_isRoad = true;
            }
        }
    }

    private void PlaceSpecialSettlements(TileMapState[,] mapData)
    {
        ApplyTerrainTemplate(mapData[0, 0], friendlySettlementTerrain);
        mapData[0, 0].m_isFriendlyArea = true;
        ExpandSettlement(mapData, friendlySettlementTerrain, friendlySettlementCount, (x, y) => x < mapSize.x / 2 && y < mapSize.y / 2, true);

        ApplyTerrainTemplate(mapData[mapSize.x - 1, mapSize.y - 1], enemySettleTerrain);
        mapData[mapSize.x - 1, mapSize.y - 1].m_isFriendlyArea = false;
        ExpandSettlement(mapData, enemySettleTerrain, enemySettleCount, (x, y) => x >= mapSize.x / 2 && y >= mapSize.y / 2, false);
    }

    private void ExpandSettlement(TileMapState[,] mapData, TerrainType.TYPE typeToExpand, int targetCount, Func<int, int, bool> areaConstraint, bool isFriendly)
    {
        HashSet<Vector2Int> currentTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> frontier = new HashSet<Vector2Int>();
        bool isFallbackMode = false;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (mapData[x, y].m_terrainType == typeToExpand)
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

            ApplyTerrainTemplate(mapData[chosen.x, chosen.y], typeToExpand);
            mapData[chosen.x, chosen.y].m_isFriendlyArea = isFriendly;
            currentTiles.Add(chosen);

            AddNeighborsToFrontier(mapData, chosen, isFallbackMode ? (x, y) => true : areaConstraint, frontier, currentTiles);
        }
    }

    private void AddNeighborsToFrontier(TileMapState[,] mapData, Vector2Int tile, Func<int, int, bool> areaConstraint, HashSet<Vector2Int> frontier, HashSet<Vector2Int> currentTiles)
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

            if (neighbor.x >= 0 && neighbor.x < mapSize.x && neighbor.y >= 0 && neighbor.y < mapSize.y &&
                areaConstraint(neighbor.x, neighbor.y) &&
                !currentTiles.Contains(neighbor) &&
                !frontier.Contains(neighbor) &&
                mapData[neighbor.x, neighbor.y].m_terrainType == TerrainType.TYPE.None)
            {
                frontier.Add(neighbor);
            }
        }
    }

    private void AssignTerrainsToAllTiles(TileMapState[,] mapData)
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (mapData[x, y].m_terrainType == TerrainType.TYPE.None)
                {
                    TerrainType.TYPE chosenTerrainType = GetTerrainTypeWithNeighborInfluence(mapData, x, y);
                    ApplyTerrainTemplate(mapData[x, y], chosenTerrainType);
                }
            }
        }
    }

    private TerrainType.TYPE GetTerrainTypeWithNeighborInfluence(TileMapState[,] mapData, int x, int y)
    {
        // 도로가 있는 타일은 통과 불가능한 지형을 제외하고 선택
        var weightedList = terrainTemplates
            .Where(t => t.m_terrainType != friendlySettlementTerrain && 
                       t.m_terrainType != enemySettleTerrain && 
                       t.m_terrainType != TerrainType.TYPE.None)
            .Select(t => new { t.m_terrainType, t.m_weight })
            .ToList();

        // 도로가 있는 타일인 경우 통과 불가능한 지형 제외
        if (mapData[x, y].m_isRoad)
        {
            weightedList = weightedList
                .Where(t => !IsImpassableTerrain(t.m_terrainType))
                .ToList();
        }

        Vector2Int[] neighbors = { new Vector2Int(x + 1, y), new Vector2Int(x - 1, y), new Vector2Int(x, y + 1), new Vector2Int(x, y - 1) };
        foreach (var neighbor in neighbors)
        {
            if (neighbor.x >= 0 && neighbor.x < mapSize.x && neighbor.y >= 0 && neighbor.y < mapSize.y)
            {
                TerrainType.TYPE neighborTerrain = mapData[neighbor.x, neighbor.y].m_terrainType;
                var terrainToBoost = weightedList.FirstOrDefault(t => t.m_terrainType == neighborTerrain);
                if (terrainToBoost != null)
                {
                    int index = weightedList.FindIndex(t => t.m_terrainType == neighborTerrain);
                    weightedList[index] = new { terrainToBoost.m_terrainType, m_weight = (terrainToBoost.m_weight * 1.5f) };
                }
            }
        }

        float totalWeight = weightedList.Sum(t => t.m_weight);

        if (totalWeight <= 0) return TerrainType.TYPE.None;
        
        double randomDouble = pseudoRandom.NextDouble();

        float randomValue = (float)(randomDouble * totalWeight);

        foreach (var terrain in weightedList)
        {
            if (randomValue < terrain.m_weight)
            {
                return terrain.m_terrainType;
            }
            randomValue -= terrain.m_weight;
        }

        return TerrainType.TYPE.None;
    }

    private void ApplyTerrainTemplate(TileMapState tileState, TerrainType.TYPE type)
    {
        if (terrainTemplatesDictionary.TryGetValue(type, out var template))
        {
            tileState.m_terrainType = template.m_terrainType;
        }
        else
        {
            Debug.LogError($"지형 템플릿을 찾을 수 없습니다: {type}");
        }
    }

    private void AssignResourcesToAllTiles(TileMapState[,] mapData)
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                var state = mapData[x, y];
                if (terrainTemplatesDictionary.TryGetValue(state.m_terrainType, out var template))
                {
                    float baseValue;
                    bool isSettlement = state.m_terrainType == friendlySettlementTerrain || state.m_terrainType == enemySettleTerrain;
                    if (isSettlement)
                    {
                        float ratio = settleMul / mainMul;
                        if (state.m_isFriendlyArea)
                        {
                            baseValue = friendlySettleResourceBase * ratio;
                        }
                        else
                        {
                            baseValue = enemyTileResourceBase * ratio;
                        }
                    }
                    else
                    {
                        baseValue = normalTileResourceBase + ((x * x + y * y) / mainMul);
                    }

                    long wood = (long)(baseValue * template.m_woodMul);
                    long iron = (long)(baseValue * template.m_iromMul);
                    long food = (long)(baseValue * template.m_foodMul);
                    long tech = (long)(baseValue * template.m_techMul);

                    state.m_resources.SetAmount(ResourceType.TYPE.Wood, wood);
                    state.m_resources.SetAmount(ResourceType.TYPE.Iron, iron);
                    state.m_resources.SetAmount(ResourceType.TYPE.Food, food);
                    state.m_resources.SetAmount(ResourceType.TYPE.Tech, tech);
                }
                else
                {
                    state.m_resources.Clear();
                }
            }
        }
    }
}