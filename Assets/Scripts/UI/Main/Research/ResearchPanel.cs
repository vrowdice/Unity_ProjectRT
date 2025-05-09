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

    public void OnOpen()
    {
        m_gameDataManager = GameManager.Instance.GameDataManager;

        foreach (Transform item in m_researchScrollViewContentTrans)
        {
            Destroy(item.gameObject);
        }

        foreach (KeyValuePair<string, ResearchData> item in m_gameDataManager.CommonResearchDataDict)
        {
            Instantiate(m_commonResearchBtnPrefeb, m_researchScrollViewContentTrans).GetComponent<CommonResearchBtn>().
                Setting(item.Value.m_code, item.Value.m_name, item.Value.m_description);
        }

        gameObject.SetActive(true);
    }

    public void OnClose()
    {

    }
}
