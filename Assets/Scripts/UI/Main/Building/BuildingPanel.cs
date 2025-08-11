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
    private int m_filterIndex = 0;

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
        SetPanelName("Building");
        SetBuildingLevel("");

        InitializeResourceDictionaries();
        ScrollViewFilterBtn(m_filterIndex);
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
    /// 0 = 기반 건물
    /// 1 = 나무
    /// 2 = 철
    /// 3 = 음식
    /// 4 = 연구력
    /// </summary>
    /// <param name="argFilterIndex"></param>
    public void ScrollViewFilterBtn(int argFilterIndex)
    {
        if (m_buildingScrollViewContentTrans == null)
        {
            Debug.LogError("Building scroll view content transform is null!");
            return;
        }

        if (m_buildingScrollViewContentTrans.childCount == 0)
        {
            CreateAllBuildingButtons();
        }

        UpdateButtonVisibility(argFilterIndex);
    }

    /// <summary>
    /// 모든 건물 버튼을 생성
    /// </summary>
    private void CreateAllBuildingButtons()
    {
        m_bulidingBtnList.Clear();

        foreach (KeyValuePair<string, BuildingEntry> item in m_gameDataManager.BuildingEntryDict)
        {
            GameObject _buildingBtnObj = Instantiate(m_buildingBtnPrefeb, m_buildingScrollViewContentTrans);
            BuildingBtn _buildingBtn = _buildingBtnObj.GetComponent<BuildingBtn>();

            if (_buildingBtn != null)
            {
                _buildingBtn.Initialize(this, item.Value);
                m_bulidingBtnList.Add(_buildingBtn);
            }
            else
            {
                Debug.LogError($"BuildingBtn component not found on prefab for building: {item.Key}");
            }
        }
    }

    /// <summary>
    /// 필터에 따라 버튼의 활성화/비활성화 상태를 업데이트
    /// </summary>
    /// <param name="filterIndex">필터 인덱스</param>
    private void UpdateButtonVisibility(int filterIndex)
    {
        foreach (BuildingBtn buildingBtn in m_bulidingBtnList)
        {
            BuildingData buildingData = buildingBtn.BuildingEntry.m_data;
            bool shouldShow = ShouldShowBuilding(buildingData, filterIndex);
            
            buildingBtn.gameObject.SetActive(shouldShow);
        }
    }

    /// <summary>
    /// 건물이 필터 조건에 따라 표시되어야 하는지 확인
    /// </summary>
    /// <param name="buildingData">확인할 건물 데이터</param>
    /// <param name="filterIndex">필터 인덱스</param>
    /// <returns>표시되어야 하면 true, 아니면 false</returns>
    private bool ShouldShowBuilding(BuildingData buildingData, int filterIndex)
    {
        // 기반 건물 필터 (0) - Production이 아닌 건물들
        if (filterIndex == 0)
        {
            return buildingData.m_buildingType != BuildingType.TYPE.Production;
        }

        // 특정 리소스 생산 건물 필터 (1-4) - 해당 리소스를 생산하는 Production 건물들
        if (filterIndex >= 1 && filterIndex <= 4)
        {
            ResourceType.TYPE targetResourceType = GetResourceTypeByFilterIndex(filterIndex);
            return buildingData.m_buildingType == BuildingType.TYPE.Production &&
                   buildingData.HasProductionResource(targetResourceType);
        }

        return false;
    }

    /// <summary>
    /// 필터 인덱스에 해당하는 리소스 타입을 반환
    /// </summary>
    /// <param name="filterIndex">필터 인덱스</param>
    /// <returns>해당하는 리소스 타입</returns>
    private ResourceType.TYPE GetResourceTypeByFilterIndex(int filterIndex)
    {
        switch (filterIndex)
        {
            case 1: return ResourceType.TYPE.Wood;
            case 2: return ResourceType.TYPE.Iron;
            case 3: return ResourceType.TYPE.Food;
            case 4: return ResourceType.TYPE.Tech;
            default: return ResourceType.TYPE.Wood;
        }
    }

    /// <summary>
    /// 모든 버튼을 비활성화
    /// </summary>
    private void DisableAllButtons()
    {
        foreach (BuildingBtn buildingBtn in m_bulidingBtnList)
        {
            if (buildingBtn != null)
            {
                buildingBtn.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 모든 버튼을 활성화
    /// </summary>
    private void EnableAllButtons()
    {
        foreach (BuildingBtn buildingBtn in m_bulidingBtnList)
        {
            if (buildingBtn != null)
            {
                buildingBtn.gameObject.SetActive(true);
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
            m_gameDataManager.BuildingEntryDict[item.Code].m_state.m_amount = item.TotalCount;
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
