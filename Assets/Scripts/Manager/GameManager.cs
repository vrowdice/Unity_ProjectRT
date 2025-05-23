using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    public GameDataManager GameDataManager => m_gameDataManager;

    public long Date { get; private set; }

    [SerializeField]
    private SceneLoadManager m_scenLoadManager = null;
    [SerializeField]
    private GameDataManager m_gameDataManager = null;
    [SerializeField]
    private GameObject m_mainUiManagerPrefeb = null;
    [SerializeField]
    private GameObject m_warningPanelPrefeb = null;

    private IUIManager m_nowUIManager = null;

    private Dictionary<ResourceType, long> m_resources = new();
    private Dictionary<ResourceType, long> m_dayAddResources = new();

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
        m_nowUIManager = Instantiate(m_mainUiManagerPrefeb).GetComponent<MainUIManager>();

        // Initialize resource dictionaries
        foreach (ResourceType argType in System.Enum.GetValues(typeof(ResourceType)))
        {
            m_resources[argType] = 0;
            m_dayAddResources[argType] = 0;
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

    public bool TryConsumeResource(ResourceType argType, long argAmount)
    {
        if (argAmount < 0)
        {
            Debug.LogError(ExceptionMessages.ErrorNegativeValue);
            return false;
        }

        if (m_resources.TryGetValue(argType, out long currentAmount))
        {
            if (currentAmount < argAmount)
                return false;

            m_resources[argType] = currentAmount - argAmount;
            return true;
        }

        Debug.LogError(ExceptionMessages.ErrorNoSuchType);
        return false;
    }

    public bool TryAddResource(ResourceType argType, long argAmount)
    {
        if (argAmount < 0)
        {
            Debug.LogError(ExceptionMessages.ErrorNegativeValue);
            return false;
        }

        if (m_resources.ContainsKey(argType))
        {
            m_resources[argType] += argAmount;
            return true;
        }

        Debug.LogError(ExceptionMessages.ErrorNoSuchType);
        return false;
    }

    public void GetDayResource(int argDay)
    {
        foreach (KeyValuePair<ResourceType, long> kvp in m_dayAddResources)
        {
            TryAddResource(kvp.Key, kvp.Value);
        }
    }

    public void GetBuildingDateResource()
    {
        // Reset day add resource dictionary
        List<ResourceType> keyList = new List<ResourceType>(m_dayAddResources.Keys);
        foreach (ResourceType key in keyList)
        {
            m_dayAddResources[key] = 0;
        }

        foreach (KeyValuePair<string, BuildingEntry> kvp in GameDataManager.BuildingEntryDict)
        {
            kvp.Value.ApplyProduction();

            foreach (ResourceAmount argProduction in kvp.Value.m_state.m_calculatedProductionList)
            {
                if (m_dayAddResources.ContainsKey(argProduction.m_type))
                {
                    m_dayAddResources[argProduction.m_type] += argProduction.m_amount;
                }
                else
                {
                    Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                }
            }
        }
    }

    /// <summary>
    /// 씬을 로드합니다
    /// </summary>
    /// <param name="argSceneName">씬 이름</param>
    public void LoadScene(string argSceneName)
    {
        m_scenLoadManager.LoadScene(argSceneName);
    }

    /// <summary>
    /// 현재 리소스 양을 가져옵니다
    /// </summary>
    public long GetResource(ResourceType argType)
    {
        return m_resources.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 하루 자원 증가량을 가져옵니다
    /// </summary>
    public long GetDayAddResource(ResourceType argType)
    {
        return m_dayAddResources.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 경고
    /// </summary>
    /// <param name="argWarnStr">경고 문자열</param>
    public void Warning(string argWarnStr)
    {
        GameObject _obj = Instantiate(m_warningPanelPrefeb, m_nowUIManager.CanvasTrans);
        _obj.transform.Find("Text").gameObject.GetComponent<Text>().text = argWarnStr;
    }
}
