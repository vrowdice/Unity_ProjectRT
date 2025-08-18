using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타일맵 관리를 담당하는 매니저 클래스
/// 맵 생성, 타일 상태 관리, 맵 크기 관리 등을 담당
/// </summary>
public class TileMapManager : MonoBehaviour
{
    [Header("Tile Map Data")]
    private List<TileMapData> m_tileMapDataList = new();
    
    // 타일맵 데이터
    private Dictionary<TerrainType.TYPE, TileMapData> m_tileMapDataDic = new();
    private TileMapState[,] m_tileMap;
    
    // 참조
    private GameDataManager m_gameDataManager;
    
    // 프로퍼티
    public Dictionary<TerrainType.TYPE, TileMapData> TileMapDataDict => m_tileMapDataDic;
    public TileMapState[,] TileMap => m_tileMap;
    
    /// <summary>
    /// 타일맵 매니저 초기화
    /// </summary>
    /// <param name="gameDataManager">게임 데이터 매니저 참조</param>
    public void Initialize(GameDataManager gameDataManager)
    {
        m_gameDataManager = gameDataManager;
        InitTileMapDict();
        GenerateTileMap();
    }
    
    /// <summary>
    /// 타일맵 데이터 딕셔너리 초기화
    /// </summary>
    private void InitTileMapDict()
    {
        m_tileMapDataDic.Clear();
        foreach (TileMapData tileMap in m_tileMapDataList)
        {
            if (!m_tileMapDataDic.ContainsKey(tileMap.m_terrainType))
            {
                m_tileMapDataDic.Add(tileMap.m_terrainType, tileMap);
            }
        }
    }
    
    /// <summary>
    /// GameBalanceData의 설정을 기반으로 타일맵을 생성합니다.
    /// </summary>
    private void GenerateTileMap()
    {
        if (m_gameDataManager.GameBalanceEntry?.m_data == null)
        {
            Debug.LogError("GameBalanceData가 설정되지 않았습니다.");
            return;
        }

        if (m_tileMapDataList == null || m_tileMapDataList.Count == 0)
        {
            Debug.LogWarning("타일맵 데이터가 없어서 맵 생성을 건너뜁니다.");
            return;
        }

        Vector2Int mapSize = m_gameDataManager.GameBalanceEntry.m_data.m_mapSize;
        int friendlySettleCount = m_gameDataManager.GameBalanceEntry.m_data.m_friendlySettle;
        int enemySettleCount = m_gameDataManager.GameBalanceEntry.m_data.m_enemySettle;

        // MapDataGenerator를 사용하여 맵 생성
        var balance = m_gameDataManager.GameBalanceEntry;
        var data = balance.m_data;
        MapDataGenerator mapGenerator = new MapDataGenerator(
            mapSize,
            m_tileMapDataList,
            TerrainType.TYPE.Settlement, // 친화적 정착지 타입
            friendlySettleCount,
            TerrainType.TYPE.Settlement, // 적대적 정착지 타입 (같은 타입 사용)
            enemySettleCount,
            data.m_friendlySettleResourceBase,
            data.m_normalTileResourceBase,
            data.m_enemyTileResourceBase,
            data.m_settleMul,
            balance.m_state.m_mainMul
        );

        m_tileMap = mapGenerator.GenerateMapData();
    }

    /// <summary>
    /// 지정된 시드로 타일맵을 재생성합니다.
    /// </summary>
    /// <param name="seed">맵 생성에 사용할 시드</param>
    public void RegenerateTileMap(string seed = null)
    {
        if (m_gameDataManager.GameBalanceEntry?.m_data == null)
        {
            Debug.LogError("GameBalanceData가 설정되지 않았습니다.");
            return;
        }

        Vector2Int mapSize = m_gameDataManager.GameBalanceEntry.m_data.m_mapSize;
        int friendlySettleCount = m_gameDataManager.GameBalanceEntry.m_data.m_friendlySettle;
        int enemySettleCount = m_gameDataManager.GameBalanceEntry.m_data.m_enemySettle;

        // MapDataGenerator를 사용하여 맵 생성 (시드 지정)
        var balance = m_gameDataManager.GameBalanceEntry;
        var data = balance.m_data;
        MapDataGenerator mapGenerator = new MapDataGenerator(
            mapSize,
            m_tileMapDataList,
            seed,
            TerrainType.TYPE.Settlement,
            friendlySettleCount,
            TerrainType.TYPE.Settlement,
            enemySettleCount,
            data.m_friendlySettleResourceBase,
            data.m_normalTileResourceBase,
            data.m_enemyTileResourceBase,
            data.m_settleMul,
            balance.m_state.m_mainMul
        );

        m_tileMap = mapGenerator.GenerateMapData();
        Debug.Log($"타일맵이 재생성되었습니다. 크기: {mapSize.x}x{mapSize.y}, 시드: {seed ?? "랜덤"}");
    }

    /// <summary>
    /// 지정된 위치의 타일맵 상태를 반환합니다.
    /// </summary>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    /// <returns>타일맵 상태, 범위를 벗어나면 null</returns>
    public TileMapState GetTileMapState(int x, int y)
    {
        if (m_tileMap == null)
        {
            Debug.LogWarning("타일맵이 생성되지 않았습니다.");
            return null;
        }

        if (x >= 0 && x < m_tileMap.GetLength(0) && y >= 0 && y < m_tileMap.GetLength(1))
        {
            return m_tileMap[x, y];
        }

        Debug.LogWarning($"타일맵 좌표가 범위를 벗어났습니다: ({x}, {y})");
        return null;
    }

    /// <summary>
    /// 타일맵의 크기를 반환합니다.
    /// </summary>
    /// <returns>타일맵 크기 (Vector2Int)</returns>
    public Vector2Int GetTileMapSize()
    {
        if (m_tileMap == null)
        {
            return Vector2Int.zero;
        }

        return new Vector2Int(m_tileMap.GetLength(0), m_tileMap.GetLength(1));
    }
    
    /// <summary>
    /// 타일맵 데이터 가져오기
    /// </summary>
    /// <param name="terrainType">지형 타입</param>
    /// <returns>타일맵 데이터</returns>
    public TileMapData GetTileMapData(TerrainType.TYPE terrainType)
    {
        return m_tileMapDataDic.TryGetValue(terrainType, out var data) ? data : null;
    }
    
    /// <summary>
    /// 타일맵 데이터 리스트 설정 (에디터용)
    /// </summary>
    /// <param name="tileMapDataList">타일맵 데이터 리스트</param>
    public void SetTileMapDataList(List<TileMapData> tileMapDataList)
    {
        m_tileMapDataList = tileMapDataList;
    }
} 