using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionPanel : BasePanel
{
    [SerializeField]
    GameObject m_factionInfoBtnPrefeb = null;
    [SerializeField]
    Transform m_factionScrollViewContentTrans = null;
    [SerializeField]
    FactionDetailPanel m_factionDetailPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        m_gameDataManager = GameDataManager.Instance;

        foreach (KeyValuePair<FactionType.TYPE, FactionEntry> item in m_gameDataManager.FactionEntryDict)
        {
            Instantiate(m_factionInfoBtnPrefeb, m_factionScrollViewContentTrans).GetComponent<FactionInfoBtn>().
                Initialize(item.Value.m_data.m_factionType, item.Value.m_data.m_name, item.Value.m_data.m_icon, this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Faction");
        SetBuildingLevel(""); // 팩션 패널은 레벨이 필요 없음
    }

    public void OpenFactionDetailPanel(FactionType.TYPE argFactionType)
    {
        m_factionDetailPanel.OnOpen(this ,m_gameDataManager.FactionEntryDict[argFactionType]);
    }
}
