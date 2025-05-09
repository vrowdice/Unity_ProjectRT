using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CommonResearchBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;

    private string m_researchCode;
    private FactionPanel m_factionPanel = null;

    public void Setting(string argResearchCode, string argName, string argExp)
    {
        m_researchCode = argResearchCode;
        m_nameText.text = argName;
        m_expText.text = argExp;
    }
}
