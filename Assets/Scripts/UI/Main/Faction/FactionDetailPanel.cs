using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactionDetailPanel : MonoBehaviour
{
    [SerializeField]
    GameObject m_factionResearchBtnPrefeb = null;

    [SerializeField]
    Image m_illustrationImage = null;
    [SerializeField]
    Image m_iconImage = null;

    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_friendlinessText = null;
    [SerializeField]
    TextMeshProUGUI m_traitText = null;

    [SerializeField]
    Transform m_uniqeResearchContentTrans = null;

    public void OnOpen(FactionPanel argFactionPanel, FactionEntry argFactionEntry)
    {
        foreach (Transform item in m_uniqeResearchContentTrans)
        {
            Destroy(item.gameObject);
        }

        m_illustrationImage.sprite = argFactionEntry.m_data.m_illustration;
        m_iconImage.sprite = argFactionEntry.m_data.m_icon;

        m_nameText.text = argFactionEntry.m_data.m_name;
        m_traitText.text = argFactionEntry.m_data.m_traitDescription;

        m_friendlinessText.text = argFactionEntry.m_state.m_like.ToString();

        foreach (FactionResearchData item in argFactionEntry.m_data.m_research)
        {
            Instantiate(m_factionResearchBtnPrefeb, m_uniqeResearchContentTrans).GetComponent<FactionResearchBtn>().
                Initialize(argFactionPanel, item.m_factionType, item.m_icon, item.m_name, item.m_description);
        }

        gameObject.SetActive(true);
    }
}
