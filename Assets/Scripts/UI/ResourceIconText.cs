using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceIconText : MonoBehaviour
{
    [SerializeField]
    Image m_icon = null;

    [SerializeField]
    TextMeshProUGUI m_text = null;
    [SerializeField]
    TextMeshProUGUI m_changeText = null;

    //각각 초기화 필요
    //해당 함수 선행 실행 후 변경 택스트 실행
    public void InitializeMainText(ResourceType argResourceType, long argBaseAmount)
    {
        if (m_icon != null)
            m_icon.sprite = GameManager.Instance.GameDataManager.GetResourceIcon(argResourceType);

        if (m_text != null)
        {
            m_text.text = NumberFormatter.FormatNumber(argBaseAmount);
            m_text.color = Color.black;
        }

        m_changeText.gameObject.SetActive(false);
    }

    public void InitializeChangeText(long argChangeAmount, bool argIsShowColor)
    {
        long change = argChangeAmount;
        m_changeText.text = (change > 0 ? "+" : "") + change;

        if (argIsShowColor)
            m_changeText.color = change > 0 ? Color.green : Color.red;
        else
            m_changeText.color = Color.black;

        m_changeText.gameObject.SetActive(true);
    }
}
