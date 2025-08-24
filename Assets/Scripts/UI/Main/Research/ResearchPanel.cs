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
    private FactionType.TYPE m_currentFilter = FactionType.TYPE.None;  // 현재 선택된 필터 (None = 모든 연구)
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

        // "전체" 버튼 생성 (None 타입으로)
        CreateFilterButton(FactionType.TYPE.None);

        // 모든 팩션에 대해 필터 버튼 생성
        foreach (var factionEntry in GameDataManager.Instance.FactionEntryDict)
        {
            // 해당 팩션에 고유 연구가 있는지 확인
            if (factionEntry.Value?.m_data?.m_uniqueResearch != null && 
                factionEntry.Value.m_data.m_uniqueResearch.Count > 0)
            {
                CreateFilterButton(factionEntry.Key);
            }
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
    /// 팩션별 필터링
    /// </summary>
    /// <param name="factionType">필터할 팩션 타입 (None = 전체)</param>
    public void FilterByFaction(FactionType.TYPE factionType)
    {
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
        if (m_currentFilter == FactionType.TYPE.None)
        {
            // "All" 필터: Common 연구와 모든 팩션의 고유 연구 표시
            
            // 1. Common 연구 표시
            foreach (KeyValuePair<string, ResearchEntry> item in GameDataManager.Instance.CommonResearchEntryDict)
            {
                if (item.Value == null) continue;

                bool shouldCreate = ShouldCreateResearchButton(item.Value, panelIndex);
                if (shouldCreate)
                {
                    CreateResearchButton(item.Value);
                }
            }

            // 2. 모든 팩션의 고유 연구 표시
            foreach (var factionEntry in GameDataManager.Instance.FactionEntryDict)
            {
                if (factionEntry.Value?.m_data?.m_uniqueResearch != null)
                {
                    foreach (var researchData in factionEntry.Value.m_data.m_uniqueResearch)
                    {
                        if (researchData == null) continue;

                        // ResearchData를 ResearchEntry로 변환 (임시로 생성)
                        var tempEntry = new ResearchEntry(researchData);
                        bool shouldCreate = ShouldCreateResearchButton(tempEntry, panelIndex);

                        if (shouldCreate)
                        {
                            CreateResearchButton(tempEntry);
                        }
                    }
                }
            }
        }
        else
        {
            // 특정 팩션 필터: 해당 팩션의 고유 연구만 표시
            var factionEntry = GameDataManager.Instance.GetFactionEntry(m_currentFilter);
            if (factionEntry?.m_data?.m_uniqueResearch != null)
            {
                foreach (var researchData in factionEntry.m_data.m_uniqueResearch)
                {
                    if (researchData == null) continue;

                    // ResearchData를 ResearchEntry로 변환 (임시로 생성)
                    var tempEntry = new ResearchEntry(researchData);
                    bool shouldCreate = ShouldCreateResearchButton(tempEntry, panelIndex);

                    if (shouldCreate)
                    {
                        CreateResearchButton(tempEntry);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 연구 버튼 생성 여부 판단
    /// </summary>
    /// <param name="researchEntry">연구 항목</param>
    /// <param name="panelIndex">패널 인덱스</param>
    /// <returns>생성 여부</returns>
    private bool ShouldCreateResearchButton(ResearchEntry researchEntry, int panelIndex)
    {
        switch (panelIndex)
        {
            case 0: // 연구 가능 (잠금 해제되었고, 시작되지 않은 경우)
                return !researchEntry.m_state.m_isLocked && 
                       researchEntry.m_state.IsNotStarted;
            case 1: // 잠금
                return researchEntry.m_state.m_isLocked;
            case 2: // 완료됨
                return researchEntry.m_state.IsCompleted;
            default:
                Debug.LogError(ExceptionMessages.ErrorInvalidResearchInfo);
                return false;
        }
    }

    /// <summary>
    /// 개별 연구 버튼 생성
    /// </summary>
    /// <param name="researchEntry">연구 항목</param>
    private void CreateResearchButton(ResearchEntry researchEntry)
    {
        GameObject btnObj = Instantiate(m_commonResearchBtnPrefeb, m_researchScrollViewContentTrans);
        ResearchBtn researchBtn = btnObj.GetComponent<ResearchBtn>();
        
        if (researchBtn != null)
        {
            researchBtn.Initialize(this, researchEntry);
        }
        else
        {
            Debug.LogError("ResearchBtn component not found on prefab!");
        }
    }
}
