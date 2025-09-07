using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 데이터 로딩을 담당하는 클래스
/// </summary>
public static class DataLoader
{
    /// <summary>
    /// Resources에서 모든 데이터를 로드
    /// </summary>
    public static void LoadAllDataFromResources(
        List<TileMapData> tileMapDataList,
        List<EventGroupData> eventGroupDataList,
        List<FactionData> factionDataList,
        List<BuildingData> buildingDataList,
        List<RequestLineTemplate> requestLineTemplateList,
        List<ResourceIcon> resourceIconList,
        List<TokenIcon> tokenIconList,
        List<RequestIcon> requestIconList,
        ref GameBalanceData gameBalanceData,
        string factionDataPath = "Faction",
        string buildingDataPath = "Building",
        string requestLineTemplatePath = "RequestLineTemplate",
        string eventGroupDataPath = "Event/EventGroup",
        string tileMapDataPath = "TileMap")
    {
        LoadTileMapDataFromResources(tileMapDataList, tileMapDataPath);
        LoadEventGroupDataFromResources(eventGroupDataList, eventGroupDataPath);
        LoadFactionDataFromResources(factionDataList, factionDataPath);
        LoadBuildingDataFromResources(buildingDataList, buildingDataPath);
        LoadRequestLineTemplateDataFromResources(requestLineTemplateList, requestLineTemplatePath);
        // 아이콘 및 GameBalanceData는 GameDataManager에서 수동으로 설정

        Debug.Log("All game data loaded from Resources.");
    }

    private static void LoadTileMapDataFromResources(List<TileMapData> tileMapDataList, string path = "")
    {
        TileMapData[] dataArray = Resources.LoadAll<TileMapData>(path);
        tileMapDataList.Clear();
        tileMapDataList.AddRange(dataArray);
        Debug.Log($"{tileMapDataList.Count} tile map data loaded from Resources path: {path}");
    }

    private static void LoadEventGroupDataFromResources(List<EventGroupData> eventGroupDataList, string path = "")
    {
        EventGroupData[] dataArray = Resources.LoadAll<EventGroupData>(path);
        eventGroupDataList.Clear();
        eventGroupDataList.AddRange(dataArray);
        Debug.Log($"{eventGroupDataList.Count} event group data loaded from Resources path: {path}");
    }

    private static void LoadFactionDataFromResources(List<FactionData> factionDataList, string path = "")
    {
        FactionData[] dataArray = Resources.LoadAll<FactionData>(path);
        factionDataList.Clear();
        factionDataList.AddRange(dataArray);
        Debug.Log($"{factionDataList.Count} faction data loaded from Resources path: {path}");
    }



    private static void LoadBuildingDataFromResources(List<BuildingData> buildingDataList, string path = "")
    {
        BuildingData[] dataArray = Resources.LoadAll<BuildingData>(path);
        buildingDataList.Clear();
        buildingDataList.AddRange(dataArray);
        Debug.Log($"{buildingDataList.Count} building data loaded from Resources path: {path}");
    }

    private static void LoadRequestLineTemplateDataFromResources(List<RequestLineTemplate> requestLineTemplateList, string path = "")
    {
        RequestLineTemplate[] dataArray = Resources.LoadAll<RequestLineTemplate>(path);
        requestLineTemplateList.Clear();
        requestLineTemplateList.AddRange(dataArray);
        Debug.Log($"{requestLineTemplateList.Count} request line templates loaded from Resources path: {path}");
    }



    #if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 모든 데이터를 자동으로 로드
    /// </summary>
    public static void LoadAllDataFromAssets(
        List<TileMapData> tileMapDataList,
        List<EventGroupData> eventGroupDataList,
        List<FactionData> factionDataList,
        List<BuildingData> buildingDataList,
        List<RequestLineTemplate> requestLineTemplateList,
        List<ResourceIcon> resourceIconList,
        List<TokenIcon> tokenIconList,
        List<RequestIcon> requestIconList,
        ref GameBalanceData gameBalanceData)
    {
        LoadTileMapDataFromAssets(tileMapDataList);
        LoadEventGroupDataFromAssets(eventGroupDataList);
        LoadFactionDataFromAssets(factionDataList);
        LoadBuildingDataFromAssets(buildingDataList);
        LoadRequestLineTemplateDataFromAssets(requestLineTemplateList);
        LoadResourceIconsFromAssets(resourceIconList);
        LoadTokenIconsFromAssets(tokenIconList);
        LoadRequestIconsFromAssets(requestIconList);
        LoadGameBalanceDataFromAssets(ref gameBalanceData);

        Debug.Log("All game data has been loaded automatically.");
    }

