using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    public GameDataManager GameDataManager => m_gameDataManager;
    public MainUIManager MainUiManager => m_mainUiManager;

    [SerializeField]
    SceneLoadManager m_scenLoadManager = null;
    [SerializeField]
    GameDataManager m_gameDataManager = null;
    [SerializeField]
    GameObject m_mainUiManagerPrefeb = null;


    private MainUIManager m_mainUiManager = null;

    // Resource variables
    private int m_wood;
    private int m_metal;
    private int m_food;
    private int m_tech;

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

        m_wood = 0;
        m_metal = 0;
        m_food = 0;
        m_tech = 0;
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
