using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RequestBtn : MonoBehaviour
{
    [SerializeField]
    Image m_clientImage = null;
    [SerializeField]
    Image m_typeImage = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;
    [SerializeField]
    TextMeshProUGUI m_completeRewardText = null;
    [SerializeField]
    TextMeshProUGUI m_completeConditionText = null;

    string m_code = "00001";
    RequestPanel m_requestPanel = null;

    public void Initialize(RequestPanel argPanel, string argCode, Sprite argClientImage, string argName, string argExp)
    {
        m_requestPanel = argPanel;
        m_code = argCode;
        m_clientImage.sprite = argClientImage;
        m_nameText.text = argName;
        m_expText.text = argExp;
    }

    public void Click()
    {
        m_requestPanel.OpenRequestDetailPanel(m_code);
    }
}
