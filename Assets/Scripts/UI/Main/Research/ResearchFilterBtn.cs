using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchFilterBtn : MonoBehaviour
{
    [SerializeField]
    Image m_factionImage = null;
    [SerializeField]
    TextMeshProUGUI m_text = null;

    FactionType.TYPE m_factionType;
    ResearchPanel m_researchPanel = null;

    public void Init(FactionType.TYPE argType, ResearchPanel argParentPanel)
    {
        m_factionType = argType;
        m_researchPanel = argParentPanel;

        // 일반 연구 버튼인 경우 (None 타입)
        if (argType == FactionType.TYPE.None)
        {
            if (m_factionImage != null)
            {
                m_factionImage.sprite = null; // 또는 기본 아이콘
            }
            
            if (m_text != null)
            {
                m_text.text = "Normal";
            }
        }
        else
        {
            // GameDataManager에서 직접 팩션 데이터 가져오기
            var factionEntry = GameDataManager.Instance.GetFactionEntry(argType);
            if (factionEntry != null)
            {
                // 팩션 아이콘 설정
                if (m_factionImage != null)
                {
                    m_factionImage.sprite = factionEntry.m_data.m_icon;
                }
                
                // 팩션 이름 설정
                if (m_text != null)
                {
                    m_text.text = factionEntry.m_data.m_name;
                }
            }
            else
            {
                Debug.LogWarning($"Faction data not found for type: {argType}");
            }
        }
    }

    public void InitForAllResearch(ResearchPanel argParentPanel)
    {
        m_researchPanel = argParentPanel;
        
        if (m_factionImage != null)
        {
            m_factionImage.sprite = null; // 또는 전체 아이콘
        }
        
        if (m_text != null)
        {
            m_text.text = "All";
        }
        
        // 전체 연구용 특별 값 설정
        m_factionType = FactionType.TYPE.None; // 임시로 None 사용
    }

    public void OnClick()
    {
        // 클릭 시 해당 팩션으로 필터링
        if (m_researchPanel != null)
        {
            // "All" 버튼인지 확인 (텍스트로 구분)
            if (m_text != null && m_text.text == "All")
            {
                m_researchPanel.FilterByAllResearch();
            }
            else
            {
                m_researchPanel.FilterByFaction(m_factionType);
            }
        }
    }
}