    /// <summary>
    /// 지정된 경로에서 모든 데이터를 로드
    /// </summary>
    public static void LoadAllDataFromPaths(
        List<TileMapData> tileMapDataList,
        List<EventGroupData> eventGroupDataList,
        List<FactionData> factionDataList,
        List<BuildingData> buildingDataList,
        List<RequestLineTemplate> requestLineTemplateList,
        List<ResourceIcon> resourceIconList,
        List<TokenIcon> tokenIconList,
        List<RequestIcon> requestIconList,
        ref GameBalanceData gameBalanceData,
        string factionDataPath,
        string buildingDataPath,
        string requestLineTemplatePath,
        string eventGroupDataPath,
        string tileMapDataPath)
    {
        LoadTileMapDataFromPath(tileMapDataList, tileMapDataPath);
        LoadEventGroupDataFromPath(eventGroupDataList, eventGroupDataPath);
        LoadFactionDataFromPath(factionDataList, factionDataPath);
        LoadBuildingDataFromPath(buildingDataList, buildingDataPath);
        LoadRequestLineTemplateDataFromPath(requestLineTemplateList, requestLineTemplatePath);
        // 아이콘 및 GameBalanceData는 GameDataManager에서 수동으로 설정

        Debug.Log("All game data has been loaded from specified paths.");
    }

    private static void LoadTileMapDataFromAssets(List<TileMapData> tileMapDataList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TileMapData");
        tileMapDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TileMapData data = UnityEditor.AssetDatabase.LoadAssetAtPath<TileMapData>(path);
            if (data != null)
            {
                tileMapDataList.Add(data);
            }
        }
        
