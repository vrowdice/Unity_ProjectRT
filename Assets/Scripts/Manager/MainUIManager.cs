using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUIManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> m_panelList = new List<GameObject>();
    [SerializeField]
    GameObject m_BackMainBtn = null;

    [Header("Resource Panel Text")]
    [SerializeField]
    TextMeshProUGUI m_woodText = null;
    [SerializeField]
    TextMeshProUGUI m_metalText = null;
    [SerializeField]
    TextMeshProUGUI m_foodText = null;
    [SerializeField]
    TextMeshProUGUI m_techText = null;

    [Header("Game Info Text")]
    [SerializeField]
    TextMeshProUGUI m_dateText = null;
    [SerializeField]
    TextMeshProUGUI m_requestText = null;

    private GameManager m_gameManager = null;
    private List<IUIPanel> m_iPanelList = new List<IUIPanel>();
    private Transform m_canvasTrans = null;
    private int m_nowPanelIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        FirstSetting();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FirstSetting()
    {
        m_gameManager = GameManager.Instance;
        m_canvasTrans = gameObject.transform.Find("Canvas");

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

        //메인으로 돌아가기 버튼
        m_BackMainBtn.SetActive(false);

        SetAllResourceText();
    }

    void SetAllResourceText()
    {
        SetResourceText(ResourceType.Wood);
        SetResourceText(ResourceType.Metal);
        SetResourceText(ResourceType.Food);
        SetResourceText(ResourceType.Tech);
    }

    void SetResourceText(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood:
                m_woodText.text = NumberFormatter.FormatNumber(m_gameManager.Wood);
                break;
            case ResourceType.Metal:
                m_metalText.text = NumberFormatter.FormatNumber(m_gameManager.Metal);
                break;
            case ResourceType.Food:
                m_foodText.text = NumberFormatter.FormatNumber(m_gameManager.Food);
                break;
            case ResourceType.Tech:
                m_techText.text = NumberFormatter.FormatNumber(m_gameManager.Tech);
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                break;
        }
    }

    public bool TryAdd(ResourceType argType, int argAmount)
    {
        if (m_gameManager.TryAddResource(argType, argAmount))
        {
            SetResourceText(argType);
            return true;
        }
        return false;
    }

    public bool TryConsume(ResourceType argType, int argAmount)
    {
        if (m_gameManager.TryConsumeResource(argType, argAmount))
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

        if(m_iPanelList[argPanelIndex] != null)
        {
            m_iPanelList[argPanelIndex].OnOpen();
        }

        if (m_nowPanelIndex != 0)
        {
            m_BackMainBtn.SetActive(true);
        }
        else
        {
            m_BackMainBtn.SetActive(false);
        }
    }

    public void AddOneDate()
    {
        GameManager.Instance.AddDate(1);

        m_dateText.text = GameManager.Instance.Date + " Days";
    }

    //--디버그용--
    //릴리즈 시 주석처리 하거나 삭제할 것
    public void AddResource1000()
    {
        TryAdd(ResourceType.Wood, 1000);
        TryAdd(ResourceType.Metal, 1000);
        TryAdd(ResourceType.Food, 1000);
        TryAdd(ResourceType.Tech, 1000);
    }
}
