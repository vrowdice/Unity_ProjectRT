using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUIManager : MonoBehaviour, IUIManager
{
    [SerializeField]
    List<GameObject> m_panelList = new List<GameObject>();
    [SerializeField]
    GameObject m_BackMainBtn = null;

    [Header("Resource Panel Text")]
    [SerializeField]
    TextMeshProUGUI m_woodText = null;
    [SerializeField]
    TextMeshProUGUI m_addWoodText = null;
    [SerializeField]
    TextMeshProUGUI m_metalText = null;
    [SerializeField]
    TextMeshProUGUI m_addMetalText = null;
    [SerializeField]
    TextMeshProUGUI m_foodText = null;
    [SerializeField]
    TextMeshProUGUI m_addFoodText = null;
    [SerializeField]
    TextMeshProUGUI m_techText = null;
    [SerializeField]
    TextMeshProUGUI m_addTechText = null;

    [Header("Game Info Text")]
    [SerializeField]
    TextMeshProUGUI m_dateText = null;
    [SerializeField]
    TextMeshProUGUI m_requestText = null;

    [Header("Common UI")]
    [SerializeField]
    GameObject m_resourceIconTextPrefeb = null;

    private GameManager m_gameManager = null;
    private List<IUIPanel> m_iPanelList = new List<IUIPanel>();
    private Transform m_canvasTrans = null;
    private int m_nowPanelIndex = 0;

    public GameObject ResourceIconTextPrefeb { get => m_resourceIconTextPrefeb; }
    public Transform CanvasTrans => m_canvasTrans;

    void SetResourceText(ResourceType.TYPE argType)
    {
        long resourceAmount = m_gameManager.GetResource(argType);
        string formattedAmount = ReplaceUtils.FormatNumber(resourceAmount);

        switch (argType)
        {
            case ResourceType.TYPE.Wood:
                m_woodText.text = formattedAmount;
                break;
            case ResourceType.TYPE.Iron:
                m_metalText.text = formattedAmount;
                break;
            case ResourceType.TYPE.Food:
                m_foodText.text = formattedAmount;
                break;
            case ResourceType.TYPE.Tech:
                m_techText.text = formattedAmount;
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                break;
        }
    }

    void SetAddResourceText(ResourceType.TYPE argType)
    {
        long resourceAmount = m_gameManager.GetDayAddResource(argType);
        string formattedAmount = ReplaceUtils.FormatNumber(resourceAmount);

        switch (argType)
        {
            case ResourceType.TYPE.Wood:
                m_addWoodText.text = "+ " + formattedAmount;
                break;
            case ResourceType.TYPE.Iron:
                m_addMetalText.text = "+ " + formattedAmount;
                break;
            case ResourceType.TYPE.Food:
                m_addFoodText.text = "+ " + formattedAmount;
                break;
            case ResourceType.TYPE.Tech:
                m_addTechText.text = "+ " + formattedAmount;
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                break;
        }
    }

    public void Initialize()
    {
        m_gameManager = GameManager.Instance;
        m_canvasTrans = transform;

        //패널 초기화
        foreach (GameObject item in m_panelList)
        {
            item.SetActive(false);

            IUIPanel panel = item.GetComponent<IUIPanel>();
            if (panel != null)
            {
                m_iPanelList.Add(panel);
            }
            else
            {
                Debug.LogWarning($"Panel object {item.name} does not implement IUIPanel.");
                m_iPanelList.Add(null);
            }
        }
        m_panelList[0].SetActive(true);

        m_BackMainBtn.SetActive(false);

        UpdateAllMainText();
    }

    public void UpdateAllMainText()
    {
        SetResourceText(ResourceType.TYPE.Wood);
        SetResourceText(ResourceType.TYPE.Iron);
        SetResourceText(ResourceType.TYPE.Food);
        SetResourceText(ResourceType.TYPE.Tech);

        SetAddResourceText(ResourceType.TYPE.Wood);
        SetAddResourceText(ResourceType.TYPE.Iron);
        SetAddResourceText(ResourceType.TYPE.Food);
        SetAddResourceText(ResourceType.TYPE.Tech);

        UpdateDateText();
        UpdateRequestText();
    }

    public bool TryAdd(ResourceType.TYPE argType, int argAmount)
    {
        if (m_gameManager.TryChangeResource(argType, argAmount))
        {
            SetResourceText(argType);
            return true;
        }
        return false;
    }

    public bool TryConsume(ResourceType.TYPE argType, int argAmount)
    {
        if (m_gameManager.TryChangeResource(argType, argAmount))
        {
            SetResourceText(argType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 패널 인덱스에 따라 패널 이동
    /// </summary>
    /// <param name="argPanelIndex">패널 인덱스</param>
    public void MovePanel(int argPanelIndex)
    {
        if (m_panelList.Count <= argPanelIndex || 0 > argPanelIndex)
        {
            return;
        }

        m_nowPanelIndex = argPanelIndex;

        foreach (GameObject item in m_panelList)
        {
            item.SetActive(false);
        }
        m_panelList[argPanelIndex].SetActive(true);

        if (m_nowPanelIndex != 0)
        {
            m_BackMainBtn.SetActive(true);
        }
        else
        {
            m_BackMainBtn.SetActive(false);
        }

        if (m_iPanelList[argPanelIndex] != null)
        {
            m_iPanelList[argPanelIndex].OnOpen(GameManager.Instance.GameDataManager, this);
        }
    }

    public void UpdateRequestText()
    {
        m_requestText.text = m_gameManager.GameDataManager.AcceptableRequestList.Count + " / " + m_gameManager.GameDataManager.AcceptedRequestList.Count;
    }

    public void UpdateDateText()
    {
        m_dateText.text = GameManager.Instance.Date + " Days";
    }

    public void AddOneDate()
    {
        GameManager.Instance.AddDate(1);

        UpdateAllMainText();
    }

    //--디버그용--
    //릴리즈 시 주석처리 하거나 삭제할 것
    public void AddResource1000()
    {
        TryAdd(ResourceType.TYPE.Wood, 1000);
        TryAdd(ResourceType.TYPE.Iron, 1000);
        TryAdd(ResourceType.TYPE.Food, 1000);
        TryAdd(ResourceType.TYPE.Tech, 1000);
    }
}
