using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPanel : MonoBehaviour, IUIPanel
{
    [SerializeField]
    GameObject m_buildingBtnPrefeb = null;
    [SerializeField]
    Transform m_buildingScrollViewContentTrans = null;

    private GameDataManager m_gameDataManager = null;
    private MainUIManager m_mainUIManager = null;
    private List<BuildingBtn> m_bulidingBtnList = new List<BuildingBtn>();

    public MainUIManager MainUIManager => m_mainUIManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(GameDataManager argDataManager, MainUIManager argUIManager)
    {
        m_gameDataManager = argDataManager;
        m_mainUIManager = argUIManager;

        gameObject.SetActive(true);

        if(m_buildingScrollViewContentTrans.childCount != m_gameDataManager.BuildingEntryDict.Count)
        {
            foreach (Transform item in m_buildingScrollViewContentTrans)
            {
                Destroy(item.gameObject);
            }
        }
        else
        {
            return;
        }

        foreach (KeyValuePair<string,BuildingEntry> item in m_gameDataManager.BuildingEntryDict)
        {
            GameObject _buildingBtnObj = Instantiate(m_buildingBtnPrefeb, m_buildingScrollViewContentTrans);
            BuildingBtn _buildingBtn = _buildingBtnObj.GetComponent<BuildingBtn>();

            m_bulidingBtnList.Add(_buildingBtn);
            _buildingBtn.Initialize(this, item.Value);
        }
    }

    public void OnClose()
    {

    }
}
