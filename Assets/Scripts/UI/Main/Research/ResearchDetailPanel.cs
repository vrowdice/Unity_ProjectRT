using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResearchDetailPanel : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_researchNameText = null;
    [SerializeField]
    TextMeshProUGUI m_researchDescriptionText = null;
    [SerializeField]
    TextMeshProUGUI m_researchCostText = null;
    [SerializeField]
    TextMeshProUGUI m_researchDurationText = null;
    [SerializeField]
    Button m_researchPanelBtn = null;
    [SerializeField]
    TextMeshProUGUI m_researchPanelBtnText = null;
    
    private FactionResearchEntry m_currentResearchEntry = null;
    private ResearchPanel m_researchPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        // 버튼 클릭 이벤트 등록
        if (m_researchPanelBtn != null)
        {
            m_researchPanelBtn.onClick.AddListener(OnResearchButtonClick);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 연구 상세 정보 설정
    /// </summary>
    /// <param name="researchEntry">연구 항목 데이터</param>
    /// <param name="researchPanel">연구 패널 참조</param>
    public void SetResearchDetail(FactionResearchEntry researchEntry, ResearchPanel researchPanel)
    {
        m_currentResearchEntry = researchEntry;
        m_researchPanel = researchPanel;

        if (researchEntry == null || researchEntry.m_data == null) return;

        // 기본 정보 설정
        if (m_researchNameText != null)
        {
            m_researchNameText.text = researchEntry.m_data.m_name;
        }

        if (m_researchDescriptionText != null)
        {
            m_researchDescriptionText.text = researchEntry.m_data.m_description;
        }

        if (m_researchCostText != null)
        {
            m_researchCostText.text = $"Cost: {researchEntry.m_data.m_cost}";
        }

        if (m_researchDurationText != null)
        {
            int days = Mathf.FloorToInt(researchEntry.m_data.m_duration);
            m_researchDurationText.text = $"Duration: {days} days";
        }

        // 연구 상태에 따른 버튼 설정
        UpdateButtonByResearchState();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 연구 상태에 따른 버튼 업데이트
    /// </summary>
    private void UpdateButtonByResearchState()
    {
        if (m_researchPanelBtn == null || m_researchPanelBtnText == null) return;

        if (m_currentResearchEntry == null)
        {
            m_researchPanelBtn.gameObject.SetActive(false);
            return;
        }

        FactionResearchState state = m_currentResearchEntry.m_state;

        if (state.m_isLocked)
        {
            // 잠금 상태: 버튼 비활성화
            m_researchPanelBtn.gameObject.SetActive(false);
        }
        else if (state.IsCompleted)
        {
            // 완료 상태: 버튼 비활성화
            m_researchPanelBtn.gameObject.SetActive(false);
        }
        else if (state.IsInProgress)
        {
            // 연구 중 상태: 연구 중단 버튼
            m_researchPanelBtn.gameObject.SetActive(true);
            m_researchPanelBtnText.text = "Cancel Research";
        }
        else
        {
            // 연구 가능 상태: 연구 시작 버튼
            m_researchPanelBtn.gameObject.SetActive(true);
            m_researchPanelBtnText.text = "Start Research";
        }
    }

    /// <summary>
    /// 연구 버튼 클릭 시 호출
    /// </summary>
    private void OnResearchButtonClick()
    {
        if (m_currentResearchEntry == null) return;

        FactionResearchState state = m_currentResearchEntry.m_state;

        if (state.m_isLocked || state.IsCompleted)
        {
            // 잠금 또는 완료 상태에서는 아무 동작 안함
            return;
        }
        else if (state.IsInProgress)
        {
            // 연구 중 상태: 연구 중단
            CancelResearch();
        }
        else
        {
            // 연구 가능 상태: 연구 시작
            StartResearch();
        }

        // 버튼 클릭 후 창 닫기는 각 동작 성공 시점에서 처리
    }

    /// <summary>
    /// 연구 시작
    /// </summary>
    private void StartResearch()
    {
        if (m_currentResearchEntry == null) return;

        // 연구력 차감 (GameManager에서 처리)
        if (m_researchPanel != null && GameManager.Instance != null)
        {
            // 연구 비용을 Tech 리소스로 차감
            bool canStartResearch = GameManager.Instance.TryChangeResource(ResourceType.TYPE.Tech, -m_currentResearchEntry.m_data.m_cost);
            
            if (canStartResearch)
            {
                // 연구 시작 (진행도 0으로 시작)
                m_currentResearchEntry.m_state.m_progress = 0; // 연구 시작
                
                // 연구 중인 항목 목록 업데이트
                m_researchPanel.UpdateInprogressResearch();
                
                // 연구 가능 탭 재구성 (현재 선택된 탭이 연구 가능 탭인 경우)
                m_researchPanel.SelectResearchContent(0);
                
                // 버튼 상태 업데이트
                UpdateButtonByResearchState();

                // 상세 패널 닫기
                gameObject.SetActive(false);
                
                Debug.Log($"Research started: {m_currentResearchEntry.m_data.m_name}");
            }
            else
            {
                Debug.Log("Not enough research points!");
                // 여기에 연구력 부족 알림 UI 표시
            }
        }
    }

    /// <summary>
    /// 연구 중단 (80% 환불 확인 다이얼로그)
    /// </summary>
    private void CancelResearch()
    {
        if (m_currentResearchEntry == null) return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is null!");
            return;
        }

        FactionResearchState state = m_currentResearchEntry.m_state;
        if (state == null || !state.IsInProgress)
        {
            return;
        }

        int cost = m_currentResearchEntry.m_data.m_cost;
        long refundAmount = Mathf.FloorToInt(cost * 0.8f);
        string message = $"Cancelling research will only refund 80%({refundAmount}) of the research cost.\nDo you want to proceed?";

        GameManager.Instance.ShowConfirmDialog(message, () =>
        {
            // 80% 환불
            GameManager.Instance.TryChangeResource(ResourceType.TYPE.Tech, refundAmount);

            // 연구 진행도 초기화 (미시작 상태로)
            state.m_progress = -1;
            state.m_isResearched = false;

            // 연구 중인 항목 목록에서 제거 및 UI 갱신
            if (m_researchPanel != null)
            {
                m_researchPanel.UpdateInprogressResearch();
                m_researchPanel.SelectResearchContent(0);
            }

                        // 버튼 상태 업데이트
            UpdateButtonByResearchState();

            // 상세 패널 닫기
            gameObject.SetActive(false);
            
            Debug.Log($"Research cancelled with 80% refund: {m_currentResearchEntry.m_data.m_name}");
        });
    }

    /// <summary>
    /// 패널 초기화
    /// </summary>
    public void ClearDetail()
    {
        m_currentResearchEntry = null;
        m_researchPanel = null;
        
        if (m_researchNameText != null) m_researchNameText.text = "";
        if (m_researchDescriptionText != null) m_researchDescriptionText.text = "";
        if (m_researchCostText != null) m_researchCostText.text = "";
        if (m_researchDurationText != null) m_researchDurationText.text = "";
        
        if (m_researchPanelBtn != null)
        {
            m_researchPanelBtn.gameObject.SetActive(false);
        }
    }
}
