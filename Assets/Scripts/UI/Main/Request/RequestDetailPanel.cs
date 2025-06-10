using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequestDetailPanel : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_titleText = null;
    [SerializeField]
    TextMeshProUGUI m_descriptionText = null;
    [SerializeField]
    TextMeshProUGUI m_rewardText = null;

    private RequestPanel m_requestPanel;

    public void OnOpen(RequestPanel argRequestPanel, bool argIsAcceptable, int argRequestIndex)
    {
        m_requestPanel = argRequestPanel;

        m_rewardText.text = "";

        gameObject.SetActive(true);
    }
}
