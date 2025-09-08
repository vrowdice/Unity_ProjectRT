using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DifficultyBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_text;

    BalanceType.TYPE m_type;

    public void Init(BalanceType.TYPE argBalanceType)
    {
        m_type = argBalanceType;

        m_text.text = m_type.ToString();
    }

    public void OnClick()
    {
        GameDataManager.Instance.GameBalanceEntry.m_state.m_mainMul =
         GameDataManager.Instance.GameBalanceEntry.m_data.GetBalanceTypeBalance(m_type).m_mul;
    }
}
