using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleUIManager : MonoBehaviour, IUIManager
{
    [SerializeField]
    List<GameObject> m_panelList = new List<GameObject>();

    [Header("Common UI Prefabs")]
    [SerializeField]
    GameObject m_resourceIconTextPrefeb = null;
    [SerializeField]
    GameObject m_conditionPanelTextPrefeb = null;

    private List<IUIPanel> m_iPanelList = new List<IUIPanel>();
    private Transform m_canvasTrans = null;
    private int m_nowPanelIndex = 0;

    public Transform CanvasTrans => m_canvasTrans;
    
    // IUIManager 인터페이스 구현
    public GameObject ResourceIconTextPrefeb => m_resourceIconTextPrefeb;
    public GameObject ConditionPanelTextPrefeb => m_conditionPanelTextPrefeb;

    void Start()
    {
        Initialize(GameManager.Instance, GameDataManager.Instance);
    }

    public void Initialize(GameManager argGameManager, GameDataManager argGameDataManager)
    {
        m_canvasTrans = transform;

        m_iPanelList.Clear();

        foreach (GameObject item in m_panelList)
        {
            if (item == null)
            {
                m_iPanelList.Add(null);
                continue;
            }

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

        if (m_panelList.Count > 0 && m_panelList[0] != null)
        {
            m_panelList[0].SetActive(true);
        }

        MovePanel(0);
    }

    public void MovePanel(int argPanelIndex)
    {
        if (!IsValidPanelIndex(argPanelIndex))
        {
            Debug.LogWarning($"Invalid panel index: {argPanelIndex}. Valid range: 0-{m_panelList.Count - 1}");
            return;
        }

        m_nowPanelIndex = argPanelIndex;

        DeactivateAllPanels();
        ActivateCurrentPanel();
        OpenCurrentPanel();
    }

    private bool IsValidPanelIndex(int panelIndex)
    {
        return panelIndex >= 0 && panelIndex < m_panelList.Count;
    }

    private void DeactivateAllPanels()
    {
        foreach (GameObject item in m_panelList)
        {
            if (item != null)
            {
                item.SetActive(false);
            }
        }
    }

    private void ActivateCurrentPanel()
    {
        if (m_panelList[m_nowPanelIndex] != null)
        {
            m_panelList[m_nowPanelIndex].SetActive(true);
        }
        else
        {
            Debug.LogError($"Panel at index {m_nowPanelIndex} is null!");
        }
    }

    private void OpenCurrentPanel()
    {
        if (m_iPanelList[m_nowPanelIndex] != null)
        {
            // TitleUIManager는 IUIManager를 구현하므로 this를 전달
            m_iPanelList[m_nowPanelIndex].OnOpen(GameDataManager.Instance, this);
        }
        else
        {
            Debug.LogWarning($"IUIPanel at index {m_nowPanelIndex} is null!");
        }
    }

    public void UpdateAllMainText()
    {
        // Title 씬에서는 메인 리소스/요청 텍스트가 없으므로 현재는 비워 둡니다.
    }
}
