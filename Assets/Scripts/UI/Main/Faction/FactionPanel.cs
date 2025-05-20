using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionPanel : MonoBehaviour, IUIPanel
{
    [SerializeField]
    GameObject m_factionInfoBtnPrefeb = null;
    [SerializeField]
    Transform m_factionScrollViewContentTrans = null;
    [SerializeField]
    FactionDetailPanel m_factionDetailPanel = null;

    private GameDataManager m_gameDataManager = null;

    // Start is called before the first frame update
    void Start()
    {
        m_gameDataManager = GameManager.Instance.GameDataManager;

        foreach (KeyValuePair<FactionType, FactionEntry> item in m_gameDataManager.FactionDataDict)
        {
            Instantiate(m_factionInfoBtnPrefeb, m_factionScrollViewContentTrans).GetComponent<FactionInfoBtn>().
                Setting(item.Value.m_data.m_factionType, item.Value.m_data.m_name, item.Value.m_data.m_icon, this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;
        gameObject.SetActive(true);
    }

    public void OnClose()
    {

    }

    public void OpenFactionDetailPanel(FactionType argFactionType)
    {
        m_factionDetailPanel.OnOpen(m_gameDataManager.FactionDataDict[argFactionType]);
    }
}
