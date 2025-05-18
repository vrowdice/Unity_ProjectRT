using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FactionResearchBtn : MonoBehaviour
{
    [SerializeField]
    Image m_iconImage = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;

    private FactionType m_factionType;
    private FactionPanel m_factionPanel = null;

    public void Setting(FactionType argFactionType, Sprite argIcon, string argName, string argExp)
    {
        m_factionType = argFactionType;
        m_iconImage.sprite = argIcon;
        m_nameText.text = argName;
        m_expText.text = argExp;
    }
}