        Debug.Log($"{tileMapDataList.Count} tile map data loaded.");
    }

    private static void LoadEventGroupDataFromAssets(List<EventGroupData> eventGroupDataList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EventGroupData");
        eventGroupDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EventGroupData data = UnityEditor.AssetDatabase.LoadAssetAtPath<EventGroupData>(path);
            if (data != null)
            {
                eventGroupDataList.Add(data);
            }
        }
        
        Debug.Log($"{eventGroupDataList.Count} event group data loaded.");
    }

    private static void LoadFactionDataFromAssets(List<FactionData> factionDataList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FactionData");
        factionDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            FactionData data = UnityEditor.AssetDatabase.LoadAssetAtPath<FactionData>(path);
            if (data != null)
            {
                factionDataList.Add(data);
            }
        }
        
        Debug.Log($"{factionDataList.Count} faction data loaded.");
    }



    private static void LoadBuildingDataFromAssets(List<BuildingData> buildingDataList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BuildingData");
        buildingDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            BuildingData data = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>(path);
            if (data != null)
            {
                buildingDataList.Add(data);
            }
        }
        
        Debug.Log($"{buildingDataList.Count} building data loaded.");
    }

    private static void LoadRequestLineTemplateDataFromAssets(List<RequestLineTemplate> requestLineTemplateList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RequestLineTemplate");
        requestLineTemplateList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            RequestLineTemplate data = UnityEditor.AssetDatabase.LoadAssetAtPath<RequestLineTemplate>(path);
            if (data != null)
            {
                requestLineTemplateList.Add(data);
            }
        }
        
        Debug.Log($"{requestLineTemplateList.Count} request line templates loaded.");
    }

    private static void LoadResourceIconsFromAssets(List<ResourceIcon> resourceIconList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite");
        resourceIconList.Clear();
        
        var resourceTypes = System.Enum.GetValues(typeof(ResourceType.TYPE));
        
        foreach (ResourceType.TYPE resourceType in resourceTypes)
        {
            string resourceName = resourceType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(resourceName) || 
                     sprite.name.ToLower().Contains("resource") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    ResourceIcon icon = new ResourceIcon
                    {
                        m_type = resourceType,
                        m_icon = sprite
                    };
                    resourceIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{resourceIconList.Count} resource icons auto-setup completed.");
    }

    private static void LoadTokenIconsFromAssets(List<TokenIcon> tokenIconList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite");
        tokenIconList.Clear();
        
        var tokenTypes = System.Enum.GetValues(typeof(TokenType.TYPE));
        
        foreach (TokenType.TYPE tokenType in tokenTypes)
        {
            string tokenName = tokenType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(tokenName) || 
                     sprite.name.ToLower().Contains("token") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    TokenIcon icon = new TokenIcon
                    {
                        m_type = tokenType,
                        m_icon = sprite
                    };
                    tokenIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{tokenIconList.Count} token icons auto-setup completed.");
    }

    private static void LoadRequestIconsFromAssets(List<RequestIcon> requestIconList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite");
        requestIconList.Clear();
        
        var requestTypes = System.Enum.GetValues(typeof(RequestType.TYPE));
        
        foreach (RequestType.TYPE requestType in requestTypes)
        {
            string requestName = requestType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(requestName) || 
                     sprite.name.ToLower().Contains("request") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    RequestIcon icon = new RequestIcon
                    {
                        m_type = requestType,
                        m_icon = sprite
                    };
                    requestIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{requestIconList.Count} request icons auto-setup completed.");
    }

    private static void LoadGameBalanceDataFromAssets(ref GameBalanceData gameBalanceData)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameBalanceData");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameBalanceData data = UnityEditor.AssetDatabase.LoadAssetAtPath<GameBalanceData>(path);
            if (data != null)
            {
                gameBalanceData = data;
                Debug.Log("Game balance data loaded.");
                return;
            }
        }
        
        Debug.LogWarning("Game balance data not found.");
    }

    #region Path-based Loading Methods
    private static void LoadTileMapDataFromPath(List<TileMapData> tileMapDataList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TileMapData", new[] { path });
        tileMapDataList.Clear();
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TileMapData data = UnityEditor.AssetDatabase.LoadAssetAtPath<TileMapData>(assetPath);
            if (data != null)
            {
                tileMapDataList.Add(data);
            }
        }
        
        Debug.Log($"{tileMapDataList.Count} tile map data loaded from path: {path}");
    }

    private static void LoadEventGroupDataFromPath(List<EventGroupData> eventGroupDataList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EventGroupData", new[] { path });
        eventGroupDataList.Clear();
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EventGroupData data = UnityEditor.AssetDatabase.LoadAssetAtPath<EventGroupData>(assetPath);
            if (data != null)
            {
                eventGroupDataList.Add(data);
            }
        }
        
        Debug.Log($"{eventGroupDataList.Count} event group data loaded from path: {path}");
    }

    private static void LoadFactionDataFromPath(List<FactionData> factionDataList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FactionData", new[] { path });
        factionDataList.Clear();
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            FactionData data = UnityEditor.AssetDatabase.LoadAssetAtPath<FactionData>(assetPath);
            if (data != null)
            {
                factionDataList.Add(data);
            }
        }
        
        Debug.Log($"{factionDataList.Count} faction data loaded from path: {path}");
    }



    private static void LoadBuildingDataFromPath(List<BuildingData> buildingDataList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BuildingData", new[] { path });
        buildingDataList.Clear();
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            BuildingData data = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>(assetPath);
            if (data != null)
            {
                buildingDataList.Add(data);
            }
        }
        
        Debug.Log($"{buildingDataList.Count} building data loaded from path: {path}");
    }

    private static void LoadRequestLineTemplateDataFromPath(List<RequestLineTemplate> requestLineTemplateList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RequestLineTemplate", new[] { path });
        requestLineTemplateList.Clear();
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            RequestLineTemplate data = UnityEditor.AssetDatabase.LoadAssetAtPath<RequestLineTemplate>(assetPath);
            if (data != null)
            {
                requestLineTemplateList.Add(data);
            }
        }
        
        Debug.Log($"{requestLineTemplateList.Count} request line templates loaded from path: {path}");
    }

    private static void LoadResourceIconsFromPath(List<ResourceIcon> resourceIconList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { path });
        resourceIconList.Clear();
        
        var resourceTypes = System.Enum.GetValues(typeof(ResourceType.TYPE));
        
        foreach (ResourceType.TYPE resourceType in resourceTypes)
        {
            string resourceName = resourceType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(resourceName) || 
                     sprite.name.ToLower().Contains("resource") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    ResourceIcon icon = new ResourceIcon
                    {
                        m_type = resourceType,
                        m_icon = sprite
                    };
                    resourceIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{resourceIconList.Count} resource icons loaded from path: {path}");
    }

    private static void LoadTokenIconsFromPath(List<TokenIcon> tokenIconList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { path });
        tokenIconList.Clear();
        
        var tokenTypes = System.Enum.GetValues(typeof(TokenType.TYPE));
        
        foreach (TokenType.TYPE tokenType in tokenTypes)
        {
            string tokenName = tokenType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(tokenName) || 
                     sprite.name.ToLower().Contains("token") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    TokenIcon icon = new TokenIcon
                    {
                        m_type = tokenType,
                        m_icon = sprite
                    };
                    tokenIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{tokenIconList.Count} token icons loaded from path: {path}");
    }

    private static void LoadRequestIconsFromPath(List<RequestIcon> requestIconList, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { path });
        requestIconList.Clear();
        
        var requestTypes = System.Enum.GetValues(typeof(RequestType.TYPE));
        
        foreach (RequestType.TYPE requestType in requestTypes)
        {
            string requestName = requestType.ToString().ToLower();
            
            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                
                if (sprite != null && 
                    (sprite.name.ToLower().Contains(requestName) || 
                     sprite.name.ToLower().Contains("request") ||
                     sprite.name.ToLower().Contains("icon")))
                {
                    RequestIcon icon = new RequestIcon
                    {
                        m_type = requestType,
                        m_icon = sprite
                    };
                    requestIconList.Add(icon);
                    break;
                }
            }
        }
        
        Debug.Log($"{requestIconList.Count} request icons loaded from path: {path}");
    }

    private static void LoadGameBalanceDataFromPath(ref GameBalanceData gameBalanceData, string path)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameBalanceData", new[] { path });
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameBalanceData data = UnityEditor.AssetDatabase.LoadAssetAtPath<GameBalanceData>(assetPath);
            if (data != null)
            {
                gameBalanceData = data;
                Debug.Log($"Game balance data loaded from path: {path}");
                return;
            }
        }
        
        Debug.LogWarning($"Game balance data not found in path: {path}");
    }
    #endregion
    #endif
} 