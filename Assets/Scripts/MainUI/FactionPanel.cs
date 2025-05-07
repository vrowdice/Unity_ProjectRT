using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionPanel : MonoBehaviour, IUIPanel
{
    [SerializeField]
    GameObject m_factionInfoBtnPrefeb = null;
    [SerializeField]
    Transform m_factionScrollViewContent = null;


    GameDataManager m_gameDataManager = null;

    // Start is called before the first frame update
    void Start()
    {
        m_gameDataManager = GameManager.Instance.GameDataManager;

        foreach(FactionData item in m_gameDataManager.FactionData)
        {
            Instantiate(m_factionInfoBtnPrefeb, m_factionScrollViewContent).GetComponent<FactionInfoBtn>().
                Setting(item.name, item.m_icon);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnOpen()
    {
        
    }

    public void OnClose()
    {

    }
}
