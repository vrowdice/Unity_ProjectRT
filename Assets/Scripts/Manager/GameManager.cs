using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private SceneLoadManager m_scenLoadManager = null;
    [SerializeField]
    private GameDataManager m_gameDataManager = null;
    [SerializeField]
    private GameObject m_warningPanelPrefeb = null;
    [SerializeField]
    private GameObject m_confirmDialogPrefab = null;

    public static GameManager Instance { get; private set; }
    public GameDataManager GameDataManager => m_gameDataManager;

    public int Date { get; private set; }
    public long WealthToken { get; private set; }
    public long ExchangeToken { get; private set; }

    private IUIManager m_nowUIManager = null;

    private Dictionary<ResourceType.TYPE, long> m_resourcesDict = new();
    private Dictionary<ResourceType.TYPE, long> m_producedResourcesDict = new();

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
        foreach (ResourceType.TYPE argType in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
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
                m_nowUIManager.Initialize(this);
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

        GetBuildingDateResource();
    }

    public void AddDate(int argAddDate)
    {
        if (argAddDate < 0)
        {
            Date = 0;
            Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
            return;
        }

        if(GameDataManager.GameBalanceEntry.m_data.m_maxDate <= Date)
        {
            Date = GameDataManager.GameBalanceEntry.m_data.m_maxDate;
        }

        GetBuildingDateResource();
        GetDayResource(argAddDate);

        Date += argAddDate;

        GameBalanceEntry _balanceEntry = m_gameDataManager.GameBalanceEntry;

        _balanceEntry.m_state.m_dateMul = 1.0f + Mathf.Pow(_balanceEntry.m_data.m_dateBalanceMul, Date);
        if (Date % _balanceEntry.m_data.m_makeRequestDate == 0)
        {
            m_gameDataManager.MakeRandomRequest();
        }

        if(_balanceEntry.m_data.m_forcedContactRequestList.Contains(Date) == true)
        {
            m_gameDataManager.RandomContactRequest();
        }

        if (m_gameDataManager.EventEntry.AddDate() == true)
        {
            Warning(InfoMessages.EventOccurs);
        }
    }

    public bool TryChangeAllResources(Dictionary<ResourceType.TYPE, long> argResourceChanges)
    {
        foreach (KeyValuePair<ResourceType.TYPE, long> item in argResourceChanges)
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

        foreach (KeyValuePair<ResourceType.TYPE, long> item in argResourceChanges)
        {
            m_resourcesDict[item.Key] += item.Value;
        }

        m_nowUIManager.UpdateAllMainText();
        return true;
    }

    public bool TryChangeResource(ResourceType.TYPE argType, long argAmount)
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

        m_nowUIManager.UpdateAllMainText();
        return true;
    }

    public void GetDayResource(int argDay)
    {
        for (int i = 0; i < argDay; i++)
        {
            foreach (var kvp in m_producedResourcesDict.ToList())
            {
                TryChangeResource(kvp.Key, kvp.Value);
            }
        }
    }

    public void GetBuildingDateResource()
    {
        //리소스 양을 모두 0으로
        foreach (var key in m_producedResourcesDict.Keys.ToList())
        {
            m_producedResourcesDict[key] = 0;
        }

        // 각 건물마다 생산 계산
        foreach (var buildingPair in GameDataManager.BuildingEntryDict)
        {
            BuildingEntry building = buildingPair.Value;
            building.ApplyProduction();

            foreach (var production in building.m_state.m_calculatedProductionList)
            {
                float modifier = 1f;
                if (m_gameDataManager.EventEntry.m_state.m_buildingResourceModDic.TryGetValue(production.m_type, out float value))
                {
                    modifier = (value == 0f) ? 1f : value;
                }

                if (m_producedResourcesDict.ContainsKey(production.m_type))
                {
                    m_producedResourcesDict[production.m_type] += (long)(production.m_amount * modifier);
                }
                else
                {
                    Debug.LogWarning($"[GetBuildingDateResource] Unknown resource type: {production.m_type}");
                }
            }
        }
    }


    public void LoadScene(string argSceneName)
    {
        m_scenLoadManager.LoadScene(argSceneName);
    }

    public long GetResource(ResourceType.TYPE argType)
    {
        return m_resourcesDict.TryGetValue(argType, out long value) ? value : 0;
    }

    public long GetDayAddResource(ResourceType.TYPE argType)
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