using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchPanel : BasePanel
{
    [SerializeField]
    Transform m_researchScrollViewContentTrans = null;
    [SerializeField]
    GameObject m_commonResearchBtnPrefeb = null;

    private const int m_labBuildingCode = 10001;

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Lab");
        SetBuildingLevel(m_labBuildingCode);

        SelectResearchContent(0);
    }

    /// <summary>
    /// 연구 패널 선택
    /// </summary>
    /// <param name="argPanelIndex"></param>
    /// 0 = 연구 가능
    /// 1 = 잠금
    /// 2 = 완료됨
    public void SelectResearchContent(int argPanelIndex)
    {
        if (m_researchScrollViewContentTrans == null)
        {
            Debug.LogError("Research scroll view content transform is null!");
            return;
        }

        if (m_commonResearchBtnPrefeb == null)
        {
            Debug.LogError("Common research button prefab is null!");
            return;
        }

        ClearResearchButtons();
        CreateResearchButtons(argPanelIndex);
    }

    /// <summary>
    /// 연구 버튼들 제거
    /// </summary>
    private void ClearResearchButtons()
    {
        foreach (Transform item in m_researchScrollViewContentTrans)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
    }

    /// <summary>
    /// 연구 버튼들 생성
    /// </summary>
    /// <param name="panelIndex">패널 인덱스</param>
    private void CreateResearchButtons(int panelIndex)
    {
        foreach (KeyValuePair<string, ResearchEntry> item in m_gameDataManager.CommonResearchEntryDict)
        {
            if (item.Value == null) continue;

            bool shouldCreate = false;

            switch (panelIndex)
            {
                case 0: // 연구 가능
                    shouldCreate = item.Value.m_state.m_isLocked == false && item.Value.m_state.m_isResearched == false;
                    break;
                case 1: // 잠금
                    shouldCreate = item.Value.m_state.m_isLocked == true;
                    break;
                case 2: // 완료됨
                    shouldCreate = item.Value.m_state.m_isResearched == true;
                    break;
                default:
                    Debug.LogError(ExceptionMessages.ErrorInvalidResearchInfo);
                    return;
            }

            if (shouldCreate)
            {
                GameObject btnObj = Instantiate(m_commonResearchBtnPrefeb, m_researchScrollViewContentTrans);
                CommonResearchBtn researchBtn = btnObj.GetComponent<CommonResearchBtn>();
                
                if (researchBtn != null)
                {
                    researchBtn.Initialize(this, item.Value);
                }
                else
                {
                    Debug.LogError("CommonResearchBtn component not found on prefab!");
                }
            }
        }
    }
}
