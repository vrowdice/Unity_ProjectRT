using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactionResearchBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;

    private FactionType m_factionType;
    private FactionPanel m_factionPanel = null;

    public void Setting(FactionType argFactionType, string argName, string argExp)
    {
        m_factionType = argFactionType;
        m_nameText.text = argName;
        m_expText.text = argExp;
    }
}
