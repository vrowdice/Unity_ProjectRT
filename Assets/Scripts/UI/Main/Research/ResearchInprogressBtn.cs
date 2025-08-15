using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchInprogressBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_timeText = null;
    [SerializeField]
    Image m_iconImage = null;
    [SerializeField]
    Slider m_progressSlider = null;

    private ResearchPanel m_researchPanel = null;
    private ResearchEntry m_researchEntry = null;
    private float m_researchTime = 0f;
    private float m_totalResearchTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateResearchProgress();
    }

    /// <summary>
    /// 연구 중인 버튼 초기화
    /// </summary>
    /// <param name="researchPanel">연구 패널 참조</param>
    /// <param name="researchEntry">연구 항목 데이터</param>
    public void Initialize(ResearchPanel researchPanel, ResearchEntry researchEntry)
    {
        m_researchPanel = researchPanel;
        m_researchEntry = researchEntry;

        if (m_researchEntry != null && m_researchEntry.m_data != null)
        {
            // 연구 이름 설정
            if (m_nameText != null)
            {
                m_nameText.text = m_researchEntry.m_data.m_name;
            }

            // 연구 아이콘 설정 (있는 경우)
            if (m_iconImage != null && m_researchEntry.m_data.m_icon != null)
            {
                m_iconImage.sprite = m_researchEntry.m_data.m_icon;
            }

            // 연구 시간 설정
            m_totalResearchTime = m_researchEntry.m_data.m_duration;
            m_researchTime = m_totalResearchTime - m_researchEntry.m_state.m_progress;

            // 진행도 슬라이더 초기화
            UpdateProgressDisplay();
        }
    }

    /// <summary>
    /// 연구 항목 데이터 반환
    /// </summary>
    /// <returns>연구 항목 데이터</returns>
    public ResearchEntry GetResearchEntry()
    {
        return m_researchEntry;
    }

    /// <summary>
    /// 연구 진행도 업데이트
    /// </summary>
    public void UpdateProgress()
    {
        UpdateProgressDisplay();
    }

    /// <summary>
    /// 연구 진행도 표시 업데이트
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (m_researchEntry == null) return;

        // 진행도 계산 (m_progress가 -1이면 0으로 처리)
        float actualProgress = Mathf.Max(0, m_researchEntry.m_state.m_progress);
        float progress = actualProgress / m_totalResearchTime;
        progress = Mathf.Clamp01(progress);

        // 슬라이더 업데이트
        if (m_progressSlider != null)
        {
            m_progressSlider.value = progress;
        }

        // 남은 시간 계산 및 표시 (m_progress가 -1이면 전체 시간으로 계산)
        float remainingTime = m_totalResearchTime - actualProgress;
        remainingTime = Mathf.Max(0, remainingTime);

        if (m_timeText != null)
        {
            if (remainingTime <= 0)
            {
                m_timeText.text = "Complete";
            }
            else
            {
                int days = Mathf.FloorToInt(remainingTime);
                m_timeText.text = $"{days} days remaining";
            }
        }
    }

    /// <summary>
    /// 연구 진행도 업데이트 (실시간)
    /// </summary>
    private void UpdateResearchProgress()
    {
        if (m_researchEntry == null || m_researchEntry.m_state.m_isResearched) return;

        // 연구가 진행 중인지 확인 (실제 게임 로직에 따라 조정 필요)
        // 여기서는 예시로 시간에 따른 진행도를 업데이트
        if (m_researchEntry.m_state.m_progress < m_totalResearchTime)
        {
            // 실제 구현에서는 게임 매니저나 연구 매니저에서 진행도를 업데이트해야 함
            // 이 부분은 게임의 연구 시스템에 따라 조정이 필요합니다
        }

        UpdateProgressDisplay();
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (연구 상세 패널 열기)
    /// </summary>
    public void OnButtonClick()
    {
        if (m_researchPanel != null && m_researchEntry != null)
        {
            m_researchPanel.OpenResearchDetailPanel(m_researchEntry);
        }
    }
}
