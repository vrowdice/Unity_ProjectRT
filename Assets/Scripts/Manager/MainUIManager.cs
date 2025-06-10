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

    void SetResourceText(ResourceType argType)
    {
        long resourceAmount = m_gameManager.GetResource(argType);
        string formattedAmount = ReplaceUtils.FormatNumber(resourceAmount);

        switch (argType)
        {
            case ResourceType.Wood:
                m_woodText.text = formattedAmount;
                break;
            case ResourceType.Iron:
                m_metalText.text = formattedAmount;
                break;
            case ResourceType.Food:
                m_foodText.text = formattedAmount;
                break;
            case ResourceType.Tech:
                m_techText.text = formattedAmount;
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                break;
        }
    }

    void SetAddResourceText(ResourceType argType)
    {
        long resourceAmount = m_gameManager.GetDayAddResource(argType);
        string formattedAmount = ReplaceUtils.FormatNumber(resourceAmount);

        switch (argType)
        {
            case ResourceType.Wood:
                m_addWoodText.text = "+ " + formattedAmount;
                break;
            case ResourceType.Iron:
                m_addMetalText.text = "+ " + formattedAmount;
                break;
            case ResourceType.Food:
                m_addFoodText.text = "+ " + formattedAmount;
                break;
            case ResourceType.Tech:
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

        //�г� �ʱ�ȭ
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
        SetResourceText(ResourceType.Wood);
        SetResourceText(ResourceType.Iron);
        SetResourceText(ResourceType.Food);
        SetResourceText(ResourceType.Tech);

        SetAddResourceText(ResourceType.Wood);
        SetAddResourceText(ResourceType.Iron);
        SetAddResourceText(ResourceType.Food);
        SetAddResourceText(ResourceType.Tech);

        UpdateDateText();
        UpdateRequestText();
    }

    public bool TryAdd(ResourceType argType, int argAmount)
    {
        if (m_gameManager.TryChangeResource(argType, argAmount))
        {
            SetResourceText(argType);
            return true;
        }
        return false;
    }

    public bool TryConsume(ResourceType argType, int argAmount)
    {
        if (m_gameManager.TryChangeResource(argType, argAmount))
        {
            SetResourceText(argType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// �г� �ε����� ���� �г� �̵�
    /// </summary>
    /// <param name="argPanelIndex">�г� �ε���</param>
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

    //--����׿�--
    //������ �� �ּ�ó�� �ϰų� ������ ��
    public void AddResource1000()
    {
        TryAdd(ResourceType.Wood, 1000);
        TryAdd(ResourceType.Iron, 1000);
        TryAdd(ResourceType.Food, 1000);
        TryAdd(ResourceType.Tech, 1000);
    }
}
