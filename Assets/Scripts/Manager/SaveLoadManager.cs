using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 게임의 저장/로드 기능을 전담하는 매니저 클래스
/// GameDataManager와 독립적으로 작동하며 싱글톤 패턴 사용
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SaveLoadManager Instance { get; private set; }

    [Header("Save Settings")]
    [SerializeField] private int maxSaveSlots = 10;
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5분

    // 자동 저장 타이머
    private float autoSaveTimer = 0f;

    #region Unity Lifecycle
    void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        // 자동 저장 처리
        if (enableAutoSave)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
    }
    #endregion

    #region Save Data Structures
    
    /// <summary>
    /// 게임 저장 데이터 구조체
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        [Header("Save Info")]
        public string saveDateTime;
        public string gameVersion = "1.0.0";
        public int saveSlot;
        
        [Header("Game Progress")]
        public int gameDate;
        public long wealthToken;
        public long exchangeToken;
        
        [Header("Faction States")]
        public List<FactionSaveData> factionStates = new();
        
        [Header("Building States")]
        public List<BuildingSaveData> buildingStates = new();
        
        [Header("Research States")]
        public List<ResearchSaveData> researchStates = new();
        
        [Header("Request States")]
        public List<RequestSaveData> acceptableRequests = new();
        public List<RequestSaveData> acceptedRequests = new();
        
        [Header("Resource Data")]
        public ResourceSaveData resourceData = new();
        
        [Header("Game Balance")]
        public GameBalanceSaveData gameBalanceData = new();
        
        [Header("Event States")]
        public EventSaveData eventData = new();
        
        [Header("TileMap States")]
        public List<TileMapSaveData> tileMapStates = new();
    }
    
    [System.Serializable]
    public class FactionSaveData
    {
        public FactionType.TYPE factionType;
        public int like;
    }
    
    [System.Serializable]
    public class BuildingSaveData
    {
        public string buildingCode;
        public long amount;
        public bool isUnlocked;
    }
    
    [System.Serializable]
    public class ResearchSaveData
    {
        public FactionType.TYPE factionType;
        public string researchCode;
        public bool isCompleted;
        public bool isLocked;
    }
    
    [System.Serializable]
    public class RequestSaveData
    {
        public RequestType.TYPE requestType;
        public FactionType.TYPE factionType;
        public bool isContact;
        public int factionAddLike;
        public string title;
        public string description;
        // RequestCompleteCondition과 보상은 런타임에 재생성
    }
    
    [System.Serializable]
    public class ResourceSaveData
    {
        public List<ResourceAmountSaveData> resources = new();
    }
    
    [System.Serializable]
    public class ResourceAmountSaveData
    {
        public ResourceType.TYPE type;
        public long amount;
    }
    
    [System.Serializable]
    public class GameBalanceSaveData
    {
        public float mainMul = 1.0f;
        public float dateMul = 1.0f;
    }
    
    [System.Serializable]
    public class EventSaveData
    {
        public List<ActiveEventSaveData> activeEvents = new();
    }
    
    [System.Serializable]
    public class ActiveEventSaveData
    {
        public string eventCode;
        public int remainingDuration;
    }
    
    [System.Serializable]
    public class TileMapSaveData
    {
        public int x, y;
        public TerrainType.TYPE terrainType;
        public bool isFriendlyArea;
        public bool isRoad;
        public GameResourcesSaveData resources;
    }
    
    [System.Serializable]
    public class GameResourcesSaveData
    {
        public long wood, iron, food, tech;
    }
    
    #endregion

    #region Save/Load Operations
    
    /// <summary>
    /// 현재 게임 상태를 저장 데이터로 변환
    /// </summary>
    /// <returns>저장 데이터</returns>
    public GameSaveData CreateSaveData()
    {
        var saveData = new GameSaveData();
        
        // 기본 정보
        saveData.saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // GameManager에서 기본 게임 정보 수집
        if (GameManager.Instance != null)
        {
            saveData.gameDate = GameManager.Instance.Date;
        }
        
        // GameDataManager에서 데이터 수집
        if (GameDataManager.Instance != null)
        {
            var gameDataManager = GameDataManager.Instance;
            
            // 자원 정보
            if (gameDataManager.ResourceManager != null)
            {
                saveData.wealthToken = gameDataManager.ResourceManager.WealthToken;
                saveData.exchangeToken = gameDataManager.ResourceManager.ExchangeToken;
                SaveResourceStates(saveData, gameDataManager);
            }
            
            // 각 상태 데이터 수집
            SaveFactionStates(saveData, gameDataManager);
            SaveBuildingStates(saveData, gameDataManager);
            SaveResearchStates(saveData, gameDataManager);
            SaveRequestStates(saveData, gameDataManager);
            SaveGameBalanceState(saveData, gameDataManager);
            SaveEventStates(saveData, gameDataManager);
            SaveTileMapStates(saveData, gameDataManager);
        }
        
        return saveData;
    }
    
    /// <summary>
    /// 저장 데이터로부터 게임 상태 복원
    /// </summary>
    /// <param name="saveData">저장 데이터</param>
    public void LoadFromSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("[SaveLoadManager] Save data is null!");
            return;
        }
        
        Debug.Log($"[SaveLoadManager] Loading game data from {saveData.saveDateTime}");
        
        // GameManager 상태 복원
        if (GameManager.Instance != null)
        {
            // GameManager에 Date 설정 메서드 추가 필요
            // GameManager.Instance.SetDate(saveData.gameDate);
        }
        
        // GameDataManager 상태 복원
        if (GameDataManager.Instance != null)
        {
            var gameDataManager = GameDataManager.Instance;
            
            // 자원 복원
            if (gameDataManager.ResourceManager != null)
            {
                LoadResourceStates(saveData, gameDataManager);
                
                // 토큰 복원
                long wealthDiff = saveData.wealthToken - gameDataManager.ResourceManager.WealthToken;
                long exchangeDiff = saveData.exchangeToken - gameDataManager.ResourceManager.ExchangeToken;
                gameDataManager.ResourceManager.ChangeTokens(wealthDiff, exchangeDiff);
            }
            
            // 각 상태 데이터 복원
            LoadFactionStates(saveData, gameDataManager);
            LoadBuildingStates(saveData, gameDataManager);
            LoadResearchStates(saveData, gameDataManager);
            LoadRequestStates(saveData, gameDataManager);
            LoadGameBalanceState(saveData, gameDataManager);
            LoadEventStates(saveData, gameDataManager);
            LoadTileMapStates(saveData, gameDataManager);
        }
        
        Debug.Log($"[SaveLoadManager] Game loaded successfully");
    }
    
    /// <summary>
    /// JSON 파일로 게임 저장
    /// </summary>
    /// <param name="slotIndex">저장 슬롯 번호</param>
    /// <returns>저장 성공 여부</returns>
    public bool SaveGameToFile(int slotIndex = 0)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveLoadManager] Invalid save slot: {slotIndex}");
            return false;
        }
        
        try
        {
            var saveData = CreateSaveData();
            saveData.saveSlot = slotIndex;
            
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = GetSaveFilePath(slotIndex);
            
            File.WriteAllText(filePath, json);
            Debug.Log($"[SaveLoadManager] Game saved to slot {slotIndex}: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Failed to save game to slot {slotIndex}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// JSON 파일에서 게임 로드
    /// </summary>
    /// <param name="slotIndex">저장 슬롯 번호</param>
    /// <returns>로드 성공 여부</returns>
    public bool LoadGameFromFile(int slotIndex = 0)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveLoadManager] Invalid save slot: {slotIndex}");
            return false;
        }
        
        try
        {
            string filePath = GetSaveFilePath(slotIndex);
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveLoadManager] Save file not found: {filePath}");
                return false;
            }
            
            string json = File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            LoadFromSaveData(saveData);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Failed to load game from slot {slotIndex}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 자동 저장
    /// </summary>
    public void AutoSave()
    {
        const int AUTO_SAVE_SLOT = 0; // 슬롯 0을 자동 저장용으로 사용
        if (SaveGameToFile(AUTO_SAVE_SLOT))
        {
            Debug.Log("[SaveLoadManager] Auto save completed");
        }
    }
    
    #endregion

    #region File Management
    
    /// <summary>
    /// 저장 파일 경로 생성
    /// </summary>
    /// <param name="slotIndex">슬롯 번호</param>
    /// <returns>파일 경로</returns>
    private string GetSaveFilePath(int slotIndex)
    {
        string saveDirectory = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        return Path.Combine(saveDirectory, $"save_slot_{slotIndex:D2}.json");
    }
    
    /// <summary>
    /// 저장 파일 존재 여부 확인
    /// </summary>
    /// <param name="slotIndex">슬롯 번호</param>
    /// <returns>존재 여부</returns>
    public bool HasSaveFile(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots) return false;
        return File.Exists(GetSaveFilePath(slotIndex));
    }
    
    /// <summary>
    /// 저장 파일 정보 가져오기
    /// </summary>
    /// <param name="slotIndex">슬롯 번호</param>
    /// <returns>저장 파일 정보 (없으면 null)</returns>
    public SaveFileInfo GetSaveFileInfo(int slotIndex)
    {
        if (!HasSaveFile(slotIndex)) return null;
        
        try
        {
            string filePath = GetSaveFilePath(slotIndex);
            string json = File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            return new SaveFileInfo
            {
                slotIndex = slotIndex,
                saveDateTime = saveData.saveDateTime,
                gameDate = saveData.gameDate,
                gameVersion = saveData.gameVersion,
                fileSize = new FileInfo(filePath).Length
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Failed to get save file info for slot {slotIndex}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 저장 파일 삭제
    /// </summary>
    /// <param name="slotIndex">슬롯 번호</param>
    /// <returns>삭제 성공 여부</returns>
    public bool DeleteSaveFile(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveLoadManager] Invalid save slot: {slotIndex}");
            return false;
        }
        
        try
        {
            string filePath = GetSaveFilePath(slotIndex);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[SaveLoadManager] Save file deleted: slot {slotIndex}");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Failed to delete save file slot {slotIndex}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 모든 저장 파일 정보 가져오기
    /// </summary>
    /// <returns>저장 파일 정보 리스트</returns>
    public List<SaveFileInfo> GetAllSaveFileInfos()
    {
        var infos = new List<SaveFileInfo>();
        
        for (int i = 0; i < maxSaveSlots; i++)
        {
            var info = GetSaveFileInfo(i);
            if (info != null)
            {
                infos.Add(info);
            }
        }
        
        return infos;
    }
    
    #endregion

    #region Save File Info
    
    /// <summary>
    /// 저장 파일 정보 구조체
    /// </summary>
    [System.Serializable]
    public class SaveFileInfo
    {
        public int slotIndex;
        public string saveDateTime;
        public int gameDate;
        public string gameVersion;
        public long fileSize;
        
        public string GetFormattedFileSize()
        {
            if (fileSize < 1024) return $"{fileSize} B";
            if (fileSize < 1024 * 1024) return $"{fileSize / 1024f:F1} KB";
            return $"{fileSize / (1024f * 1024f):F1} MB";
        }
    }
    
    #endregion

    #region Individual Save/Load Methods
    // 이 부분은 다음 단계에서 구현할 예정
    // SaveFactionStates, LoadFactionStates 등의 메서드들
    
    private void SaveFactionStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveFactionStates - TODO");
    }
    
    private void SaveBuildingStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveBuildingStates - TODO");
    }
    
    private void SaveResearchStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveResearchStates - TODO");
    }
    
    private void SaveRequestStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveRequestStates - TODO");
    }
    
    private void SaveResourceStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveResourceStates - TODO");
    }
    
    private void SaveGameBalanceState(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveGameBalanceState - TODO");
    }
    
    private void SaveEventStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveEventStates - TODO");
    }
    
    private void SaveTileMapStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] SaveTileMapStates - TODO");
    }
    
    private void LoadFactionStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadFactionStates - TODO");
    }
    
    private void LoadBuildingStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadBuildingStates - TODO");
    }
    
    private void LoadResearchStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadResearchStates - TODO");
    }
    
    private void LoadRequestStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadRequestStates - TODO");
    }
    
    private void LoadResourceStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadResourceStates - TODO");
    }
    
    private void LoadGameBalanceState(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadGameBalanceState - TODO");
    }
    
    private void LoadEventStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadEventStates - TODO");
    }
    
    private void LoadTileMapStates(GameSaveData saveData, GameDataManager gameDataManager)
    {
        // TODO: 구현 예정
        Debug.Log("[SaveLoadManager] LoadTileMapStates - TODO");
    }
    
    #endregion
} 