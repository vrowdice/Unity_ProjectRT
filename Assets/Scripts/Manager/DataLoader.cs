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
        List<EventGroupData> eventGroupDataList,
        List<FactionData> factionDataList,
        List<ResearchData> commonResearchDataList,
        List<BuildingData> buildingDataList,
        List<RequestLineTemplate> requestLineTemplateList,
        List<ResourceIcon> resourceIconList,
        List<TokenIcon> tokenIconList,
        List<RequestIcon> requestIconList,
        ref GameBalanceData gameBalanceData)
    {
        LoadEventGroupDataFromResources(eventGroupDataList);
        LoadFactionDataFromResources(factionDataList);
        LoadCommonResearchDataFromResources(commonResearchDataList);
        LoadBuildingDataFromResources(buildingDataList);
        LoadRequestLineTemplateDataFromResources(requestLineTemplateList);
        LoadResourceIconsFromResources(resourceIconList);
        LoadTokenIconsFromResources(tokenIconList);
        LoadRequestIconsFromResources(requestIconList);
        LoadGameBalanceDataFromResources(ref gameBalanceData);

        Debug.Log("All game data loaded from Resources.");
    }

    private static void LoadEventGroupDataFromResources(List<EventGroupData> eventGroupDataList)
    {
        EventGroupData[] dataArray = Resources.LoadAll<EventGroupData>("");
        eventGroupDataList.Clear();
        eventGroupDataList.AddRange(dataArray);
        Debug.Log($"{eventGroupDataList.Count} event group data loaded from Resources.");
    }

    private static void LoadFactionDataFromResources(List<FactionData> factionDataList)
    {
        FactionData[] dataArray = Resources.LoadAll<FactionData>("");
        factionDataList.Clear();
        factionDataList.AddRange(dataArray);
        Debug.Log($"{factionDataList.Count} faction data loaded from Resources.");
    }

    private static void LoadCommonResearchDataFromResources(List<ResearchData> commonResearchDataList)
    {
        ResearchData[] dataArray = Resources.LoadAll<ResearchData>("");
        commonResearchDataList.Clear();
        
        foreach (ResearchData data in dataArray)
        {
            if (data.m_factionType == FactionType.TYPE.None)
            {
                commonResearchDataList.Add(data);
            }
        }
        
        Debug.Log($"{commonResearchDataList.Count} common research data loaded from Resources.");
    }

    private static void LoadBuildingDataFromResources(List<BuildingData> buildingDataList)
    {
        BuildingData[] dataArray = Resources.LoadAll<BuildingData>("");
        buildingDataList.Clear();
        buildingDataList.AddRange(dataArray);
        Debug.Log($"{buildingDataList.Count} building data loaded from Resources.");
    }

    private static void LoadRequestLineTemplateDataFromResources(List<RequestLineTemplate> requestLineTemplateList)
    {
        RequestLineTemplate[] dataArray = Resources.LoadAll<RequestLineTemplate>("");
        requestLineTemplateList.Clear();
        requestLineTemplateList.AddRange(dataArray);
        Debug.Log($"{requestLineTemplateList.Count} request line templates loaded from Resources.");
    }

    private static void LoadResourceIconsFromResources(List<ResourceIcon> resourceIconList)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("");
        resourceIconList.Clear();
        
        var resourceTypes = System.Enum.GetValues(typeof(ResourceType.TYPE));
        
        foreach (ResourceType.TYPE resourceType in resourceTypes)
        {
            string resourceName = resourceType.ToString().ToLower();
            
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name.ToLower().Contains(resourceName) || 
                    sprite.name.ToLower().Contains("resource") ||
                    sprite.name.ToLower().Contains("icon"))
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
        
        Debug.Log($"{resourceIconList.Count} resource icons loaded from Resources.");
    }

    private static void LoadTokenIconsFromResources(List<TokenIcon> tokenIconList)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("");
        tokenIconList.Clear();
        
        var tokenTypes = System.Enum.GetValues(typeof(TokenType.TYPE));
        
        foreach (TokenType.TYPE tokenType in tokenTypes)
        {
            string tokenName = tokenType.ToString().ToLower();
            
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name.ToLower().Contains(tokenName) || 
                    sprite.name.ToLower().Contains("token") ||
                    sprite.name.ToLower().Contains("icon"))
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
        
        Debug.Log($"{tokenIconList.Count} token icons loaded from Resources.");
    }

    private static void LoadRequestIconsFromResources(List<RequestIcon> requestIconList)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("");
        requestIconList.Clear();
        
        var requestTypes = System.Enum.GetValues(typeof(RequestType.TYPE));
        
        foreach (RequestType.TYPE requestType in requestTypes)
        {
            string requestName = requestType.ToString().ToLower();
            
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name.ToLower().Contains(requestName) || 
                    sprite.name.ToLower().Contains("request") ||
                    sprite.name.ToLower().Contains("icon"))
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
        
        Debug.Log($"{requestIconList.Count} request icons loaded from Resources.");
    }

    private static void LoadGameBalanceDataFromResources(ref GameBalanceData gameBalanceData)
    {
        GameBalanceData[] dataArray = Resources.LoadAll<GameBalanceData>("");
        if (dataArray.Length > 0)
        {
            gameBalanceData = dataArray[0];
            Debug.Log("Game balance data loaded from Resources.");
        }
        else
        {
            Debug.LogWarning("Game balance data not found in Resources.");
        }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 모든 데이터를 자동으로 로드
    /// </summary>
    public static void LoadAllDataFromAssets(
        List<EventGroupData> eventGroupDataList,
        List<FactionData> factionDataList,
        List<ResearchData> commonResearchDataList,
        List<BuildingData> buildingDataList,
        List<RequestLineTemplate> requestLineTemplateList,
        List<ResourceIcon> resourceIconList,
        List<TokenIcon> tokenIconList,
        List<RequestIcon> requestIconList,
        ref GameBalanceData gameBalanceData)
    {
        LoadEventGroupDataFromAssets(eventGroupDataList);
        LoadFactionDataFromAssets(factionDataList);
        LoadCommonResearchDataFromAssets(commonResearchDataList);
        LoadBuildingDataFromAssets(buildingDataList);
        LoadRequestLineTemplateDataFromAssets(requestLineTemplateList);
        LoadResourceIconsFromAssets(resourceIconList);
        LoadTokenIconsFromAssets(tokenIconList);
        LoadRequestIconsFromAssets(requestIconList);
        LoadGameBalanceDataFromAssets(ref gameBalanceData);

        Debug.Log("All game data has been loaded automatically.");
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

    private static void LoadCommonResearchDataFromAssets(List<ResearchData> commonResearchDataList)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ResearchData");
        commonResearchDataList.Clear();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ResearchData data = UnityEditor.AssetDatabase.LoadAssetAtPath<ResearchData>(path);
            if (data != null && data.m_factionType == FactionType.TYPE.None)
            {
                commonResearchDataList.Add(data);
            }
        }
        
        Debug.Log($"{commonResearchDataList.Count} common research data loaded.");
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
    #endif
} 