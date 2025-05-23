using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RequestBtn : MonoBehaviour
{
    [SerializeField]
    Image m_iconImage = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;

    //ÀÇ·Ú °èÈ¹ÀÌ È®½ÇÈ÷ µÇ¸é ÇÁ¸®º£ÀÕÀ¸·Î ¹Ù²Ü°Í
    public string m_code = "00001";
    public RequestPanel m_requestPanel = null;

    public void Initialize(RequestPanel argPanel, string argCode, Sprite argIcon, string argName, string argExp)
    {
        m_requestPanel = argPanel;
        m_code = argCode;
        m_iconImage.sprite = argIcon;
        m_nameText.text = argName;
        m_expText.text = argExp;
    }

    public void Click()
    {
        m_requestPanel.OpenRequestDetailPanel(m_code);
    }
}
