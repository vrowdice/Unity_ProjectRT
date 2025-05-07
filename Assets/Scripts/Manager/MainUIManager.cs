using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> m_panelList = new List<GameObject>();
    [SerializeField]
    GameObject m_BackMainBtn = null;

    private List<IUIPanel> m_iPanelList = new List<IUIPanel>();
    private Transform m_canvasTrans = null;
    private int m_nowPanelIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_canvasTrans = gameObject.transform.Find("Canvas");

        foreach (GameObject item in m_panelList)
        {
            item.SetActive(false);

            // Try to get the component that implements IUIPanel
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
    }

    // Update is called once per frame
    void Update()
    {

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
