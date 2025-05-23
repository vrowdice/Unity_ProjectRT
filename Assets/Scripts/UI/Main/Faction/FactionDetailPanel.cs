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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(FactionEntry argFactionEntry)
    {
        foreach (Transform item in m_uniqeResearchContentTrans)
        {
            Destroy(item.gameObject);
        }

        m_illustrationImage.sprite = argFactionEntry.m_data.m_illustration;
        m_iconImage.sprite = argFactionEntry.m_data.m_icon;

        m_nameText.text = argFactionEntry.m_data.m_name;
        m_traitText.text = argFactionEntry.m_data.m_traitDescription;

        foreach(ResearchData item in argFactionEntry.m_data.m_uniqueResearch)
        {
            Instantiate(m_factionResearchBtnPrefeb, m_uniqeResearchContentTrans).GetComponent<FactionResearchBtn>().
                Initialize(item.m_factionType, item.m_icon, item.m_name, item.m_description);
        }

        gameObject.SetActive(true);
    }
}
