using System;
using UnityEngine;
using System.Collections.Generic; // For List and Queue

/// <summary>
/// 미니맵을 위한 절차적 맵 생성기
/// </summary>
public class TileMapGenerator : MonoBehaviour
{
    // --- Tile Prefabs ---
    [Header("Tile Prefab")]
    [SerializeField] private GameObject defaultTilePrefab; // 단일 프리팹 사용

    // --- Map Settings ---
    [Header("Map Settings")]
    [SerializeField] private Vector2Int mapSize = new Vector2Int(10, 10); // Fixed 10x10 mini-map

    // --- Generation Settings ---
    [Header("Generation Settings")]
    public string seed;
    public bool useRandomSeed;

    // --- Generated Map Data ---
    public TileType[,] MapTiles;
    public TerrainType[,] MapTerrains;
    public int[,] EnemyCombatPower; // To store combat power for enemy territories

    // --- Constants for specific tile counts ---
    private const int FRIENDLY_SETTLEMENT_COUNT = 7;
    private const int ENEMY_STRONGHOLD_COUNT = 15;

    // --- Helper for random number generation ---
    private System.Random pseudoRandom;

    // --- Tile Colors ---
    // 각 타일 타입 및 지형 타입에 대한 색상 정의
    [Header("Tile Colors")]
    public Color friendlyAreaColor = Color.cyan;
    public Color enemyTerritoryColor = new Color(0.8f, 0.2f, 0.2f); // Dark Red
    public Color friendlySettlementColor = Color.blue;
    public Color enemyStrongholdColor = Color.magenta;
    public Color roadColor = Color.yellow;

    public Color plainColor = Color.green;
    public Color forestColor = new Color(0.1f, 0.5f, 0.1f); // Dark Green
    public Color mountainColor = new Color(0.5f, 0.5f, 0.5f); // Gray
    public Color riverColor = Color.blue; // Overriding for River specific
    public Color volcanoColor = new Color(0.4f, 0.2f, 0.0f); // Brown/Orange for Volcano

    /// <summary>
    /// 맵 생성 및 그리기 시작
    /// </summary>
    void Start()
    {
        InitializeRandomSeed();
        GenerateMiniMap();
        DrawMapTiles();
    }

    /// <summary>
    /// 새로운 맵 생성 (런타임에서 호출 가능)
    /// </summary>
    [ContextMenu("Generate New Map")]
    public void GenerateNewMap()
    {
        // 기존 타일들 제거
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        InitializeRandomSeed();
        GenerateMiniMap();
        DrawMapTiles();
    }

    /// <summary>
    /// 랜덤 시드 초기화
    /// </summary>
    private void InitializeRandomSeed()
    {
        if (useRandomSeed)
            seed = DateTime.Now.ToBinary().ToString();
        pseudoRandom = new System.Random(seed.GetHashCode());
    }

    /// <summary>
    /// 미니맵 생성의 전체 프로세스
    /// </summary>
    private void GenerateMiniMap()
    {
        MapTiles = new TileType[mapSize.x, mapSize.y];
        MapTerrains = new TerrainType[mapSize.x, mapSize.y];
        EnemyCombatPower = new int[mapSize.x, mapSize.y]; // Initialize for enemy data

        // 1. 기본 맵 초기화 (적의 타일로 설정)
        InitializeMapAsEnemyTerritory();

        // 2. 길 생성
        GenerateRoads();

        // 3. 아군 타일 및 적 본진 타일 배치 (개척지)
        PlaceSpecialSettlements();

        // 4. 나머지 지형 배정
        AssignTerrainsToRemainingTiles();

        // 5. 적 영토에 전투력 및 산출량 데이터 배치 (TODO: 상세 구현 필요)
        PlaceEnemyDataAndResources();
    }

