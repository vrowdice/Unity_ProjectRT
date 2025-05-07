using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactionInfoBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    Image m_image = null;

    private FactionType m_factionType;
    private FactionPanel m_factionPanel = null;

    public void Setting(FactionType argFactionType, string argName, Sprite argSprite, FactionPanel argPanel)
    {
        m_factionType = argFactionType;
        m_nameText.text = argName;
        m_image.sprite = argSprite;
        m_factionPanel = argPanel;
    }

    public void Click()
    {
        m_factionPanel.OpenFactionDetailPanel(m_factionType);
    }
}
