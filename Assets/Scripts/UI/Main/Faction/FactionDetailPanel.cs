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
    TextMeshProUGUI m_nameTMP = null;
    [SerializeField]
    TextMeshProUGUI m_friendlinessTMP = null;
    [SerializeField]
    TextMeshProUGUI m_traitTMP = null;

    [SerializeField]
    Transform m_uniqeResearchContentTrans = null;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(FactionData argFactionData)
    {
        foreach (Transform item in m_uniqeResearchContentTrans)
        {
            Destroy(item.gameObject);
        }

        m_illustrationImage.sprite = argFactionData.m_illustration;
        m_iconImage.sprite = argFactionData.m_icon;

        m_nameTMP.text = argFactionData.m_name;
        m_traitTMP.text = argFactionData.m_traitDescription;

        foreach(ResearchData item in argFactionData.m_uniqueResearch)
        {
            Instantiate(m_factionResearchBtnPrefeb, m_uniqeResearchContentTrans).GetComponent<FactionResearchBtn>().
                Setting(item.m_factionType, item.m_name, item.m_description);
        }

        gameObject.SetActive(true);
    }
}
