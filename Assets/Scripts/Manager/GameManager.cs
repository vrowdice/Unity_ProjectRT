using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameDataManager GameDataManager => m_gameDataManager;

    public long Date { get; private set; }

    [SerializeField]
    private SceneLoadManager m_scenLoadManager = null;
    [SerializeField]
    private GameDataManager m_gameDataManager = null;
    [SerializeField]
    private GameObject m_warningPanelPrefeb = null;
    [SerializeField]
    private GameObject m_confirmDialogPrefab = null;

    private IUIManager m_nowUIManager = null;

    private Dictionary<ResourceType, long> m_resourcesDict = new();
    private Dictionary<ResourceType, long> m_producedResourcesDict = new();

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

    void Start()
    {
        FirstSetting();
    }

    void FirstSetting()
    {
        foreach (ResourceType argType in System.Enum.GetValues(typeof(ResourceType)))
        {
            //릴리즈 시 0으로 변경할 것
            m_resourcesDict[argType] = 100000;
            m_producedResourcesDict[argType] = 0;
        }

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            m_nowUIManager = canvas.GetComponent<IUIManager>();

            if (m_nowUIManager == null)
            {
                m_nowUIManager = canvas.GetComponentInChildren<IUIManager>();
            }

            if (m_nowUIManager != null)
            {
                m_nowUIManager.Initialize();
            }
            else
            {
                Debug.LogError("No IUIManager found on the Canvas or its children!");
            }
        }
        else
        {
            Debug.LogError("No Canvas found in the scene!");
        }

        AddDate(0);
    }

    public void AddDate(int argAddDate)
    {
        if (argAddDate < 0)
        {
            Date = 0;
            Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
            return;
        }

        GetBuildingDateResource();
        GetDayResource(argAddDate);

        Date += argAddDate;
    }

    public bool TryChangeAllResources(Dictionary<ResourceType, long> argResourceChanges)
    {
        foreach (KeyValuePair<ResourceType, long> item in argResourceChanges)
        {
            if (!m_resourcesDict.ContainsKey(item.Key))
            {
                Debug.LogError($"Resource type {item.Key} not found in resource dictionary.");
                return false;
            }

            if (item.Value < 0)
            {
                if (m_resourcesDict[item.Key] + item.Value < 0)
                {
                    Warning($"Not enough {item.Key} to perform this action. Required: {Mathf.Abs(item.Value)}, Have: {m_resourcesDict[item.Key]}");
                    return false;
                }
            }
        }

        foreach (KeyValuePair<ResourceType, long> item in argResourceChanges)
        {
            m_resourcesDict[item.Key] += item.Value;
        }

        m_nowUIManager.SetAllResourceText();
        return true;
    }

    public bool TryChangeResource(ResourceType argType, long argAmount)
    {
        if (!m_resourcesDict.ContainsKey(argType))
        {
            Debug.LogError($"Resource type {argType} not found in resource dictionary.");
            return false;
        }

        if (argAmount < 0)
        {
            if (m_resourcesDict[argType] + argAmount < 0)
            {
                Warning($"Not enough {argType} to perform this action. Required: {Mathf.Abs(argAmount)}, Have: {m_resourcesDict[argType]}");
                return false;
            }
        }

        m_resourcesDict[argType] += argAmount;

        m_nowUIManager.SetAllResourceText();
        return true;
    }

    public void GetDayResource(int argDay)
    {
        for (int i = 0; i < argDay; i++)
        {
            foreach (KeyValuePair<ResourceType, long> kvp in m_producedResourcesDict)
            {
                TryChangeResource(kvp.Key, kvp.Value);
            }
        }
    }

    public void GetBuildingDateResource()
    {
        List<ResourceType> keyList = new List<ResourceType>(m_producedResourcesDict.Keys);
        foreach (ResourceType key in keyList)
        {
            m_producedResourcesDict[key] = 0;
        }

        foreach (KeyValuePair<string, BuildingEntry> kvp in GameDataManager.BuildingEntryDict)
        {
            kvp.Value.ApplyProduction();

            foreach (ResourceAmount argProduction in kvp.Value.m_state.m_calculatedProductionList)
            {
                if (m_producedResourcesDict.ContainsKey(argProduction.m_type))
                {
                    m_producedResourcesDict[argProduction.m_type] += argProduction.m_amount;
                }
                else
                {
                    Debug.LogError($"Production resource type {argProduction.m_type} not found in produced resources dictionary.");
                }
            }
        }
    }

    public void LoadScene(string argSceneName)
    {
        m_scenLoadManager.LoadScene(argSceneName);
    }

    public long GetResource(ResourceType argType)
    {
        return m_resourcesDict.TryGetValue(argType, out long value) ? value : 0;
    }

    public long GetDayAddResource(ResourceType argType)
    {
        return m_producedResourcesDict.TryGetValue(argType, out long value) ? value : 0;
    }

    public void Warning(string argWarnStr)
    {
        GameObject _obj = Instantiate(m_warningPanelPrefeb, m_nowUIManager.CanvasTrans);
        _obj.transform.Find("Text").gameObject.GetComponent<Text>().text = argWarnStr;
    }

    public void ShowConfirmDialog(string message, Action onYes)
    {
        GameObject dialogObj = Instantiate(m_confirmDialogPrefab, m_nowUIManager.CanvasTrans);
        ConfirmDialogUI dialog = dialogObj.GetComponent<ConfirmDialogUI>();
        dialog.Setup(message, onYes);
    }
}