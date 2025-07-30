using System;
using UnityEngine;

/// <summary>
/// 셀룰러 오토마타를 사용한 절차적 맵 생성기
/// </summary>
public class TileMapGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Grid grid;
    
    [Header("Tile Prefabs")]
    [SerializeField] private Blocks[] tilePrefabs;

    [Header("Generation Settings")]
    public string seed;
    public bool useRandomSeed;
    
    [SerializeField] private Vector2Int size = new Vector2Int(50, 50);
    [SerializeField, Range(0, 100)]
    private int normalBlockPercent = 45;
    [SerializeField, Range(0, 100)]
    private int normalBiomePercent = 45;

    [Header("Smoothing")]
    [SerializeField] private int smoothingFactor = 5;
    
    [Header("Generated Map Data")]
    public int[,] MapHeights;
    public int[,] MapBiomes;
    
    /// <summary>
    /// 맵 생성 및 그리기 시작
    /// </summary>
    void Start()
    {
        GenerateMap();
        DrawMapTiles();
    }

    /// <summary>
    /// 셀룰러 오토마타를 사용한 맵 생성
    /// </summary>
    void GenerateMap()
    {
        MapHeights = new int[size.x, size.y];
        MapBiomes = new int[size.x, size.y];
        RandomFillMap();

        // 스무딩을 여러 번 적용하여 자연스러운 형태 생성
        for (int i = 0; i < smoothingFactor; i++)
        {
            SmoothMap(MapHeights);
            SmoothMap(MapBiomes);
        }
    }

    /// <summary>
    /// 랜덤으로 맵 초기화
    /// </summary>
    void RandomFillMap()
    {
        if (useRandomSeed)
            seed = DateTime.Now.ToBinary().ToString();

        System.Random psuedoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // 경계는 벽으로 설정
                if (x == 0 || x == size.x - 1 || y == 0 || y == size.y - 1)
                    MapHeights[x, y] = 1;
                else 
                    MapHeights[x, y] = psuedoRandom.Next(0, 100) < normalBlockPercent ? 0 : 1;
                
                MapBiomes[x, y] = psuedoRandom.Next(0, 100) < normalBiomePercent ? 0 : 1;
            }
        }
    }

    /// <summary>
    /// 셀룰러 오토마타 스무딩 적용
    /// </summary>
    /// <param name="map">스무딩할 맵</param>
    void SmoothMap(int[,] map)
    {
        int[,] newMap = new int[size.x, size.y];
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int neighbourDifferentTiles = GetSurroundingTiles(map, x, y);

                // 주변 타일의 비율에 따라 결정
                if (neighbourDifferentTiles > 4)
                    map[x, y] = 1;
                else if (neighbourDifferentTiles < 4)
                    map[x, y] = 0;
                // 4개면 현재 상태 유지
            }
        }
    }

    /// <summary>
    /// 주변 타일 중 1인 타일의 개수 계산
    /// </summary>
    /// <param name="map">확인할 맵</param>
    /// <param name="gridX">X 좌표</param>
    /// <param name="gridY">Y 좌표</param>
    /// <returns>주변 1인 타일 개수</returns>
    int GetSurroundingTiles(int[,] map, int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < size.x && neighbourY >= 0 && neighbourY < size.y)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                        wallCount += map[neighbourX, neighbourY];
                }
                else 
                    wallCount++; // 경계 밖은 벽으로 처리
            }
        }

        return wallCount;
    }

    /// <summary>
    /// 생성된 맵 데이터를 기반으로 타일 오브젝트 생성
    /// </summary>
    void DrawMapTiles()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 pos = grid.CellToWorld(new Vector3Int(x, y, 0));
                Instantiate(tilePrefabs[MapBiomes[x, y]][MapHeights[x, y]], pos, Quaternion.identity, gameObject.transform);
            }
        }
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
        
        GenerateMap();
        DrawMapTiles();
    }
}

/// <summary>
/// 바이옴별 타일 프리팹을 관리하는 클래스
/// </summary>
[Serializable]
public class Blocks
{
    public GameObject normalBlock;
    public GameObject highBlock;

    /// <summary>
    /// 인덱스로 타일 프리팹 접근
    /// </summary>
    /// <param name="i">0: 일반 블록, 1: 높은 블록</param>
    /// <returns>해당하는 타일 프리팹</returns>
    public GameObject this[int i]
    {
        get { return (i == 0) ? normalBlock : highBlock; }
    }
} 