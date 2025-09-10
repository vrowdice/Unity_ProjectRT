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
    Transform m_researchFilterScrollViewContentTrans = null;
    [SerializeField]
    GameObject m_commonResearchBtnPrefeb = null;
    [SerializeField]
    GameObject m_researchInprogressBtnPrefeb = null;
    [SerializeField]
    GameObject m_researchFilterBtnPrefeb = null;  // 필터 버튼 프리팹 추가
    [SerializeField]
    ResearchDetailPanel m_researchDetailPanel = null;


    private const int m_labBuildingCode = 10001;

    // 연구 중인 항목들을 추적하는 리스트
    private List<ResearchInprogressBtn> m_inprogressButtons = new List<ResearchInprogressBtn>();
    
    // 필터링 관련 변수들
    private FactionType.TYPE m_currentFilter = FactionType.TYPE.None;  // 현재 선택된 필터
    private bool m_showAllResearch = false;  // 전체 연구 표시 여부
    private int m_currentPanelIndex = 0;  // 현재 선택된 패널 인덱스

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Lab");
        SetBuildingLevel(m_labBuildingCode);

        // 필터 버튼들 생성
        CreateFilterButtons();
        
        // 기본적으로 전체 연구 표시
        m_showAllResearch = true;
        SelectResearchContent(0);
        UpdateInprogressResearch();
    }

    /// <summary>
    /// 연구 상세 패널 열기
    /// </summary>
    /// <param name="researchEntry">연구 항목 데이터</param>
    public void OpenResearchDetailPanel(FactionResearchEntry researchEntry)
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

        // 모든 팩션의 진행중인 연구 확인
        foreach (var factionKvp in m_gameDataManager.FactionEntryDict)
        {
            var factionEntry = factionKvp.Value;
            if (factionEntry?.m_data?.m_research != null)
            {
                foreach (var researchData in factionEntry.m_data.m_research)
                {
                    if (researchData == null) continue;

                    var researchEntry = GameDataManager.Instance.GetResearchEntry(researchData.m_code);
                    if (researchEntry == null) continue;

                    // 연구 중인 항목인지 확인 (잠금 해제되었고, 연구가 진행 중인 경우)
                    if (!researchEntry.m_state.m_isLocked && researchEntry.m_state.IsInProgress)
                    {
                        GameObject btnObj = Instantiate(m_researchInprogressBtnPrefeb, m_researchInprogressScrollViewContentTrans);
                        ResearchInprogressBtn inprogressBtn = btnObj.GetComponent<ResearchInprogressBtn>();
                        
                        if (inprogressBtn != null)
                        {
                            inprogressBtn.Initialize(this, researchData, researchEntry.m_state, factionEntry);
                            m_inprogressButtons.Add(inprogressBtn);
                        }
                        else
                        {
                            Debug.LogError("ResearchInprogressBtn component not found on prefab!");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 특정 연구 항목의 진행도 업데이트
    /// </summary>
    /// <param name="researchEntry">업데이트할 연구 항목</param>
    public void UpdateResearchProgress(FactionResearchEntry researchEntry)
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
    private void RemoveInprogressButton(FactionResearchEntry researchEntry)
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
    /// 필터 버튼들 생성
    /// </summary>
    private void CreateFilterButtons()
    {
        if (m_researchFilterScrollViewContentTrans == null || m_researchFilterBtnPrefeb == null)
        {
            Debug.LogError("Filter components are not assigned!");
            return;
        }

        ClearFilterButtons();

        // "All" 버튼 생성 (전체 연구용)
        CreateAllResearchFilterButton();

        // 모든 팩션에 대해 필터 버튼 생성 (None 포함)
        foreach (var factionEntry in m_gameDataManager.FactionEntryDict)
        {
            // 해당 팩션에 연구가 있는지 확인
            if (factionEntry.Value?.m_data?.m_research != null && 
                factionEntry.Value.m_data.m_research.Count > 0)
            {
                CreateFilterButton(factionEntry.Key);
            }
        }
    }

    /// <summary>
    /// 전체 연구 필터 버튼 생성
    /// </summary>
    private void CreateAllResearchFilterButton()
    {
        GameObject btnObj = Instantiate(m_researchFilterBtnPrefeb, m_researchFilterScrollViewContentTrans);
        ResearchFilterBtn filterBtn = btnObj.GetComponent<ResearchFilterBtn>();
        
        if (filterBtn != null)
        {
            filterBtn.InitForAllResearch(this);
        }
        else
        {
            Debug.LogError("ResearchFilterBtn component not found on prefab!");
        }
    }

    /// <summary>
    /// 개별 필터 버튼 생성
    /// </summary>
    /// <param name="factionType">팩션 타입</param>
    private void CreateFilterButton(FactionType.TYPE factionType)
    {
        GameObject btnObj = Instantiate(m_researchFilterBtnPrefeb, m_researchFilterScrollViewContentTrans);
        ResearchFilterBtn filterBtn = btnObj.GetComponent<ResearchFilterBtn>();
        
        if (filterBtn != null)
        {
            filterBtn.Init(factionType, this);
        }
        else
        {
            Debug.LogError("ResearchFilterBtn component not found on prefab!");
        }
    }

    /// <summary>
    /// 필터 버튼들 제거
    /// </summary>
    private void ClearFilterButtons()
    {
        foreach (Transform item in m_researchFilterScrollViewContentTrans)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
    }

    /// <summary>
    /// 전체 연구 필터링
    /// </summary>
    public void FilterByAllResearch()
    {
        m_showAllResearch = true;
        
        // 현재 선택된 패널 인덱스로 다시 필터링하여 표시
        SelectResearchContent(m_currentPanelIndex);
    }

    /// <summary>
    /// 팩션별 필터링
    /// </summary>
    /// <param name="factionType">필터할 팩션 타입</param>
    public void FilterByFaction(FactionType.TYPE factionType)
    {
        m_showAllResearch = false;
        m_currentFilter = factionType;
        
        // 현재 선택된 패널 인덱스로 다시 필터링하여 표시
        SelectResearchContent(m_currentPanelIndex);
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

        m_currentPanelIndex = argPanelIndex;
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
        if (m_showAllResearch)
        {
            // 전체 연구: 모든 팩션의 연구 표시
            foreach (var factionEntry in m_gameDataManager.FactionEntryDict)
            {
                if (factionEntry.Value != null)
                {
                    CreateResearchButtonsForFaction(factionEntry.Value, panelIndex);
                }
            }
        }
        else
        {
            // 특정 팩션 필터: 해당 팩션의 연구만 표시
            var factionEntry = m_gameDataManager.GetFactionEntry(m_currentFilter);
            if (factionEntry != null)
            {
                CreateResearchButtonsForFaction(factionEntry, panelIndex);
            }
        }
    }

    /// <summary>
    /// 특정 팩션의 연구 버튼들 생성
    /// </summary>
    /// <param name="factionEntry">팩션 엔트리</param>
    /// <param name="panelIndex">패널 인덱스</param>
    private void CreateResearchButtonsForFaction(FactionEntry factionEntry, int panelIndex)
    {
        if (factionEntry?.m_data?.m_research != null)
        {
            foreach (var researchData in factionEntry.m_data.m_research)
            {
                if (researchData == null) continue;

                var researchEntry = GameDataManager.Instance.GetResearchEntry(researchData.m_code);
                if (researchEntry == null) continue;

                bool shouldCreate = ShouldCreateResearchButton(researchData, researchEntry.m_state, panelIndex);
                if (shouldCreate)
                {
                    CreateResearchButton(researchData, researchEntry.m_state, factionEntry);
                }
            }
        }
    }

    /// <summary>
    /// 연구 버튼 생성 여부 판단
    /// </summary>
    /// <param name="researchData">연구 데이터</param>
    /// <param name="researchState">연구 상태</param>
    /// <param name="panelIndex">패널 인덱스</param>
    /// <returns>생성 여부</returns>
    private bool ShouldCreateResearchButton(FactionResearchData researchData, FactionResearchState researchState, int panelIndex)
    {
        switch (panelIndex)
        {
            case 0: // 연구 가능 (잠금 해제되었고, 시작되지 않은 경우)
                return !researchState.m_isLocked && 
                       researchState.IsNotStarted;
            case 1: // 잠금
                return researchState.m_isLocked;
            case 2: // 완료됨
                return researchState.IsCompleted;
            default:
                Debug.LogError(ExceptionMessages.ErrorInvalidResearchInfo);
                return false;
        }
    }

    /// <summary>
    /// 개별 연구 버튼 생성
    /// </summary>
    /// <param name="researchData">연구 데이터</param>
    /// <param name="researchState">연구 상태</param>
    /// <param name="factionEntry">팩션 엔트리</param>
    private void CreateResearchButton(FactionResearchData researchData, FactionResearchState researchState, FactionEntry factionEntry)
    {
        GameObject btnObj = Instantiate(m_commonResearchBtnPrefeb, m_researchScrollViewContentTrans);
        ResearchBtn researchBtn = btnObj.GetComponent<ResearchBtn>();
        
        if (researchBtn != null)
        {
            researchBtn.Initialize(this, researchData, researchState, factionEntry);
        }
        else
        {
            Debug.LogError("ResearchBtn component not found on prefab!");
        }
    }
}

