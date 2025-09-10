using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DifficultyBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_text;

    BalanceType.TYPE m_type;
    private NewStartPanel m_newStartPanel = null;

    public void Init(BalanceType.TYPE argBalanceType, NewStartPanel argNewStartPanel)
    {
        m_type = argBalanceType;
        m_newStartPanel = argNewStartPanel;

        m_text.text = m_type.ToString();
    }

    public void OnClick()
    {
        // 난이도 적용
        GameDataManager.Instance.GameBalanceEntry.m_state.m_mainMul =
         GameDataManager.Instance.GameBalanceEntry.m_data.GetBalanceTypeBalance(m_type).m_mul;

        // 바로 SelectFactionPanel로 넘어가기
        if (m_newStartPanel != null)
        {
            m_newStartPanel.ShowFactionSelection();
        }
    }
}
