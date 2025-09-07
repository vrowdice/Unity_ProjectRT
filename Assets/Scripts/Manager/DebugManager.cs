using UnityEngine;

/// <summary>
/// 디버그 및 개발용 기능들을 관리하는 매니저 클래스
/// 리소스 추가, 팩션 우호도 조작 등의 기능을 제공
/// </summary>
public class DebugManager : MonoBehaviour
{
    [Header("Debug Panel Settings")]
    [SerializeField]
    private GameObject m_debugPanelPrefab;
    [SerializeField]
    private Transform m_canvasTransform;
    [SerializeField]
    private KeyCode m_debugToggleKey = KeyCode.F1;

    [Header("Dynamic Panel Creation")]
    [SerializeField]
    private bool m_createPanelDynamically = true;

    private GameManager m_gameManager;
    private MainUIManager m_mainUIManager;
    private GameObject m_currentDebugPanel;
    private DebugPanel m_debugPanelComponent;

    /// <summary>
    /// DebugManager 초기화
    /// </summary>
    /// <param name="gameManager">게임 매니저 참조</param>
    /// <param name="mainUIManager">메인 UI 매니저 참조</param>
    public void Initialize(GameManager gameManager, MainUIManager mainUIManager)
    {
        m_gameManager = gameManager;
        m_mainUIManager = mainUIManager;

        // Canvas Transform이 설정되지 않았다면 자동으로 찾기
        if (m_canvasTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                m_canvasTransform = canvas.transform;
            }
        }

