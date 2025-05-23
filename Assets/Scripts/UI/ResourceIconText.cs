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

    public void Initialize(ResourceType argResourceType, long argBaseAmount, long? argChangeAmount = null)
    {
        if (m_icon != null)
            m_icon.sprite = GameManager.Instance.GameDataManager.GetResourceIcon(argResourceType);

        if (m_text != null)
        {
            m_text.text = NumberFormatter.FormatNumber(argBaseAmount);
            m_text.color = Color.black;
        }
        
        if (m_changeText != null)
        {
            if (argChangeAmount.HasValue && argChangeAmount.Value != 0)
            {
                long change = argChangeAmount.Value;
                m_changeText.text = (change > 0 ? "+" : "") + change;
                m_changeText.color = change > 0 ? Color.green : Color.red;
                m_changeText.gameObject.SetActive(true);
            }
            else
            {
                m_changeText.gameObject.SetActive(false);
            }
        }
    }


}
