using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임의 핵심 로직을 관리하는 매니저 클래스
/// 싱글톤 패턴으로 구현되어 전역에서 접근 가능
/// 날짜, 리소스, 토큰, UI 매니저 등을 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField]
    private SceneLoadManager m_scenLoadManager = null;
    [SerializeField]
    private GameDataManager m_gameDataManager = null;
    
    [Header("UI Prefabs")]
    [SerializeField]
    private GameObject m_warningPanelPrefeb = null;
    [SerializeField]
    private GameObject m_confirmDialogPrefab = null;

    // 상수 정의
    private const long INITIAL_RESOURCE_AMOUNT = 100000;
    private const long MIN_RESOURCE_AMOUNT = 0;
    private const float DEFAULT_MULTIPLIER = 1.0f;
    private const float DEFAULT_ADDITION = 0f;

    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }
    public GameDataManager GameDataManager => m_gameDataManager;
    public IUIManager MainUiManager => m_nowUIManager;

    // 게임 상태 데이터
    public int Date { get; private set; }
    public long WealthToken { get; private set; }
    public long ExchangeToken { get; private set; }

    // 현재 활성화된 UI 매니저
    private IUIManager m_nowUIManager = null;

    // 리소스 관리 딕셔너리
    private Dictionary<ResourceType.TYPE, long> m_resourcesDict = new();
    private Dictionary<ResourceType.TYPE, long> m_producedResourcesDict = new();
    private Dictionary<ResourceType.TYPE, long> m_buildingProductionDict = new();
    private Dictionary<ResourceType.TYPE, long> m_territoryProductionDict = new();

    /// <summary>
    /// 싱글톤 패턴 초기화
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임 시작 시 초기 설정
    /// </summary>
    void Start()
    {
        FirstSetting();
    }

    /// <summary>
    /// 게임의 첫 번째 설정
    /// 리소스 초기화, UI 매니저 설정, 건물 생산량 계산
    /// </summary>
    void FirstSetting()
    {
        InitializeResources();
        SetupUIManager();
        GetBuildingDateResource();
    }

    /// <summary>
    /// 모든 리소스를 초기값으로 설정
    /// </summary>
    private void InitializeResources()
    {
        foreach (ResourceType.TYPE resourceType in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            m_resourcesDict[resourceType] = INITIAL_RESOURCE_AMOUNT;
            m_producedResourcesDict[resourceType] = MIN_RESOURCE_AMOUNT;
            m_buildingProductionDict[resourceType] = MIN_RESOURCE_AMOUNT;
            m_territoryProductionDict[resourceType] = MIN_RESOURCE_AMOUNT;
        }
    }

    /// <summary>
    /// UI 매니저 찾기 및 설정
    /// </summary>
    private void SetupUIManager()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return;
        }

        m_nowUIManager = canvas.GetComponent<IUIManager>();
        if (m_nowUIManager == null)
        {
            m_nowUIManager = canvas.GetComponentInChildren<IUIManager>();
        }

        if (m_nowUIManager != null)
        {
            m_nowUIManager.Initialize(this, m_gameDataManager);
        }
        else
        {
            Debug.LogError("No IUIManager found on the Canvas or its children!");
        }
    }

    /// <summary>
    /// 날짜를 추가하고 관련 로직 실행
    /// 리소스 생산, 요청 생성, 이벤트 체크, 연구 진행 등을 수행
    /// </summary>
    /// <param name="argAddDate">추가할 날짜 수</param>
    public void AddDate(int argAddDate)
    {
        if (argAddDate < 0)
        {
            Date = 0;
            Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
            return;
        }

        if (GameDataManager.GameBalanceEntry.m_data.m_maxDate <= Date)
        {
            Date = GameDataManager.GameBalanceEntry.m_data.m_maxDate;
        }

        GetBuildingDateResource();
        GetDayResource(argAddDate);

        Date += argAddDate;

        UpdateGameBalance();
        UpdateResearchProgress(argAddDate);
        CheckAndProcessRequests();
        CheckAndProcessEvents();

        m_gameDataManager.EffectManager.GetAllActiveEffectInfo();
    }

    /// <summary>
    /// 게임 밸런스 업데이트
    /// </summary>
    private void UpdateGameBalance()
    {
        GameBalanceEntry balanceEntry = m_gameDataManager.GameBalanceEntry;
        balanceEntry.m_state.m_dateMul = 1.0f + Mathf.Pow(balanceEntry.m_data.m_dateBalanceMul, Date);
    }

    /// <summary>
    /// 요청 생성 및 강제 연락 요청 체크
    /// </summary>
    private void CheckAndProcessRequests()
    {
        GameBalanceEntry balanceEntry = m_gameDataManager.GameBalanceEntry;
        
        // 요청 생성 조건 체크
        if (Date % balanceEntry.m_data.m_makeRequestDate == 0)
        {
            m_gameDataManager.MakeRandomRequest();
        }

        // 강제 연락 요청 체크
        if (balanceEntry.m_data.m_forcedContactRequestList.Contains(Date))
        {
            m_gameDataManager.ForceContactRequest();
        }
    }

    /// <summary>
    /// 이벤트 발생 체크
    /// </summary>
    private void CheckAndProcessEvents()
    {
        if (m_gameDataManager.EventManager.ProcessEventDate())
        {
            Warning(InfoMessages.EventOccurs);
        }
    }

    /// <summary>
    /// 연구 진행도 업데이트
    /// </summary>
    /// <param name="daysPassed">경과한 일수</param>
    private void UpdateResearchProgress(int daysPassed)
    {
        if (m_gameDataManager?.CommonResearchEntryDict == null) return;

        foreach (var researchEntry in m_gameDataManager.CommonResearchEntryDict.Values)
        {
            if (researchEntry == null || researchEntry.m_data == null) continue;

            // 연구가 진행 중인 경우에만 진행도 증가
            if (researchEntry.m_state.IsInProgress)
            {
                // 진행도 증가
                researchEntry.m_state.m_progress += daysPassed;

                // 연구 완료 체크
                if (researchEntry.m_state.m_progress >= researchEntry.m_data.m_duration)
                {
                    // 연구 완료
                    researchEntry.m_state.m_progress = (int)researchEntry.m_data.m_duration;
                    researchEntry.m_state.m_isResearched = true;
                    
                    // 연구 완료 시 이펙트 활성화
                    researchEntry.CompleteResearch(m_gameDataManager);
                    
                    Debug.Log($"Research completed: {researchEntry.m_data.m_name}");
                }
            }
        }
    }

    /// <summary>
    /// 여러 리소스를 한 번에 변경
    /// 부족한 리소스가 있으면 변경하지 않고 false 반환
    /// </summary>
    /// <param name="argResourceChanges">변경할 리소스와 양</param>
    /// <returns>성공 여부</returns>
    public bool TryChangeAllResources(Dictionary<ResourceType.TYPE, long> argResourceChanges)
    {
        if (argResourceChanges == null)
        {
            Debug.LogError("Resource changes dictionary is null.");
            return false;
        }

        // 먼저 모든 리소스가 충분한지 확인
        if (!ValidateResourceChanges(argResourceChanges))
        {
            return false;
        }

        // 모든 리소스 변경 실행
        foreach (KeyValuePair<ResourceType.TYPE, long> resourceChange in argResourceChanges)
        {
            m_resourcesDict[resourceChange.Key] += resourceChange.Value;
        }

        UpdateUI();
        return true;
    }

    /// <summary>
    /// 리소스 변경 유효성 검사
    /// </summary>
    /// <param name="resourceChanges">변경할 리소스와 양</param>
    /// <returns>유효성 여부</returns>
    private bool ValidateResourceChanges(Dictionary<ResourceType.TYPE, long> resourceChanges)
    {
        foreach (KeyValuePair<ResourceType.TYPE, long> resourceChange in resourceChanges)
        {
            if (!m_resourcesDict.ContainsKey(resourceChange.Key))
            {
                Debug.LogError($"Resource type {resourceChange.Key} not found in resource dictionary.");
                return false;
            }

            if (resourceChange.Value < 0 && m_resourcesDict[resourceChange.Key] + resourceChange.Value < 0)
            {
                Warning($"Not enough {resourceChange.Key} to perform this action. Required: {Mathf.Abs(resourceChange.Value)}, Have: {m_resourcesDict[resourceChange.Key]}");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 단일 리소스를 변경
    /// 부족한 리소스면 변경하지 않고 false 반환
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <param name="argAmount">변경할 양 (음수면 소비)</param>
    /// <returns>성공 여부</returns>
    public bool TryChangeResource(ResourceType.TYPE argType, long argAmount)
    {
        if (!m_resourcesDict.ContainsKey(argType))
        {
            Debug.LogError($"Resource type {argType} not found in resource dictionary.");
            return false;
        }

        if (argAmount < 0 && m_resourcesDict[argType] + argAmount < 0)
        {
            Warning($"Not enough {argType} to perform this action. Required: {Mathf.Abs(argAmount)}, Have: {m_resourcesDict[argType]}");
            return false;
        }

        m_resourcesDict[argType] += argAmount;
        UpdateUI();
        return true;
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (m_nowUIManager != null)
        {
            m_nowUIManager.UpdateAllMainText();
        }
    }

    /// <summary>
    /// 지정된 일수만큼 리소스를 생산
    /// </summary>
    /// <param name="argDay">생산할 일수</param>
    public void GetDayResource(int argDay)
    {
        for (int day = 0; day < argDay; day++)
        {
            foreach (KeyValuePair<ResourceType.TYPE, long> production in m_producedResourcesDict.ToList())
            {
                TryChangeResource(production.Key, production.Value);
            }
        }
    }

    /// <summary>
    /// 모든 건물의 생산량을 계산하여 일일 생산량 업데이트
    /// 이벤트 효과도 적용
    /// </summary>
    public void GetBuildingDateResource()
    {
        ResetProductionResources();
        CalculateBuildingProduction();
        CalculateTerritoryProduction();
    }

    /// <summary>
    /// 생산량 딕셔너리를 모두 0으로 초기화
    /// </summary>
    private void ResetProductionResources()
    {
        foreach (ResourceType.TYPE resourceType in m_producedResourcesDict.Keys.ToList())
        {
            m_producedResourcesDict[resourceType] = MIN_RESOURCE_AMOUNT;
            m_buildingProductionDict[resourceType] = MIN_RESOURCE_AMOUNT;
            m_territoryProductionDict[resourceType] = MIN_RESOURCE_AMOUNT;
        }
    }

    /// <summary>
    /// 모든 건물의 생산량 계산
    /// </summary>
    private void CalculateBuildingProduction()
    {
        foreach (KeyValuePair<string, BuildingEntry> buildingPair in GameDataManager.BuildingEntryDict)
        {
            BuildingEntry building = buildingPair.Value;
            building.ApplyProduction();

            if (building.m_state.m_amount <= 0)
            {
                continue;
            }

            ProcessBuildingProduction(building);
        }
    }

    /// <summary>
    /// 개별 건물의 생산량 처리
    /// </summary>
    /// <param name="building">처리할 건물</param>
    private void ProcessBuildingProduction(BuildingEntry building)
    {
        foreach (var production in building.m_state.m_calculatedProductionList)
        {
            if (!m_producedResourcesDict.ContainsKey(production.m_type))
            {
                Debug.LogWarning($"[GetBuildingDateResource] Unknown resource type: {production.m_type}");
                continue;
            }

            float multiplier = GetResourceMultiplier(production.m_type);
            float addition = GetResourceAddition(production.m_type);

            // 합연산 후 곱연산 적용
            long finalAmount = (long)((production.m_amount + addition) * multiplier);
            m_producedResourcesDict[production.m_type] += finalAmount;
            m_buildingProductionDict[production.m_type] += finalAmount;
        }
    }

    /// <summary>
    /// 리소스 곱연산 값 가져오기
    /// </summary>
    /// <param name="resourceType">리소스 타입</param>
    /// <returns>곱연산 값</returns>
    private float GetResourceMultiplier(ResourceType.TYPE resourceType)
    {
        if (m_gameDataManager.EventManager.EventState.m_buildingResourceModDic.TryGetValue(resourceType, out float modValue))
        {
            return (modValue == 0f) ? DEFAULT_MULTIPLIER : modValue;
        }
        return DEFAULT_MULTIPLIER;
    }

    /// <summary>
    /// 리소스 합연산 값 가져오기
    /// </summary>
    /// <param name="resourceType">리소스 타입</param>
    /// <returns>합연산 값</returns>
    private float GetResourceAddition(ResourceType.TYPE resourceType)
    {
        if (m_gameDataManager.EventManager.EventState.m_buildingResourceAddDic.TryGetValue(resourceType, out float addValue))
        {
            return addValue;
        }
        return DEFAULT_ADDITION;
    }

    /// <summary>
    /// 영토 생산량 계산
    /// </summary>
    private void CalculateTerritoryProduction()
    {
        if (m_gameDataManager.TileMapManager?.TileMap == null)
        {
            return;
        }

        Vector2Int mapSize = m_gameDataManager.TileMapManager.GetTileMapSize();
        
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                TileMapState tileState = m_gameDataManager.TileMapManager.GetTileMapState(x, y);
                if (tileState != null && tileState.m_isFriendlyArea)
                {
                    TileMapData tileData = m_gameDataManager.TileMapManager.GetTileMapData(tileState.m_terrainType);
                    if (tileData != null)
                    {
                        // 기본 영토 생산량 (타일당 1씩)
                        long baseTerritoryProduction = 1;
                        
                        // 지형별 배수 적용
                        float woodMultiplier = tileData.m_woodMul;
                        float ironMultiplier = tileData.m_iromMul;
                        float foodMultiplier = tileData.m_foodMul;
                        float techMultiplier = tileData.m_techMul;
                        
                        // 영토 이펙트 적용
                        float territoryWoodMultiplier = GetTerritoryResourceMultiplier(ResourceType.TYPE.Wood);
                        float territoryIronMultiplier = GetTerritoryResourceMultiplier(ResourceType.TYPE.Iron);
                        float territoryFoodMultiplier = GetTerritoryResourceMultiplier(ResourceType.TYPE.Food);
                        float territoryTechMultiplier = GetTerritoryResourceMultiplier(ResourceType.TYPE.Tech);
                        
                        float territoryWoodAddition = GetTerritoryResourceAddition(ResourceType.TYPE.Wood);
                        float territoryIronAddition = GetTerritoryResourceAddition(ResourceType.TYPE.Iron);
                        float territoryFoodAddition = GetTerritoryResourceAddition(ResourceType.TYPE.Food);
                        float territoryTechAddition = GetTerritoryResourceAddition(ResourceType.TYPE.Tech);
                        
                        // 최종 영토 생산량 계산
                        long woodProduction = (long)((baseTerritoryProduction * woodMultiplier + territoryWoodAddition) * territoryWoodMultiplier);
                        long ironProduction = (long)((baseTerritoryProduction * ironMultiplier + territoryIronAddition) * territoryIronMultiplier);
                        long foodProduction = (long)((baseTerritoryProduction * foodMultiplier + territoryFoodAddition) * territoryFoodMultiplier);
                        long techProduction = (long)((baseTerritoryProduction * techMultiplier + territoryTechAddition) * territoryTechMultiplier);
                        
                        // 영토 생산량에 추가
                        m_territoryProductionDict[ResourceType.TYPE.Wood] += woodProduction;
                        m_territoryProductionDict[ResourceType.TYPE.Iron] += ironProduction;
                        m_territoryProductionDict[ResourceType.TYPE.Food] += foodProduction;
                        m_territoryProductionDict[ResourceType.TYPE.Tech] += techProduction;
                        
                        // 전체 생산량에도 추가
                        m_producedResourcesDict[ResourceType.TYPE.Wood] += woodProduction;
                        m_producedResourcesDict[ResourceType.TYPE.Iron] += ironProduction;
                        m_producedResourcesDict[ResourceType.TYPE.Food] += foodProduction;
                        m_producedResourcesDict[ResourceType.TYPE.Tech] += techProduction;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 영토 리소스 곱연산 값 가져오기
    /// </summary>
    /// <param name="resourceType">리소스 타입</param>
    /// <returns>곱연산 값</returns>
    private float GetTerritoryResourceMultiplier(ResourceType.TYPE resourceType)
    {
        if (m_gameDataManager.EventManager.EventState.m_territoryResourceModDic.TryGetValue(resourceType, out float modValue))
        {
            return (modValue == 0f) ? DEFAULT_MULTIPLIER : modValue;
        }
        return DEFAULT_MULTIPLIER;
    }

    /// <summary>
    /// 영토 리소스 합연산 값 가져오기
    /// </summary>
    /// <param name="resourceType">리소스 타입</param>
    /// <returns>합연산 값</returns>
    private float GetTerritoryResourceAddition(ResourceType.TYPE resourceType)
    {
        if (m_gameDataManager.EventManager.EventState.m_territoryResourceAddDic.TryGetValue(resourceType, out float addValue))
        {
            return addValue;
        }
        return DEFAULT_ADDITION;
    }

    /// <summary>
    /// 씬을 로드
    /// </summary>
    /// <param name="argSceneName">로드할 씬 이름</param>
    public void LoadScene(string argSceneName)
    {
        if (m_scenLoadManager != null)
        {
            m_scenLoadManager.LoadScene(argSceneName);
        }
        else
        {
            Debug.LogError("SceneLoadManager is not assigned!");
        }
    }

    /// <summary>
    /// 특정 리소스의 현재 보유량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>보유량</returns>
    public long GetResource(ResourceType.TYPE argType)
    {
        return m_resourcesDict.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 특정 리소스의 일일 생산량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>일일 생산량</returns>
    public long GetDayAddResource(ResourceType.TYPE argType)
    {
        return m_producedResourcesDict.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 특정 리소스의 건물 생산량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>건물 생산량</returns>
    public long GetBuildingProduction(ResourceType.TYPE argType)
    {
        return m_buildingProductionDict.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 특정 리소스의 영토 생산량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>영토 생산량</returns>
    public long GetTerritoryProduction(ResourceType.TYPE argType)
    {
        return m_territoryProductionDict.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 경고 메시지를 표시
    /// </summary>
    /// <param name="argWarnStr">경고 메시지</param>
    public void Warning(string argWarnStr)
    {
        if (m_warningPanelPrefeb == null || m_nowUIManager?.CanvasTrans == null)
        {
            Debug.LogError("Warning panel prefab or canvas transform is null!");
            return;
        }

        GameObject warningObj = Instantiate(m_warningPanelPrefeb, m_nowUIManager.CanvasTrans);
        Transform textTransform = warningObj.transform.Find("Text");
        
        if (textTransform != null)
        {
            Text textComponent = textTransform.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = argWarnStr;
            }
            else
            {
                Debug.LogError("Text component not found on warning panel!");
            }
        }
        else
        {
            Debug.LogError("Text transform not found on warning panel!");
        }
    }

    /// <summary>
    /// 확인 다이얼로그를 표시
    /// </summary>
    /// <param name="message">다이얼로그 메시지</param>
    /// <param name="onYes">확인 버튼 클릭 시 실행할 액션</param>
    public void ShowConfirmDialog(string message, Action onYes)
    {
        if (m_confirmDialogPrefab == null || m_nowUIManager?.CanvasTrans == null)
        {
            Debug.LogError("Confirm dialog prefab or canvas transform is null!");
            return;
        }

        GameObject dialogObj = Instantiate(m_confirmDialogPrefab, m_nowUIManager.CanvasTrans);
        ConfirmDialogUI dialog = dialogObj.GetComponent<ConfirmDialogUI>();
        
        if (dialog != null)
        {
            dialog.Setup(message, onYes);
        }
        else
        {
            Debug.LogError("ConfirmDialogUI component not found on dialog prefab!");
        }
    }
}