    /// <summary>
    /// 기본적으로 왼쪽 상단 모서리 일부를 제외한 나머지 타일은 적의 타일로 지정됩니다.
    /// (x=0, y=0을 포함하는 3x3 영역을 아군 시작점으로 가정)
    /// </summary>
    private void InitializeMapAsEnemyTerritory()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // Assuming top-left 3x3 (x<3, y<3) is initially "friendly area" for visual distinction
                // This will be overwritten later by specific settlement placement
                if (x < 3 && y < 3)
                {
                    MapTiles[x, y] = TileType.FriendlyArea;
                }
                else
                {
                    MapTiles[x, y] = TileType.EnemyTerritory;
                }
            }
        }
        // 오른쪽 하단 모서리는 적의 본진 타일
        MapTiles[mapSize.x - 1, mapSize.y - 1] = TileType.EnemyStronghold;
    }

    /// <summary>
    /// 최소 2개의 길 생성
    /// 길: [1,1] ~ [10,10]의 x=y인 대각선 타일을 기준으로 상/하단의 타일을 하나 지정한 뒤 해당 타일을
    /// 경유하며 [1,1] → [10,10]으로 향하는 최단 거리의 경로 2개를 생성한다.
    /// </summary>
    private void GenerateRoads()
    {
        // Path 1: Top-side of diagonal
        GenerateSingleRoad(new Vector2Int(0, 0), new Vector2Int(mapSize.x - 1, mapSize.y - 1), true);

        // Path 2: Bottom-side of diagonal
        GenerateSingleRoad(new Vector2Int(0, 0), new Vector2Int(mapSize.x - 1, mapSize.y - 1), false);
    }

    /// <summary>
    /// 단일 길 생성 (최단 경로)
    /// </summary>
    /// <param name="start">시작 지점</param>
    /// <param name="end">끝 지점</param>
    /// <param name="useTopOffset">대각선 기준 상단 오프셋을 사용할지 (true) 하단 오프셋을 사용할지 (false)</param>
    private void GenerateSingleRoad(Vector2Int start, Vector2Int end, bool useTopOffset)
    {
        // Find a pivot point near the diagonal
        Vector2Int pivot;
        int maxOffset = 2; // Max offset from the diagonal (e.g., 1 or 2 tiles)
        int offset = pseudoRandom.Next(1, maxOffset + 1);

        if (useTopOffset)
        {
            // Pivot above diagonal: (x, y-offset) or (x+offset, y)
            pivot = new Vector2Int(pseudoRandom.Next(start.x + 1, end.x - 1), pseudoRandom.Next(start.y + 1, end.y - 1));
            // Ensure pivot is generally "above" the diagonal or to the left
            if (pivot.x > pivot.y) pivot.y -= offset; // Move up
            else pivot.x += offset; // Move right
        }
        else
        {
            // Pivot below diagonal: (x, y+offset) or (x-offset, y)
            pivot = new Vector2Int(pseudoRandom.Next(start.x + 1, end.x - 1), pseudoRandom.Next(start.y + 1, end.y - 1));
            // Ensure pivot is generally "below" the diagonal or to the right
            if (pivot.x < pivot.y) pivot.y += offset; // Move down
            else pivot.x -= offset; // Move left
        }

        // Clamp pivot to map boundaries
        pivot.x = Mathf.Clamp(pivot.x, 0, mapSize.x - 1);
        pivot.y = Mathf.Clamp(pivot.y, 0, mapSize.y - 1);

        // Use a simple pathfinding (e.g., A* or Breadth-First Search) for shortest path
        // For a 10x10 grid, Manhattan distance-based path is sufficient.
        // This is a simplified direct path, not a true shortest path algorithm.
        MarkPathAsRoad(start, pivot);
        MarkPathAsRoad(pivot, end);
    }

    /// <summary>
    /// 두 지점 사이의 최단 경로를 길로 표시 (맨해튼 거리 기반)
    /// </summary>
    /// <param name="p1">시작 지점</param>
    /// <param name="p2">끝 지점</param>
    private void MarkPathAsRoad(Vector2Int p1, Vector2Int p2)
    {
        int currX = p1.x;
        int currY = p1.y;

        while (currX != p2.x || currY != p2.y)
        {
            MapTiles[currX, currY] = TileType.Road;

            if (currX != p2.x)
            {
                currX += (p2.x > currX) ? 1 : -1;
            }
            else if (currY != p2.y)
            {
                currY += (p2.y > currY) ? 1 : -1;
            }
        }
        MapTiles[p2.x, p2.y] = TileType.Road; // Ensure the end point is also marked
    }


    /// <summary>
    /// 1. 아군 타일과 적 본진 타일은 개척지라는 특수 타일을 배치한다.
    /// (아군 타일: 7타일, 적 본진 타일: 15타일 고정)
    /// 1-1. [1,1]에 아군, [10,10]에 적군 타일을 하나씩 배치한다.
    /// 1-2. 아군 타일은 x,y < 4이내의 9칸, 적 본진은 x<5, y<6의 20칸 내에서 인접한 상하좌우 타일 중 하나로 퍼져나간다.
    /// 1-3. 해당 과정은 아군 타일이 7개, 적 본진 타일이 15개가 될 때까지 한 타일씩만 퍼지도록 반복한다.
    /// </summary>
    private void PlaceSpecialSettlements()
    {
        // Place initial tiles (adjusting for 0-indexed arrays: [0,0] and [9,9])
        MapTiles[0, 0] = TileType.FriendlySettlement;
        MapTiles[mapSize.x - 1, mapSize.y - 1] = TileType.EnemyStronghold;

        // Expand Friendly Settlement
        ExpandSettlement(TileType.FriendlySettlement, FRIENDLY_SETTLEMENT_COUNT,
                         (x, y) => x < 3 && y < 3); // within 3x3 top-left area (0-indexed x<3, y<3)

        // Expand Enemy Stronghold
        ExpandSettlement(TileType.EnemyStronghold, ENEMY_STRONGHOLD_COUNT,
                         (x, y) => x >= mapSize.x - 5 && y >= mapSize.y - 6); // within x<5, y<6 area for 10x10 map (9-5=4, 9-6=3. So from x=5 to 9, y=4 to 9)
    }

    /// <summary>
    /// 특정 타입의 개척지를 지정된 개수만큼 확장
    /// </summary>
    /// <param name="typeToExpand">확장할 타일 타입</param>
    /// <param name="targetCount">목표 타일 개수</param>
    /// <param name="areaConstraint">확장 가능한 영역 제약 조건</param>
    private void ExpandSettlement(TileType typeToExpand, int targetCount, Func<int, int, bool> areaConstraint)
    {
        List<Vector2Int> currentTiles = new List<Vector2Int>();
        // Find initial tiles of the given type
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (MapTiles[x, y] == typeToExpand)
                {
                    currentTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        while (currentTiles.Count < targetCount)
        {
            // Get all potential neighbors
            List<Vector2Int> potentialNewTiles = new List<Vector2Int>();
            foreach (var tile in currentTiles)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue; // Skip self
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) > 1) continue; // Only cardinal directions (no diagonals)

                        int nx = tile.x + dx;
                        int ny = tile.y + dy;

                        if (nx >= 0 && nx < mapSize.x && ny >= 0 && ny < mapSize.y &&
                            areaConstraint(nx, ny) && // Within the allowed expansion area
                            MapTiles[nx, ny] != typeToExpand) // Not already part of this settlement
                        {
                            potentialNewTiles.Add(new Vector2Int(nx, ny));
                        }
                    }
                }
            }

            // Remove duplicates
            potentialNewTiles = new List<Vector2Int>(new HashSet<Vector2Int>(potentialNewTiles));

            if (potentialNewTiles.Count > 0)
            {
                // Randomly pick one new tile to expand to
                Vector2Int chosenTile = potentialNewTiles[pseudoRandom.Next(0, potentialNewTiles.Count)];
                MapTiles[chosenTile.x, chosenTile.y] = typeToExpand;
                currentTiles.Add(chosenTile);
            }
            else
            {
                Debug.LogWarning($"Could not expand {typeToExpand} further. Reached {currentTiles.Count} of {targetCount} tiles.");
                break; // Cannot expand further
            }
        }
    }


    /// <summary>
    /// 2-1. 적/아군 본진 타일을 제외한 무작위 타일에 임시로 1~N의 숫자를 부여. (N = 남은 타일 수)
    /// 2-2. 확률에 따라 1~N의 타일에 순서대로 지형을 부여.
    /// 2-3. 각 타일은 인접한 타일로 1개씩 퍼져나간다. 이 때 부여된 숫자의 오름차순대로 진행하며,
    ///      확장을 진행하려는 타일에 지형이 부여되어있지 않아야 퍼져나갈 수 있다.
    /// 2-4. 산, 강, 휴화산 타일은 '길' 속성이 부여된 타일로 퍼져나갈 수 없다.
    /// 2-5. 각 타일은 인접한 타일에 지형이 부여되어있지 않은 타일이 없거나 남은 공간이 없을 경우 확장을 중단한다.
    /// </summary>
    private void AssignTerrainsToRemainingTiles()
    {
        List<Vector2Int> unassignedTiles = new List<Vector2Int>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // Only consider tiles that are NOT special settlements or roads initially
                if (MapTiles[x, y] != TileType.FriendlySettlement && MapTiles[x, y] != TileType.EnemyStronghold && MapTiles[x, y] != TileType.Road)
                {
                    unassignedTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        // Randomly assign a "priority" number to each unassigned tile
        // (This step might be simplified by simply shuffling the list)
        // For simplicity, we'll shuffle the list and iterate, treating the order as priority.
        ShuffleList(unassignedTiles);

        // Define terrain probabilities (These need to be adjusted based on your 15p document)
        // Example probabilities - adjust these as needed. Sum should be 100 or less if some tiles remain undefined.
        Dictionary<TerrainType, int> terrainProbabilities = new Dictionary<TerrainType, int>
        {
            { TerrainType.Plain, 40 },
            { TerrainType.Forest, 30 },
            { TerrainType.Mountain, 10 },
            { TerrainType.River, 10 },
            { TerrainType.Volcano, 5 } // Assuming Volcano is your "휴화산" (extinct volcano)
        };


        // Iterate through shuffled unassigned tiles and assign terrains
        foreach (Vector2Int tilePos in unassignedTiles)
        {
            if (MapTerrains[tilePos.x, tilePos.y] != TerrainType.None) continue; // Already assigned during expansion

            // Select a terrain type based on probability
            TerrainType chosenTerrain = GetRandomTerrainType(terrainProbabilities);

            // If a terrain is chosen, start its expansion
            if (chosenTerrain != TerrainType.None)
            {
                ExpandTerrain(tilePos, chosenTerrain);
            }
        }

        // Ensure all unassigned tiles have a terrain (e.g., default to Plain if nothing else fits)
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (MapTerrains[x, y] == TerrainType.None &&
                    MapTiles[x, y] != TileType.FriendlySettlement &&
                    MapTiles[x, y] != TileType.EnemyStronghold &&
                    MapTiles[x, y] != TileType.Road) // Don't overwrite special tiles
                {
                    // Default unassigned tiles to Plain, or handle based on your game logic
                    MapTerrains[x, y] = TerrainType.Plain;
                }
            }
        }
    }

    /// <summary>
    /// 리스트 셔플 (Fisher-Yates)
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = pseudoRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// 확률에 따라 랜덤 지형 타입 선택
    /// </summary>
    private TerrainType GetRandomTerrainType(Dictionary<TerrainType, int> probabilities)
    {
        int totalWeight = 0;
        foreach (var pair in probabilities)
        {
            totalWeight += pair.Value;
        }

        int randomValue = pseudoRandom.Next(0, totalWeight);

        foreach (var pair in probabilities)
        {
            if (randomValue < pair.Value)
            {
                return pair.Key;
            }
            randomValue -= pair.Value;
        }
        return TerrainType.None; // Should not happen if probabilities sum to totalWeight
    }

    /// <summary>
    /// 지형 확장 로직
    /// </summary>
    /// <param name="startPos">시작 지점</param>
    /// <param name="terrainType">확장할 지형 타입</param>
    private void ExpandTerrain(Vector2Int startPos, TerrainType terrainType)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startPos);
        MapTerrains[startPos.x, startPos.y] = terrainType;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // Check adjacent tiles (cardinal directions)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) > 1) continue; // Cardinal directions only

                    int nx = current.x + dx;
                    int ny = current.y + dy;

                    if (nx >= 0 && nx < mapSize.x && ny >= 0 && ny < mapSize.y)
                    {
                        // Expansion condition: target tile must not have a terrain assigned yet
                        // AND it should not be a special settlement or road
                        if (MapTerrains[nx, ny] == TerrainType.None &&
                            MapTiles[nx, ny] != TileType.FriendlySettlement &&
                            MapTiles[nx, ny] != TileType.EnemyStronghold &&
                            MapTiles[nx, ny] != TileType.Road)
                        {
                            // Rule 2-4: 산, 강, 휴화산 타일은 '길' 속성이 부여된 타일로 퍼져나갈 수 없다.
                            if ((terrainType == TerrainType.Mountain || terrainType == TerrainType.River || terrainType == TerrainType.Volcano) && MapTiles[nx, ny] == TileType.Road)
                            {
                                continue; // Cannot expand onto a road
                            }

                            MapTerrains[nx, ny] = terrainType;
                            queue.Enqueue(new Vector2Int(nx, ny)); // Add to queue for further expansion
                            return; // Each tile expands to only one adjacent tile per "step"
                                    // This return makes it expand one tile at a time, mimicking the "한 타일씩만 퍼지도록 반복" for settlements
                                    // If you want full BFS expansion for terrains, remove this return.
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 3. 적 영토에 정해진 전투력 범위에 따라 적 등장 데이터값, 산출량 데이터 값 등을 배치.
    /// (TODO: 상세 구현 필요)
    /// </summary>
    private void PlaceEnemyDataAndResources()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (MapTiles[x, y] == TileType.EnemyTerritory || MapTiles[x, y] == TileType.EnemyStronghold)
                {
                    // Based on terrain type and/or random chance, assign enemy combat power.
                    // This is where your "전투력 범위" and "산출량 데이터" logic would go.
                    // Example:
                    switch (MapTerrains[x, y])
                    {
                        case TerrainType.Plain:
                            EnemyCombatPower[x, y] = pseudoRandom.Next(1, 5); // Low combat power
                            break;
                        case TerrainType.Forest:
                            EnemyCombatPower[x, y] = pseudoRandom.Next(3, 8); // Medium combat power
                            break;
                        case TerrainType.Mountain:
                            EnemyCombatPower[x, y] = pseudoRandom.Next(7, 12); // High combat power
                            break;
                        case TerrainType.River:
                            EnemyCombatPower[x, y] = pseudoRandom.Next(2, 6);
                            break;
                        case TerrainType.Volcano:
                            EnemyCombatPower[x, y] = pseudoRandom.Next(10, 15); // Very high combat power
                            break;
                        case TerrainType.None: // Special tiles might not have a terrain type explicitly assigned
                            // Handle cases for special tiles if needed, maybe fixed combat power or no enemies
                            EnemyCombatPower[x, y] = 0;
                            break;
                    }
                    // Implement resource output logic here as well
                }
            }
        }
    }

    /// <summary>
    /// 생성된 맵 데이터를 기반으로 타일 오브젝트 생성 및 색상 설정
    /// </summary>
    private void DrawMapTiles()
    {
        if (defaultTilePrefab == null)
        {
            Debug.LogError("Default Tile Prefab is not assigned!");
            return;
        }

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 pos = new Vector3(x, y, 0); // 각 타일이 월드 공간에서 1 단위 크기를 차지한다고 가정

                GameObject tileInstance = Instantiate(defaultTilePrefab, pos, Quaternion.identity, gameObject.transform);
                SpriteRenderer spriteRenderer = tileInstance.GetComponent<SpriteRenderer>();

                if (spriteRenderer == null)
                {
                    Debug.LogWarning($"Tile Prefab at ({x},{y}) does not have a SpriteRenderer component. Cannot set color.");
                    continue;
                }

                // TileType에 따른 색상 우선순위
                Color targetColor;
                switch (MapTiles[x, y])
                {
                    case TileType.FriendlySettlement:
                        targetColor = friendlySettlementColor;
                        break;
                    case TileType.EnemyStronghold:
                        targetColor = enemyStrongholdColor;
                        break;
                    case TileType.Road:
                        targetColor = roadColor;
                        break;
                    case TileType.FriendlyArea:
                        targetColor = friendlyAreaColor;
                        break;
                    case TileType.EnemyTerritory:
                        // 적 영토는 지형에 따라 색상 결정
                        targetColor = GetColorForTerrain(MapTerrains[x, y]);
                        break;
                    default:
                        // 기본적으로는 지형 색상 적용
                        targetColor = GetColorForTerrain(MapTerrains[x, y]);
                        break;
                }
                spriteRenderer.color = targetColor;
            }
        }
    }

    /// <summary>
    /// 지형 타입에 맞는 색상 가져오기
    /// </summary>
    private Color GetColorForTerrain(TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Plain:
                return plainColor;
            case TerrainType.Forest:
                return forestColor;
            case TerrainType.Mountain:
                return mountainColor;
            case TerrainType.River:
                return riverColor;
            case TerrainType.Volcano:
                return volcanoColor;
            case TerrainType.None: // 지형이 할당되지 않은 경우 (예: 버그 또는 초기 상태)
                return Color.white; // 기본 흰색
            default:
                return Color.white;
        }
    }
}

/// <summary>
/// 타일의 기본적인 분류 (아군/적군/길/본진)
/// </summary>
public enum TileType
{
    None, // Unassigned
    FriendlyArea, // Initial generic friendly area
    EnemyTerritory, // General enemy owned territory
    FriendlySettlement, // Specific friendly "개척지"
    EnemyStronghold, // Specific enemy "본진"
    Road // Designated path
}

/// <summary>
/// 타일의 지형 유형
/// </summary>
public enum TerrainType
{
    None, // Unassigned
    Plain, // 평지
    Forest, // 숲
    Mountain, // 산
    River, // 강
    Volcano // 휴화산
}