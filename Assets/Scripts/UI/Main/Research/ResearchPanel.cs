using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchPanel : BasePanel
{
    [Header("Research Panel")]
    [SerializeField]
    Transform m_researchScrollViewContentTrans = null;
    [SerializeField]
    Transform m_researchInprogressScrollViewContentTrans = null;
    [SerializeField]
    GameObject m_commonResearchBtnPrefeb = null;
    [SerializeField]
    GameObject m_researchInprogressBtnPrefeb = null;
    [SerializeField]
    ResearchDetailPanel m_researchDetailPanel = null;

    private const int m_labBuildingCode = 10001;

    // 연구 중인 항목들을 추적하는 리스트
    private List<ResearchInprogressBtn> m_inprogressButtons = new List<ResearchInprogressBtn>();

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
        UpdateInprogressResearch();
    }

    /// <summary>
    /// 연구 상세 패널 열기
    /// </summary>
    /// <param name="researchEntry">연구 항목 데이터</param>
    public void OpenResearchDetailPanel(ResearchEntry researchEntry)
    {
        if (m_researchDetailPanel != null)
        {
            m_researchDetailPanel.SetResearchDetail(researchEntry, this);
        }
        else
        {
            Debug.LogError("Research detail panel is not assigned!");
        }
    }

    /// <summary>
    /// 연구 중인 항목들 업데이트
    /// </summary>
    public void UpdateInprogressResearch()
    {
        ClearInprogressButtons();
        CreateInprogressButtons();
    }

    /// <summary>
    /// 연구 중인 버튼들 제거
    /// </summary>
    private void ClearInprogressButtons()
    {
        foreach (Transform item in m_researchInprogressScrollViewContentTrans)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        m_inprogressButtons.Clear();
    }

    /// <summary>
    /// 연구 중인 버튼들 생성
    /// </summary>
    private void CreateInprogressButtons()
    {
        if (m_researchInprogressBtnPrefeb == null)
        {
            Debug.LogError("Research inprogress button prefab is null!");
            return;
        }

        foreach (KeyValuePair<string, ResearchEntry> item in m_gameDataManager.CommonResearchEntryDict)
        {
            if (item.Value == null) continue;

            // 연구 중인 항목인지 확인 (잠금 해제되었고, 연구가 진행 중인 경우)
            if (!item.Value.m_state.m_isLocked && 
                item.Value.m_state.IsInProgress)
            {
                GameObject btnObj = Instantiate(m_researchInprogressBtnPrefeb, m_researchInprogressScrollViewContentTrans);
                ResearchInprogressBtn inprogressBtn = btnObj.GetComponent<ResearchInprogressBtn>();
                
                if (inprogressBtn != null)
                {
                    inprogressBtn.Initialize(this, item.Value);
                    m_inprogressButtons.Add(inprogressBtn);
                }
                else
                {
                    Debug.LogError("ResearchInprogressBtn component not found on prefab!");
                }
            }
        }
    }

    /// <summary>
    /// 특정 연구 항목의 진행도 업데이트
    /// </summary>
    /// <param name="researchEntry">업데이트할 연구 항목</param>
    public void UpdateResearchProgress(ResearchEntry researchEntry)
    {
        // 연구 중인 버튼들 중에서 해당 항목을 찾아 업데이트
        foreach (ResearchInprogressBtn btn in m_inprogressButtons)
        {
            if (btn.GetResearchEntry() == researchEntry)
            {
                btn.UpdateProgress();
                break;
            }
        }

        // 연구가 완료되었으면 연구 중인 목록에서 제거
        if (researchEntry.m_state.m_isResearched)
        {
            RemoveInprogressButton(researchEntry);
        }
    }

    /// <summary>
    /// 연구 중인 버튼 제거
    /// </summary>
    /// <param name="researchEntry">제거할 연구 항목</param>
    private void RemoveInprogressButton(ResearchEntry researchEntry)
    {
        for (int i = m_inprogressButtons.Count - 1; i >= 0; i--)
        {
            if (m_inprogressButtons[i].GetResearchEntry() == researchEntry)
            {
                if (m_inprogressButtons[i] != null && m_inprogressButtons[i].gameObject != null)
                {
                    Destroy(m_inprogressButtons[i].gameObject);
                }
                m_inprogressButtons.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 연구 패널 선택
    /// </summary>
    /// <param name="argPanelIndex">연구 패널 인덱스</param>
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
                case 0: // 연구 가능 (잠금 해제되었고, 시작되지 않은 경우)
                    shouldCreate = !item.Value.m_state.m_isLocked && 
                                 item.Value.m_state.IsNotStarted;
                    break;
                case 1: // 잠금
                    shouldCreate = item.Value.m_state.m_isLocked;
                    break;
                case 2: // 완료됨
                    shouldCreate = item.Value.m_state.IsCompleted;
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
