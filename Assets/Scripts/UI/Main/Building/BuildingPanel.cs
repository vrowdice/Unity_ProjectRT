using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPanel : MonoBehaviour, IUIPanel
{
    [SerializeField]
    GameObject m_buildingBtnPrefeb = null;
    [SerializeField]
    Transform m_buildingScrollViewContentTrans = null;
    [SerializeField]
    Transform m_addResourceContentTrans = null;
    [SerializeField]
    Transform m_requiredResourceContentTrans = null;

    private GameDataManager m_gameDataManager = null;
    private MainUIManager m_mainUIManager = null;
    private List<BuildingBtn> m_bulidingBtnList = new List<BuildingBtn>();

    private Dictionary<ResourceType, long> m_requireResourcesDict = new();
    private Dictionary<ResourceType, long> m_producedResourcesDict = new();

    public GameDataManager GameDataManager => m_gameDataManager;
    public MainUIManager MainUIManager => m_mainUIManager;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize resource dictionaries
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

        foreach (ResourceType argType in System.Enum.GetValues(typeof(ResourceType)))
        {
            m_requireResourcesDict[argType] = 0;
            m_producedResourcesDict[argType] = 0;
        }

        if (m_buildingScrollViewContentTrans.childCount != m_gameDataManager.BuildingEntryDict.Count)
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

        foreach (KeyValuePair<string, BuildingEntry> item in m_gameDataManager.BuildingEntryDict)
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

    public void OnClickResetButton()
    {
        GameManager.Instance.ShowConfirmDialog(ConfirmDialogMessage.ResetState, ResetState);
    }

    public void OnClickApplyButton()
    {
        GameManager.Instance.ShowConfirmDialog(ConfirmDialogMessage.ApplyState, ApplyState);
    }

    public void OnBuildingBtnChanged(string argCode)
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            m_producedResourcesDict[type] = 0;
            m_requireResourcesDict[type] = 0;
        }

        foreach (BuildingBtn item in m_bulidingBtnList)
        {
            foreach (ResourceAmount item2 in item.GetProducedResources())
            {
                m_producedResourcesDict[item2.m_type] += item2.m_amount;
            }

            foreach (ResourceAmount item2 in item.GetRequiredResources())
            {
                m_requireResourcesDict[item2.m_type] -= item2.m_amount;
            }
        }

        UpdateChangeInfoUI();
    }

    void ResetState()
    {
        foreach (BuildingBtn item in m_bulidingBtnList)
        {
            item.Initialize();
        }
        
        m_producedResourcesDict = new();
        m_requireResourcesDict = new();

        UpdateChangeInfoUI();
    }

    void ApplyState()
    {
        if (!GameManager.Instance.TryChangeAllResources(m_requireResourcesDict))
        {
            GameManager.Instance.Warning(WarningMessages.WarningNotEnoughResource);
            return;
        }

        foreach (BuildingBtn item in m_bulidingBtnList)
        {
            m_gameDataManager.BuildingEntryDict[item.Code].m_state.m_amount = item.GetTotalCount();
        }

        ResetState();
        GameManager.Instance.GetBuildingDateResource();
        MainUIManager.UpdateAllMainText();
    }

    private void UpdateChangeInfoUI()
    {
        UIUtils.ClearChildren(m_addResourceContentTrans);
        UIUtils.ClearChildren(m_requiredResourceContentTrans);

        foreach (KeyValuePair<ResourceType, long> item in m_producedResourcesDict)
        {
            GameObject obj = Instantiate(MainUIManager.ResourceIconTextPrefeb, m_addResourceContentTrans);
            obj.GetComponent<ResourceIconText>().InitializeMainText(item.Key, item.Value);
        }

        foreach (KeyValuePair<ResourceType, long> item in m_requireResourcesDict)
        {
            GameObject obj = Instantiate(MainUIManager.ResourceIconTextPrefeb, m_requiredResourceContentTrans);
            obj.GetComponent<ResourceIconText>().InitializeMainText(item.Key, item.Value);
        }
    }
}
