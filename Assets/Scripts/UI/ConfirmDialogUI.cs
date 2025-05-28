using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmDialogUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_messageText;
    [SerializeField]
    Button m_yesButton;
    [SerializeField]
    Button m_noButton;

    public void Setup(string message, Action onYes)
    {
        m_messageText.text = message;

        m_yesButton.onClick.AddListener(() =>
        {
            onYes?.Invoke();
            Destroy(gameObject);
        });

        m_noButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }
}
