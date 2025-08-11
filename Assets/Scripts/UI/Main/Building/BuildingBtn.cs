using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 건물 버튼 UI 컴포넌트
/// 건물의 정보를 표시하고 건물 개수 변경을 관리합니다.
/// </summary>
public class BuildingBtn : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject m_lockPanel = null;
    [SerializeField] private TextMeshProUGUI m_nameText = null;
    [SerializeField] private TextMeshProUGUI m_amountText = null;
    [SerializeField] private Image m_image = null;

    [Header("Resource Content")]
    [SerializeField] private Transform m_addResourceContentTrans = null;
    [SerializeField] private Transform m_requiredResourceContentTrans = null;

    // Private fields
    private BuildingPanel m_buildingPanel = null;
    private BuildingEntry m_buildingEntry = null;
    private string m_code = string.Empty;
    private int m_buildingCount = 0;
    private int m_buildingCountState = 0;

    // Properties
    public BuildingEntry BuildingEntry => m_buildingEntry;
    public string Code => m_code;
    public List<ResourceAmount> CurrentProducedResources { get; private set; } = new List<ResourceAmount>();
    public List<ResourceAmount> CurrentRequiredResources { get; private set; } = new List<ResourceAmount>();
    public int TotalCount => m_buildingCount + m_buildingCountState;

    #region Initialization
    /// <summary>
    /// 건물 버튼을 초기화합니다.
    /// </summary>
    /// <param name="argBuildingPanel">부모 건물 패널</param>
    /// <param name="argBuildingEntry">건물 엔트리 데이터</param>
    public void Initialize(BuildingPanel argBuildingPanel, BuildingEntry argBuildingEntry)
    {
        if (argBuildingEntry == null)
        {
            Debug.LogError(ExceptionMessages.ErrorNullNotAllowed);
            return;
        }

        m_buildingPanel = argBuildingPanel;
        m_buildingEntry = argBuildingEntry;
        m_code = argBuildingEntry.m_data.m_code;

        SetupUI();
        RefreshData();
    }

    /// <summary>
    /// 기존 데이터로 UI를 다시 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (m_buildingEntry == null)
        {
            Debug.LogError(ExceptionMessages.ErrorNullNotAllowed);
            return;
        }

        RefreshData();
    }

    private void SetupUI()
    {
        m_nameText.text = m_buildingEntry.m_data.m_name;
        m_image.sprite = m_buildingEntry.m_data.m_icon;
    }

    private void RefreshData()
    {
        m_buildingCount = m_buildingEntry.m_state.m_amount;
        m_buildingCountState = 0;
        
        UpdateAmountText();
        UpdateResourceContentAndUI();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 현재 생산되는 자원 목록을 반환합니다.
    /// </summary>
    /// <returns>생산 자원 목록</returns>
    public List<ResourceAmount> GetProducedResources()
    {
        return m_buildingEntry.CalculateProduction(m_buildingCountState);
    }

    /// <summary>
    /// 현재 필요한 자원 목록을 반환합니다.
    /// </summary>
    /// <returns>필요 자원 목록</returns>
    public List<ResourceAmount> GetRequiredResources()
    {
        return CurrentRequiredResources;
    }

    /// <summary>
    /// 건물 개수를 증가시킵니다.
    /// </summary>
    public void PlusBtnClick()
    {
        if (!CanModifyBuilding())
        {
            return;
        }

        m_buildingCountState++;
        UpdateUI();
        m_buildingPanel.OnBuildingBtnChanged(m_code);
    }

    /// <summary>
    /// 건물 개수를 감소시킵니다.
    /// </summary>
    public void MinusBtnClick()
    {
        if (!CanModifyBuilding())
        {
            return;
        }

        if (TotalCount <= 0)
        {
            GameManager.Instance.Warning(WarningMessages.WarningNegativeValue);
            return;
        }

        m_buildingCountState--;
        UpdateUI();
        m_buildingPanel.OnBuildingBtnChanged(m_code);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 건물 수정이 가능한지 확인합니다.
    /// </summary>
    /// <returns>수정 가능 여부</returns>
    private bool CanModifyBuilding()
    {
        if (m_buildingCount < 0)
        {
            m_lockPanel.SetActive(true);
            GameManager.Instance.Warning(WarningMessages.WarningAccessDenied);
            return false;
        }

        m_lockPanel.SetActive(false);
        return true;
    }

    /// <summary>
    /// UI를 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        UpdateAmountText();
        UpdateResourceContentAndUI();
    }

    /// <summary>
    /// 건물 개수 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateAmountText()
    {
        m_amountText.text = TotalCount.ToString();
        m_amountText.color = GetAmountTextColor();
    }

    /// <summary>
    /// 건물 개수에 따른 텍스트 색상을 반환합니다.
    /// </summary>
    /// <returns>텍스트 색상</returns>
    private Color GetAmountTextColor()
    {
        if (m_buildingCountState > 0)
            return Color.green;
        else if (m_buildingCountState < 0)
            return Color.red;
        else
            return Color.black;
    }

    /// <summary>
    /// 자원 콘텐츠와 UI를 업데이트합니다.
    /// </summary>
    private void UpdateResourceContentAndUI()
    {
        UpdateLockPanel();
        ClearResourceContents();
        UpdateProducedResources();
        UpdateRequiredResources();
    }

    /// <summary>
    /// 잠금 패널 상태를 업데이트합니다.
    /// </summary>
    private void UpdateLockPanel()
    {
        m_lockPanel.SetActive(m_buildingCount < 0);
    }

    /// <summary>
    /// 자원 콘텐츠를 초기화합니다.
    /// </summary>
    private void ClearResourceContents()
    {
        GameObjectUtils.ClearChildren(m_addResourceContentTrans);
        GameObjectUtils.ClearChildren(m_requiredResourceContentTrans);
    }

    /// <summary>
    /// 생산 자원 UI를 업데이트합니다.
    /// </summary>
    private void UpdateProducedResources()
    {
        int displayCount = TotalCount <= 0 ? 1 : TotalCount;
        CurrentProducedResources = m_buildingEntry.CalculateProduction(displayCount);

        List<ResourceAmount> baseProduced = m_buildingEntry.CalculateProduction(1);
        foreach (ResourceAmount baseItem in baseProduced)
        {
            long totalAmount = baseItem.m_amount * TotalCount;
            CreateResourceIconText(m_addResourceContentTrans, baseItem.m_type, baseItem.m_amount, totalAmount, false);
        }
    }

    /// <summary>
    /// 필요 자원 UI를 업데이트합니다.
    /// </summary>
    private void UpdateRequiredResources()
    {
        CurrentRequiredResources.Clear();

        foreach (ResourceAmount baseItem in m_buildingEntry.m_data.m_requireResourceList)
        {
            long totalAmount = CalculateRequiredResourceAmount(baseItem);
            CurrentRequiredResources.Add(new ResourceAmount(baseItem.m_type, totalAmount));

            CreateResourceIconText(m_requiredResourceContentTrans, baseItem.m_type, baseItem.m_amount, totalAmount, true);
        }
    }

    /// <summary>
    /// 필요 자원의 총량을 계산합니다.
    /// </summary>
    /// <param name="baseItem">기본 자원 아이템</param>
    /// <returns>계산된 총량</returns>
    private long CalculateRequiredResourceAmount(ResourceAmount baseItem)
    {
        if (m_buildingCountState < 0)
        {
            // 환불 계산
            long refundedAmount = (long)(baseItem.m_amount * Mathf.Abs(m_buildingCountState) * 
                m_buildingPanel.GameDataManager.GameBalanceEntry.m_data.m_buildingRefundRate);
            return -refundedAmount;
        }
        else
        {
            // 일반 비용 계산
            return baseItem.m_amount * m_buildingCountState;
        }
    }

    /// <summary>
    /// 자원 아이콘 텍스트 UI를 생성합니다.
    /// </summary>
    /// <param name="parent">부모 Transform</param>
    /// <param name="resourceType">자원 타입</param>
    /// <param name="baseAmount">기본 양</param>
    /// <param name="totalAmount">총 양</param>
    /// <param name="isRequired">필요 자원 여부</param>
    private void CreateResourceIconText(Transform parent, ResourceType.TYPE resourceType, long baseAmount, long totalAmount, bool isRequired)
    {
        GameObject obj = Instantiate(m_buildingPanel.MainUIManager.ResourceIconTextPrefeb, parent);
        var resourceIconText = obj.GetComponent<ResourceIconText>();
        
        resourceIconText.InitializeMainText(resourceType, baseAmount);

        if (isRequired)
        {
            if (totalAmount < 0)
            {
                resourceIconText.InitializeChangeText(-totalAmount, true);
            }
            else if (totalAmount > 0)
            {
                resourceIconText.InitializeChangeText(-totalAmount, true);
            }
            else
            {
                resourceIconText.InitializeChangeText(0, false);
            }
        }
        else
        {
            resourceIconText.InitializeChangeText(totalAmount, false);
        }
    }
    #endregion
}