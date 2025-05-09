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

    [Header("Resource Panel Things")]
    [SerializeField]
    TextMeshProUGUI m_woodText = null;
    [SerializeField]
    TextMeshProUGUI m_metalText = null;
    [SerializeField]
    TextMeshProUGUI m_foodText = null;
    [SerializeField]
    TextMeshProUGUI m_techText = null;

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
        m_woodText.text = m_gameManager.Wood.ToString();
        m_metalText.text = m_gameManager.Metal.ToString();
        m_foodText.text = m_gameManager.Food.ToString();
        m_techText.text = m_gameManager.Tech.ToString();
    }

    public bool TryConsume(ResourceType argType, int argAmount)
    {
        if(m_gameManager.TryConsumeResource(argType, argAmount) == true)
        {
            switch (argType)
            {
                case ResourceType.Wood:
                    m_woodText.text = m_gameManager.Wood.ToString();
                    return true;
                case ResourceType.Metal:
                    m_metalText.text = m_gameManager.Metal.ToString();
                    return true;
                case ResourceType.Food:
                    m_foodText.text = m_gameManager.Food.ToString();
                    return true;
                case ResourceType.Tech:
                    m_techText.text = m_gameManager.Tech.ToString();
                    return true;
                default:
                    Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                    return false;
            }
        }
        else
        {
            return false;
        }
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
}