        // 동적 패널 생성이 활성화되어 있다면 프리팹 생성
        if (m_createPanelDynamically)
        {
            CreateDebugPanelFromPrefab();
        }
    }

    private void Update()
    {
        // 디버그 키로 패널 토글
        if (Input.GetKeyDown(m_debugToggleKey))
        {
            ToggleDebugPanel();
        }
    }

    /// <summary>
    /// 프리팹에서 디버그 패널을 동적으로 생성
    /// </summary>
    private void CreateDebugPanelFromPrefab()
    {
        if (m_debugPanelPrefab == null)
        {
            Debug.LogWarning("Debug panel prefab is not assigned. Cannot create dynamic panel.");
            return;
        }

        if (m_canvasTransform == null)
        {
            Debug.LogError("Canvas transform not found. Cannot create debug panel.");
            return;
        }

        // 기존 패널이 있다면 제거
        if (m_currentDebugPanel != null)
        {
            DestroyImmediate(m_currentDebugPanel);
        }

        // 새 패널 생성
        m_currentDebugPanel = Instantiate(m_debugPanelPrefab, m_canvasTransform);
        m_debugPanelComponent = m_currentDebugPanel.GetComponent<DebugPanel>();

        if (m_debugPanelComponent != null)
        {
            // 패널 초기화
            m_debugPanelComponent.Initialize(this);
        }
        else
        {
            Debug.LogError("DebugPanel component not found!");
        }

        // 초기에는 비활성화
        m_currentDebugPanel.SetActive(false);
    }

    /// <summary>
    /// 디버그 패널 토글
    /// </summary>
    public void ToggleDebugPanel()
    {
        if (m_currentDebugPanel == null)
        {
            // 패널이 없다면 동적으로 생성
            CreateDebugPanelFromPrefab();
            if (m_currentDebugPanel == null) return;
        }

        bool isActive = m_currentDebugPanel.activeSelf;
        m_currentDebugPanel.SetActive(!isActive);

        if (!isActive)
        {
            Debug.Log("Debug panel opened - Check available debug features!");
        }
    }

    /// <summary>
    /// 디버그 패널 강제 닫기
    /// </summary>
    public void CloseDebugPanel()
    {
        if (m_currentDebugPanel != null)
        {
            m_currentDebugPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 디버그 패널 강제 열기
    /// </summary>
    public void OpenDebugPanel()
    {
        if (m_currentDebugPanel == null)
        {
            CreateDebugPanelFromPrefab();
        }

        if (m_currentDebugPanel != null)
        {
            m_currentDebugPanel.SetActive(true);
        }
    }

    #region Resource Debug Functions

    /// <summary>
    /// 디버그용: 모든 리소스를 1000씩 추가
    /// </summary>
    public void AddResource1000()
    {
        if (m_mainUIManager == null)
        {
            Debug.LogWarning("MainUIManager is not set.");
            return;
        }

        m_mainUIManager.TryAdd(ResourceType.TYPE.Wood, 1000);
        m_mainUIManager.TryAdd(ResourceType.TYPE.Iron, 1000);
        m_mainUIManager.TryAdd(ResourceType.TYPE.Food, 1000);
        m_mainUIManager.TryAdd(ResourceType.TYPE.Tech, 1000);

        Debug.Log("Added 1000 to all resources");
    }

    /// <summary>
    /// 디버그용: 모든 리소스를 10000씩 추가
    /// </summary>
    public void AddResource10000()
    {
        if (m_mainUIManager == null)
        {
            Debug.LogWarning("MainUIManager is not set.");
            return;
        }

        m_mainUIManager.TryAdd(ResourceType.TYPE.Wood, 10000);
        m_mainUIManager.TryAdd(ResourceType.TYPE.Iron, 10000);
        m_mainUIManager.TryAdd(ResourceType.TYPE.Food, 10000);
        m_mainUIManager.TryAdd(ResourceType.TYPE.Tech, 10000);

        Debug.Log("Added 10000 to all resources");
    }

    /// <summary>
    /// 디버그용: 특정 리소스를 지정한 양만큼 추가
    /// </summary>
    /// <param name="resourceType">리소스 타입</param>
    /// <param name="amount">추가할 양</param>
    public void AddSpecificResource(ResourceType.TYPE resourceType, int amount)
    {
        if (m_mainUIManager == null)
        {
            Debug.LogWarning("MainUIManager is not set.");
            return;
        }

        m_mainUIManager.TryAdd(resourceType, amount);
        Debug.Log($"Added {amount} to {resourceType}");
    }

    #endregion

    #region Faction Debug Functions

    /// <summary>
    /// 디버그용: 모든 팩션의 우호도를 5씩 추가
    /// </summary>
    public void AddAllFactionLike5()
    {
        var gameDataManager = GameDataManager.Instance;
        if (gameDataManager == null)
        {
            Debug.LogWarning("GameDataManager not found.");
            return;
        }

        int addedCount = 0;
        foreach (var factionEntry in gameDataManager.FactionEntryDict)
        {
            // None 팩션은 제외
            if (factionEntry.Key != FactionType.TYPE.None)
            {
                factionEntry.Value.m_state.m_like += 5;
                addedCount++;
                Debug.Log($"{factionEntry.Key} faction like +5 (current: {factionEntry.Value.m_state.m_like})");
            }
        }

        Debug.Log($"Total {addedCount} factions increased like by 5.");
    }

    /// <summary>
    /// 디버그용: 특정 팩션의 우호도를 지정한 양만큼 추가
    /// </summary>
    /// <param name="factionType">팩션 타입</param>
    /// <param name="amount">추가할 우호도</param>
    public void AddSpecificFactionLike(FactionType.TYPE factionType, int amount)
    {
        var gameDataManager = GameDataManager.Instance;
        if (gameDataManager == null)
        {
            Debug.LogWarning("GameDataManager not found.");
            return;
        }

        if (factionType == FactionType.TYPE.None)
        {
            Debug.LogWarning("Cannot manipulate None faction like.");
            return;
        }

        if (gameDataManager.FactionEntryDict.TryGetValue(factionType, out FactionEntry factionEntry))
        {
            factionEntry.m_state.m_like += amount;
            Debug.Log($"{factionType} faction like +{amount} (current: {factionEntry.m_state.m_like})");
        }
        else
        {
            Debug.LogWarning($"Faction {factionType} not found.");
        }
    }

    /// <summary>
    /// 디버그용: 모든 팩션의 우호도를 최대치로 설정
    /// </summary>
    public void MaxAllFactionLike()
    {
        var gameDataManager = GameDataManager.Instance;
        if (gameDataManager == null)
        {
            Debug.LogWarning("GameDataManager not found.");
            return;
        }

        const int maxLike = 100; // 최대 우호도 설정
        int addedCount = 0;

        foreach (var factionEntry in gameDataManager.FactionEntryDict)
        {
            // None 팩션은 제외
            if (factionEntry.Key != FactionType.TYPE.None)
            {
                factionEntry.Value.m_state.m_like = maxLike;
                addedCount++;
                Debug.Log($"Set {factionEntry.Key} faction like to maximum ({maxLike})");
            }
        }

        Debug.Log($"Total {addedCount} factions set to maximum like.");
    }

    #endregion

    #region Game Debug Functions

    /// <summary>
    /// 디버그용: 날짜를 지정한 일수만큼 추가
    /// </summary>
    /// <param name="days">추가할 일수</param>
    public void AddDays(int days)
    {
        if (m_gameManager == null)
        {
            Debug.LogWarning("GameManager is not set.");
            return;
        }

        m_gameManager.AddDate(days);
        
        if (m_mainUIManager != null)
        {
            m_mainUIManager.UpdateAllMainText();
        }

        Debug.Log($"Added {days} days (current: {m_gameManager.Date} days)");
    }

    /// <summary>
    /// 디버그용: 게임 상태 정보 로그 출력
    /// </summary>
    public void LogGameStatus()
    {
        if (m_gameManager == null)
        {
            Debug.LogWarning("GameManager is not set.");
            return;
        }

        Debug.Log("=== Game Status Info ===");
        Debug.Log($"Current Date: {m_gameManager.Date} days");
        Debug.Log($"Wood: {m_gameManager.GetResource(ResourceType.TYPE.Wood)}");
        Debug.Log($"Iron: {m_gameManager.GetResource(ResourceType.TYPE.Iron)}");
        Debug.Log($"Food: {m_gameManager.GetResource(ResourceType.TYPE.Food)}");
        Debug.Log($"Tech: {m_gameManager.GetResource(ResourceType.TYPE.Tech)}");

        var gameDataManager = GameDataManager.Instance;
        if (gameDataManager != null)
        {
            Debug.Log("=== Faction Likes ===");
            foreach (var factionEntry in gameDataManager.FactionEntryDict)
            {
                if (factionEntry.Key != FactionType.TYPE.None)
                {
                    Debug.Log($"{factionEntry.Key}: {factionEntry.Value.m_state.m_like}");
                }
            }
        }
    }

    #endregion

    #region Utility Functions

    /// <summary>
    /// 디버그 패널 재생성 (개발 중 UI 변경 시 유용)
    /// </summary>
    [ContextMenu("Recreate Debug Panel")]
    public void RecreateDebugPanel()
    {
        if (m_currentDebugPanel != null)
        {
            DestroyImmediate(m_currentDebugPanel);
        }
        CreateDebugPanelFromPrefab();
        Debug.Log("Debug panel recreated.");
    }

    /// <summary>
    /// 모든 디버그 정보 초기화 (주의!)
    /// </summary>
    public void ResetAllDebugChanges()
    {
        var gameDataManager = GameDataManager.Instance;
        if (gameDataManager != null)
        {
            foreach (var factionEntry in gameDataManager.FactionEntryDict)
            {
                if (factionEntry.Key != FactionType.TYPE.None)
                {
                    factionEntry.Value.m_state.m_like = 0;
                }
            }
        }

        if (m_gameManager != null)
        {
            // 리소스 초기화는 위험할 수 있으므로 주석 처리
            // m_gameManager.ResetResources();
        }

        Debug.LogWarning("All faction likes reset to 0!");
    }

    #endregion
} 