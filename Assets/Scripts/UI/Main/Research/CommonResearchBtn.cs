using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CommonResearchBtn : MonoBehaviour
{
    [SerializeField]
    Image m_iconImage = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;

    private string m_researchCode;
    private FactionPanel m_factionPanel = null;

    public void Setting(string argResearchCode, Sprite argIcon, string argName, string argExp)
    {
        m_researchCode = argResearchCode;
        m_iconImage.sprite = argIcon;
        m_nameText.text = argName;
        m_expText.text = argExp;
    }
}
