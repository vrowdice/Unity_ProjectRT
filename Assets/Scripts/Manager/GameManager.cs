using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    public GameDataManager GameDataManager => m_gameDataManager;
    public MainUIManager MainUiManager => m_mainUiManager;
    public int Wood { get; private set; }
    public int Metal { get; private set; }
    public int Food { get; private set; }
    public int Tech { get; private set; }
    public int Date { get; private set; }

    [SerializeField]
    SceneLoadManager m_scenLoadManager = null;
    [SerializeField]
    GameDataManager m_gameDataManager = null;
    [SerializeField]
    GameObject m_mainUiManagerPrefeb = null;


    private MainUIManager m_mainUiManager = null;


    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate
        }
    }

    void Start()
    {
        FirstSetting();
    }

    void Update()
    {
        // Update logic if needed
    }

    void FirstSetting()
    {
        m_mainUiManager = Instantiate(m_mainUiManagerPrefeb).GetComponent<MainUIManager>();

        //저장이 생기면 지울 것
        Wood = 0;
        Metal = 0;
        Food = 0;
        Tech = 0;
    }

    public void AddDate(int argAddDate)
    {
        if (argAddDate < 0)
        {
            Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
            return;
        }

        Date += argAddDate;
    }


    public bool TryConsumeResource(ResourceType argType, int argAmount)
    {
        if (argAmount < 0)
        {
            Debug.LogError(ExceptionMessages.ErrorNegativeValue);
            return false;
        }

        int currentAmount = GetResourceAmount(argType);
        if (currentAmount < argAmount)
            return false;

        SetResourceAmount(argType, currentAmount - argAmount);
        return true;
    }

    public bool TryAddResource(ResourceType argType, int argAmount)
    {
        if (argAmount < 0)
        {
            Debug.LogError(ExceptionMessages.ErrorNegativeValue);
            return false;
        }

        int currentAmount = GetResourceAmount(argType);
        SetResourceAmount(argType, currentAmount + argAmount);
        return true;
    }

    private int GetResourceAmount(ResourceType argType)
    {
        switch (argType)
        {
            case ResourceType.Wood:
                return Wood;
            case ResourceType.Metal:
                return Metal;
            case ResourceType.Food:
                return Food;
            case ResourceType.Tech:
                return Tech;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                return 0;
        }
    }

    private void SetResourceAmount(ResourceType argType, int newAmount)
    {
        switch (argType)
        {
            case ResourceType.Wood:
                Wood = newAmount;
                break;
            case ResourceType.Metal:
                Metal = newAmount;
                break;
            case ResourceType.Food:
                Food = newAmount;
                break;
            case ResourceType.Tech:
                Tech = newAmount;
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                break;
        }
    }


    /// <summary>
    /// 씬을 로드합니다
    /// </summary>
    /// <param name="argScenName">씬 이름</param>
    public void LoadScene(string argScenName)
    {
        m_scenLoadManager.LoadScene(argScenName);
    }
}