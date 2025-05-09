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

        Wood = 0;
        Metal = 0;
        Food = 0;
        Tech = 0;
    }

    public bool TryConsumeResource(ResourceType argType, int argAmount)
    {
        switch (argType)
        {
            case ResourceType.Wood:
                return TryConsumeWood(argAmount);
            case ResourceType.Metal:
                return TryConsumeMetal(argAmount);
            case ResourceType.Food:
                return TryConsumeFood(argAmount);
            case ResourceType.Tech:
                return TryConsumeTech(argAmount);
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                return false;
        }
    }
    public bool TryConsumeWood(int argAmount)
    {
        if (Wood < argAmount)
            return false;

        Wood -= argAmount;
        return true;
    }
    public bool TryConsumeMetal(int argAmount)
    {
        if (Metal < argAmount)
            return false;

        Metal -= argAmount;
        return true;
    }
    public bool TryConsumeFood(int argAmount)
    {
        if (Food < argAmount)
            return false;

        Food -= argAmount;
        return true;
    }
    public bool TryConsumeTech(int argAmount)
    {
        if (Tech < argAmount)
            return false;

        Tech -= argAmount;
        return true;
    }
    public bool TryAddResource(ResourceType argType, int argAmount)
    {
        if (argAmount < 0)
        {
            Debug.LogError("Cannot add a negative amount.");
            return false;
        }

        switch (argType)
        {
            case ResourceType.Wood:
                AddWood(argAmount);
                return true;
            case ResourceType.Metal:
                AddMetal(argAmount);
                return true;
            case ResourceType.Food:
                AddFood(argAmount);
                return true;
            case ResourceType.Tech:
                AddTech(argAmount);
                return true;
            default:
                Debug.LogError("No Such Resource Type");
                return false;
        }
    }
    public void AddWood(int argAmount)
    {
        Wood += argAmount < 0 ? 0 : argAmount;
    }

    public void AddMetal(int argAmount)
    {
        Metal += argAmount < 0 ? 0 : argAmount;
    }

    public void AddFood(int argAmount)
    {
        Food += argAmount < 0 ? 0 : argAmount;
    }

    public void AddTech(int argAmount)
    {
        Tech += argAmount < 0 ? 0 : argAmount;
    }

    /// <summary>
    /// æ¿¿ª ∑ŒµÂ«’¥œ¥Ÿ
    /// </summary>
    /// <param name="argScenName">æ¿ ¿Ã∏ß</param>
    public void LoadScene(string argScenName)
    {
        m_scenLoadManager.LoadScene(argScenName);
    }
}