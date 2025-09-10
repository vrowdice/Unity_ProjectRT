using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewStartPanel : BasePanel
{
    [Header("NewStartPanel")]
    [SerializeField]
    SelectFactionPanel m_selectFacitonPanel = null;

    [SerializeField]
    GameObject m_difficultyBtnPrefab;
    
    [SerializeField]
    Transform m_selectDifficultyScrollViewContent;

    [SerializeField]
    GameObject m_factionSelectionPanel = null;

    protected override void OnPanelOpen()
    {
        // 난이도 선택 화면 표시
        ShowDifficultySelection();

        // 기존 버튼들 정리
        ClearDifficultyButtons();

        // 난이도 버튼들 생성
        if (m_difficultyBtnPrefab == null || m_selectDifficultyScrollViewContent == null)
        {
            Debug.LogError("NewStartPanel: References are not assigned.");
            return;
        }

        foreach (var item in EnumUtils.GetAllEnumValues<BalanceType.TYPE>())
        {
            var btnGO = Instantiate(m_difficultyBtnPrefab, m_selectDifficultyScrollViewContent);
            if (btnGO.TryGetComponent<DifficultyBtn>(out var btn))
            {
                btn.Init(item, this); // NewStartPanel 참조 전달
            }
            else
            {
                Debug.LogError("NewStartPanel: DifficultyBtn component missing on prefab.");
            }
        }
    }

    protected override void OnPanelClose()
    {
        ClearDifficultyButtons();
        
        // 팩션 패널도 닫기
        if (m_selectFacitonPanel != null)
        {
            m_selectFacitonPanel.gameObject.SetActive(false);
        }
    }

    public void ShowFactionSelection()
    {
        if (m_factionSelectionPanel != null)
            m_factionSelectionPanel.SetActive(true);

        // SelectFactionPanel 초기화
        if (m_selectFacitonPanel != null)
        {
            m_selectFacitonPanel.Init();
        }
    }

    private void ShowDifficultySelection()
    {
        if (m_factionSelectionPanel != null)
            m_factionSelectionPanel.SetActive(false);
    }

    private void ClearDifficultyButtons()
    {
        foreach (Transform child in m_selectDifficultyScrollViewContent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
