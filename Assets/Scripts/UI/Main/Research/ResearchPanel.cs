using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchPanel : MonoBehaviour, IUIPanel
{
    [SerializeField]
    Transform m_researchScrollViewContentTrans = null;
    [SerializeField]
    GameObject m_commonResearchBtnPrefeb = null;

    private GameDataManager m_gameDataManager = null;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;
        gameObject.SetActive(true);

        SelectResearchContent(0);
    }

    public void OnClose()
    {

    }

    /// <summary>
    /// 연구 패널 선택
    /// </summary>
    /// <param name="argPanelIndex"></param>
    /// 0 = 연구 가능
    /// 1 = 잠긴
    /// 2 = 연구된
    public void SelectResearchContent(int argPanelIndex)
    {
        foreach (Transform item in m_researchScrollViewContentTrans)
        {
            Destroy(item.gameObject);
        }

        foreach (KeyValuePair<string, ResearchEntry> item in m_gameDataManager.CommonResearchDataDict)
        {
            switch (argPanelIndex)
            {
                case 0:
                    if (item.Value.m_state.m_isLocked == false && item.Value.m_state.m_isResearched == false)
                    {
                        Instantiate(m_commonResearchBtnPrefeb, m_researchScrollViewContentTrans).GetComponent<CommonResearchBtn>().
                            Setting(item.Value.m_data.m_code, item.Value.m_data.m_icon, item.Value.m_data.m_name, item.Value.m_data.m_description);
                    }
                    break;
                case 1:
                    if (item.Value.m_state.m_isLocked == true)
                    {
                        Instantiate(m_commonResearchBtnPrefeb, m_researchScrollViewContentTrans).GetComponent<CommonResearchBtn>().
                            Setting(item.Value.m_data.m_code, item.Value.m_data.m_icon, item.Value.m_data.m_name, item.Value.m_data.m_description);
                    }
                    break;
                case 2:
                    if (item.Value.m_state.m_isResearched == true)
                    {
                        Instantiate(m_commonResearchBtnPrefeb, m_researchScrollViewContentTrans).GetComponent<CommonResearchBtn>().
                            Setting(item.Value.m_data.m_code, item.Value.m_data.m_icon, item.Value.m_data.m_name, item.Value.m_data.m_description);
                    }
                    break;
                default:
                    Debug.LogError(ExceptionMessages.ErrorInvalidResearchInfo);
                    break;
            }

        }
    }
}
