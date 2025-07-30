using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPanel : BasePanel
{
    [SerializeField]
    GameObject m_buildingBtnPrefeb = null;
    [SerializeField]
    Transform m_buildingScrollViewContentTrans = null;
    [SerializeField]
    Transform m_addResourceContentTrans = null;
    [SerializeField]
    Transform m_requiredResourceContentTrans = null;

    private List<BuildingBtn> m_bulidingBtnList = new List<BuildingBtn>();

    private Dictionary<ResourceType.TYPE, long> m_requireResourcesDict = new();
    private Dictionary<ResourceType.TYPE, long> m_producedResourcesDict = new();

    // Start is called before the first frame update
    void Start()
    {
        // Initialize resource dictionaries
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Building");
        SetBuildingLevel(""); // 빌딩 패널은 레벨이 필요 없음

        InitializeResourceDictionaries();
        InitializeBuildingButtons();
    }

    /// <summary>
    /// 리소스 딕셔너리 초기화
    /// </summary>
    private void InitializeResourceDictionaries()
    {
        foreach (ResourceType.TYPE argType in System.Enum.GetValues(typeof(ResourceType.TYPE)))
        {
            m_requireResourcesDict[argType] = 0;
            m_producedResourcesDict[argType] = 0;
        }
    }

    /// <summary>
    /// 빌딩 버튼들 초기화
    /// </summary>
    private void InitializeBuildingButtons()
    {
        if (m_buildingScrollViewContentTrans == null)
        {
            Debug.LogError("Building scroll view content transform is null!");
            return;
        }

        // 기존 버튼들이 있으면 제거
        if (m_buildingScrollViewContentTrans.childCount != m_gameDataManager.BuildingEntryDict.Count)
        {
            ClearExistingButtons();
        }
        else
        {
            return; // 이미 올바른 개수만큼 있으면 리턴
        }

        // 새 버튼들 생성
        CreateBuildingButtons();
    }

    /// <summary>
    /// 기존 버튼들 제거
    /// </summary>
    private void ClearExistingButtons()
    {
        foreach (Transform item in m_buildingScrollViewContentTrans)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        m_bulidingBtnList.Clear();
    }

    /// <summary>
    /// 빌딩 버튼들 생성
    /// </summary>
    private void CreateBuildingButtons()
    {
        if (m_buildingBtnPrefeb == null)
        {
            Debug.LogError("Building button prefab is null!");
            return;
        }

        foreach (KeyValuePair<string, BuildingEntry> item in m_gameDataManager.BuildingEntryDict)
        {
            GameObject _buildingBtnObj = Instantiate(m_buildingBtnPrefeb, m_buildingScrollViewContentTrans);
            BuildingBtn _buildingBtn = _buildingBtnObj.GetComponent<BuildingBtn>();

            if (_buildingBtn != null)
            {
                m_bulidingBtnList.Add(_buildingBtn);
                _buildingBtn.Initialize(this, item.Value);
            }
            else
            {
                Debug.LogError($"BuildingBtn component not found on prefab for building: {item.Key}");
            }
        }
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
        foreach (ResourceType.TYPE type in System.Enum.GetValues(typeof(ResourceType.TYPE)))
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
        GameObjectUtils.ClearChildren(m_addResourceContentTrans);
        GameObjectUtils.ClearChildren(m_requiredResourceContentTrans);

        foreach (KeyValuePair<ResourceType.TYPE, long> item in m_producedResourcesDict)
        {
            GameObject obj = Instantiate(MainUIManager.ResourceIconTextPrefeb, m_addResourceContentTrans);
            obj.GetComponent<ResourceIconText>().InitializeMainText(item.Key, item.Value);
        }

        foreach (KeyValuePair<ResourceType.TYPE, long> item in m_requireResourcesDict)
        {
            GameObject obj = Instantiate(MainUIManager.ResourceIconTextPrefeb, m_requiredResourceContentTrans);
            obj.GetComponent<ResourceIconText>().InitializeMainText(item.Key, item.Value);
        }
    }
}
