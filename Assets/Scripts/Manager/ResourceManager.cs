using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자원 관리를 전담하는 매니저 클래스
/// 자원 보유량, 생산량, 소비 등을 관리
/// </summary>
public class ResourceManager : MonoBehaviour
{
    // 상수 정의
    private const long INITIAL_RESOURCE_AMOUNT = 100000;
    private const long MIN_RESOURCE_AMOUNT = 0;
    private const float DEFAULT_MULTIPLIER = 1.0f;
    private const float DEFAULT_ADDITION = 0f;

    // GameDataManager 참조
    private GameDataManager m_gameDataManager;

    // 리소스 관리 딕셔너리
    private Dictionary<ResourceType.TYPE, long> m_resourcesDict = new();
    private Dictionary<ResourceType.TYPE, long> m_producedResourcesDict = new();
    private Dictionary<ResourceType.TYPE, long> m_buildingProductionDict = new();
    private Dictionary<ResourceType.TYPE, long> m_territoryProductionDict = new();

    // 토큰 관리
    public long WealthToken { get; private set; }
    public long ExchangeToken { get; private set; }

    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="gameDataManager">GameDataManager 참조</param>
    public void Initialize(GameDataManager gameDataManager)
    {
        m_gameDataManager = gameDataManager;
        InitializeResources();
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
                string warningMessage = $"Not enough {resourceChange.Key} to perform this action. Required: {Mathf.Abs(resourceChange.Value)}, Have: {m_resourcesDict[resourceChange.Key]}";
                
                // GameManager를 통해 경고 메시지 표시
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Warning(warningMessage);
                }
                else
                {
                    Debug.LogWarning(warningMessage);
                }
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
            string warningMessage = $"Not enough {argType} to perform this action. Required: {Mathf.Abs(argAmount)}, Have: {m_resourcesDict[argType]}";
            
            // GameManager를 통해 경고 메시지 표시
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Warning(warningMessage);
            }
            else
            {
                Debug.LogWarning(warningMessage);
            }
            return false;
        }

        m_resourcesDict[argType] += argAmount;
        return true;
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
        foreach (KeyValuePair<string, BuildingEntry> buildingPair in m_gameDataManager.BuildingEntryDict)
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
                        // 타일 상태의 기본 자원값 사용 (지형 배수는 MapDataGenerator에서 반영됨)
                        long baseWood = tileState.m_resources?.GetAmount(ResourceType.TYPE.Wood) ?? 0;
                        long baseIron = tileState.m_resources?.GetAmount(ResourceType.TYPE.Iron) ?? 0;
                        long baseFood = tileState.m_resources?.GetAmount(ResourceType.TYPE.Food) ?? 0;
                        long baseTech = tileState.m_resources?.GetAmount(ResourceType.TYPE.Tech) ?? 0;
                        
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
                        long woodProduction = (long)((baseWood + territoryWoodAddition) * territoryWoodMultiplier);
                        long ironProduction = (long)((baseIron + territoryIronAddition) * territoryIronMultiplier);
                        long foodProduction = (long)((baseFood + territoryFoodAddition) * territoryFoodMultiplier);
                        long techProduction = (long)((baseTech + territoryTechAddition) * territoryTechMultiplier);
                        
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
    /// 토큰 변경
    /// </summary>
    /// <param name="wealthChange">부 토큰 변경량</param>
    /// <param name="exchangeChange">교환 토큰 변경량</param>
    public void ChangeTokens(long wealthChange, long exchangeChange)
    {
        WealthToken = Math.Max(0, WealthToken + wealthChange);
        ExchangeToken = Math.Max(0, ExchangeToken + exchangeChange);
    }
} 