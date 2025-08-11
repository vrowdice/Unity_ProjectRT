using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// GameDataManager의 커스텀 에디터
/// 인스펙터에서 자동으로 데이터를 로드할 수 있는 버튼들을 제공
/// </summary>
[CustomEditor(typeof(GameDataManager))]
public class GameDataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameDataManager gameDataManager = (GameDataManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Auto Data Loading", EditorStyles.boldLabel);

        // 게임 데이터 자동 로딩 버튼들
        if (GUILayout.Button("Load All Game Data"))
        {
            LoadAllGameData(gameDataManager);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Individual Data Loading", EditorStyles.boldLabel);

        if (GUILayout.Button("Load Tile Map Data"))
        {
            LoadTileMapData(gameDataManager);
        }

        if (GUILayout.Button("Load Event Group Data"))
        {
            LoadEventGroupData(gameDataManager);
        }

        if (GUILayout.Button("Load Faction Data"))
        {
            LoadFactionData(gameDataManager);
        }

        if (GUILayout.Button("Load Common Research Data"))
        {
            LoadCommonResearchData(gameDataManager);
        }

        if (GUILayout.Button("Load Building Data"))
        {
            LoadBuildingData(gameDataManager);
        }

        if (GUILayout.Button("Load Request Line Template"))
        {
            LoadRequestLineTemplateData(gameDataManager);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Icon Data Loading", EditorStyles.boldLabel);

        if (GUILayout.Button("Auto Setup Resource Icons"))
        {
            LoadResourceIcons(gameDataManager);
        }

        if (GUILayout.Button("Auto Setup Token Icons"))
        {
            LoadTokenIcons(gameDataManager);
        }

        if (GUILayout.Button("Auto Setup Request Icons"))
        {
            LoadRequestIcons(gameDataManager);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Game Balance Data", EditorStyles.boldLabel);

        if (GUILayout.Button("Load Game Balance Data"))
        {
            LoadGameBalanceData(gameDataManager);
        }
    }

    /// <summary>
    /// 모든 게임 데이터를 한 번에 로딩
    /// </summary>
    private void LoadAllGameData(GameDataManager gameDataManager)
    {
        LoadTileMapData(gameDataManager);
        LoadEventGroupData(gameDataManager);
        LoadFactionData(gameDataManager);
        LoadCommonResearchData(gameDataManager);
        LoadBuildingData(gameDataManager);
        LoadRequestLineTemplateData(gameDataManager);
        LoadResourceIcons(gameDataManager);
        LoadTokenIcons(gameDataManager);
        LoadRequestIcons(gameDataManager);
        LoadGameBalanceData(gameDataManager);

        EditorUtility.SetDirty(gameDataManager);
        Debug.Log("All game data has been loaded automatically.");
    }

    /// <summary>
    /// 타일맵 데이터 자동 로딩
    /// </summary>
    private void LoadTileMapData(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:TileMapData");
        List<TileMapData> tileMapDataList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<TileMapData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data => data != null)
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty tileMapDataProperty = serializedObject.FindProperty("m_tileMapDataList");
        
        tileMapDataProperty.ClearArray();
        tileMapDataProperty.arraySize = tileMapDataList.Count;
        
        for (int i = 0; i < tileMapDataList.Count; i++)
        {
            tileMapDataProperty.GetArrayElementAtIndex(i).objectReferenceValue = tileMapDataList[i];
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{tileMapDataList.Count} tile map data loaded.");
    }

    /// <summary>
    /// 이벤트 그룹 데이터 자동 로딩
    /// </summary>
    private void LoadEventGroupData(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:EventGroupData");
        List<EventGroupData> eventGroupDataList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<EventGroupData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data => data != null)
            .ToList();

        // SerializedObject를 통해 private 필드에 접근
        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty eventGroupDataProperty = serializedObject.FindProperty("m_eventGroupDataList");
        
        eventGroupDataProperty.ClearArray();
        eventGroupDataProperty.arraySize = eventGroupDataList.Count;
        
        for (int i = 0; i < eventGroupDataList.Count; i++)
        {
            eventGroupDataProperty.GetArrayElementAtIndex(i).objectReferenceValue = eventGroupDataList[i];
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{eventGroupDataList.Count} event group data loaded.");
    }

    /// <summary>
    /// 팩션 데이터 자동 로딩
    /// </summary>
    private void LoadFactionData(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:FactionData");
        List<FactionData> factionDataList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<FactionData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data => data != null)
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty factionDataProperty = serializedObject.FindProperty("m_factionDataList");
        
        factionDataProperty.ClearArray();
        factionDataProperty.arraySize = factionDataList.Count;
        
        for (int i = 0; i < factionDataList.Count; i++)
        {
            factionDataProperty.GetArrayElementAtIndex(i).objectReferenceValue = factionDataList[i];
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{factionDataList.Count} faction data loaded.");
    }

    /// <summary>
    /// 공통 연구 데이터 자동 로딩
    /// </summary>
    private void LoadCommonResearchData(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:ResearchData");
        List<ResearchData> researchDataList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ResearchData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data => data != null && data.m_factionType == FactionType.TYPE.None) // 공통 연구만
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty researchDataProperty = serializedObject.FindProperty("m_commonResearchDataList");
        
        researchDataProperty.ClearArray();
        researchDataProperty.arraySize = researchDataList.Count;
        
        for (int i = 0; i < researchDataList.Count; i++)
        {
            researchDataProperty.GetArrayElementAtIndex(i).objectReferenceValue = researchDataList[i];
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{researchDataList.Count} common research data loaded.");
    }

    /// <summary>
    /// 건물 데이터 자동 로딩
    /// </summary>
    private void LoadBuildingData(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:BuildingData");
        List<BuildingData> buildingDataList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<BuildingData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data => data != null)
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty buildingDataProperty = serializedObject.FindProperty("m_buildingDataList");
        
        buildingDataProperty.ClearArray();
        buildingDataProperty.arraySize = buildingDataList.Count;
        
        for (int i = 0; i < buildingDataList.Count; i++)
        {
            buildingDataProperty.GetArrayElementAtIndex(i).objectReferenceValue = buildingDataList[i];
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{buildingDataList.Count} building data loaded.");
    }

    /// <summary>
    /// 요청 라인 템플릿 데이터 자동 로딩
    /// </summary>
    private void LoadRequestLineTemplateData(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:RequestLineTemplate");
        List<RequestLineTemplate> requestLineTemplateList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<RequestLineTemplate>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data => data != null)
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty requestLineTemplateProperty = serializedObject.FindProperty("m_requestLineTemplateList");
        
        requestLineTemplateProperty.ClearArray();
        requestLineTemplateProperty.arraySize = requestLineTemplateList.Count;
        
        for (int i = 0; i < requestLineTemplateList.Count; i++)
        {
            requestLineTemplateProperty.GetArrayElementAtIndex(i).objectReferenceValue = requestLineTemplateList[i];
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{requestLineTemplateList.Count} request line templates loaded.");
    }

    /// <summary>
    /// 리소스 아이콘 자동 설정
    /// </summary>
    private void LoadResourceIcons(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite");
        List<Sprite> sprites = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(sprite => sprite != null)
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty resourceIconProperty = serializedObject.FindProperty("m_resourceIconList");
        
        resourceIconProperty.ClearArray();
        
        // ResourceType enum의 모든 값에 대해 아이콘 찾기
        var resourceTypes = System.Enum.GetValues(typeof(ResourceType.TYPE));
        int index = 0;
        
        foreach (ResourceType.TYPE resourceType in resourceTypes)
        {
            // 리소스 타입 이름과 일치하는 스프라이트 찾기
            string resourceName = resourceType.ToString().ToLower();
            Sprite matchingSprite = sprites.FirstOrDefault(s => 
                s.name.ToLower().Contains(resourceName) || 
                s.name.ToLower().Contains("resource") ||
                s.name.ToLower().Contains("icon"));

            if (matchingSprite != null)
            {
                resourceIconProperty.arraySize = index + 1;
                SerializedProperty element = resourceIconProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("m_type").enumValueIndex = (int)resourceType;
                element.FindPropertyRelative("m_icon").objectReferenceValue = matchingSprite;
                index++;
            }
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{index} resource icons auto-setup completed.");
    }

    /// <summary>
    /// 토큰 아이콘 자동 설정
    /// </summary>
    private void LoadTokenIcons(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite");
        List<Sprite> sprites = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(sprite => sprite != null)
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty tokenIconProperty = serializedObject.FindProperty("m_tokenIconList");
        
        tokenIconProperty.ClearArray();
        
        var tokenTypes = System.Enum.GetValues(typeof(TokenType.TYPE));
        int index = 0;
        
        foreach (TokenType.TYPE tokenType in tokenTypes)
        {
            string tokenName = tokenType.ToString().ToLower();
            Sprite matchingSprite = sprites.FirstOrDefault(s => 
                s.name.ToLower().Contains(tokenName) || 
                s.name.ToLower().Contains("token") ||
                s.name.ToLower().Contains("icon"));

            if (matchingSprite != null)
            {
                tokenIconProperty.arraySize = index + 1;
                SerializedProperty element = tokenIconProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("m_type").enumValueIndex = (int)tokenType;
                element.FindPropertyRelative("m_icon").objectReferenceValue = matchingSprite;
                index++;
            }
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{index} token icons auto-setup completed.");
    }

    /// <summary>
    /// 요청 아이콘 자동 설정
    /// </summary>
    private void LoadRequestIcons(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite");
        List<Sprite> sprites = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(sprite => sprite != null)
            .ToList();

        SerializedObject serializedObject = new SerializedObject(gameDataManager);
        SerializedProperty requestIconProperty = serializedObject.FindProperty("m_requestIconList");
        
        requestIconProperty.ClearArray();
        
        var requestTypes = System.Enum.GetValues(typeof(RequestType.TYPE));
        int index = 0;
        
        foreach (RequestType.TYPE requestType in requestTypes)
        {
            string requestName = requestType.ToString().ToLower();
            Sprite matchingSprite = sprites.FirstOrDefault(s => 
                s.name.ToLower().Contains(requestName) || 
                s.name.ToLower().Contains("request") ||
                s.name.ToLower().Contains("icon"));

            if (matchingSprite != null)
            {
                requestIconProperty.arraySize = index + 1;
                SerializedProperty element = requestIconProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("m_type").enumValueIndex = (int)requestType;
                element.FindPropertyRelative("m_icon").objectReferenceValue = matchingSprite;
                index++;
            }
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"{index} request icons auto-setup completed.");
    }

    /// <summary>
    /// 게임 밸런스 데이터 로딩
    /// </summary>
    private void LoadGameBalanceData(GameDataManager gameDataManager)
    {
        string[] guids = AssetDatabase.FindAssets("t:GameBalanceData");
        GameBalanceData gameBalanceData = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameBalanceData>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(data => data != null);

        if (gameBalanceData != null)
        {
            SerializedObject serializedObject = new SerializedObject(gameDataManager);
            SerializedProperty gameBalanceProperty = serializedObject.FindProperty("m_gameBalanceData");
            gameBalanceProperty.objectReferenceValue = gameBalanceData;
            serializedObject.ApplyModifiedProperties();
            
            Debug.Log("Game balance data loaded.");
        }
        else
        {
            Debug.LogWarning("Game balance data not found.");
        }
    }
} 