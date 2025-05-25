using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_amountText = null;
    [SerializeField]
    Image m_image = null;

    [SerializeField]
    Transform m_addResourceContentTrans = null;
    [SerializeField]
    Transform m_requiredResourceContentTrans = null;

    private BuildingPanel m_buildingPanel = null;
    private BuildingEntry m_buildingEntry = null;

    private string m_code = string.Empty;
    private int m_buildingCount = 0;
    private int m_buildingCountState = 0;

    public string Code => m_code;
    public List<ResourceAmount> CurrentProducedResources { get; private set; } = new List<ResourceAmount>();
    public List<ResourceAmount> CurrentRequiredResources { get; private set; } = new List<ResourceAmount>();

    public void Initialize(BuildingPanel argBuildingPanel, BuildingEntry argBuildingEntry)
    {
        if (argBuildingEntry == null)
        {
            Debug.LogError(ExceptionMessages.ErrorNullNotAllowed);
            return;
        }
        m_code = argBuildingEntry.m_data.m_code;

        m_buildingPanel = argBuildingPanel;
        m_buildingEntry = argBuildingEntry;

        UIUtils.ClearChildren(m_addResourceContentTrans);
        UIUtils.ClearChildren(m_requiredResourceContentTrans);

        m_nameText.text = argBuildingEntry.m_data.m_name;
        m_image.sprite = argBuildingEntry.m_data.m_icon;

        m_buildingCount = argBuildingEntry.m_state.m_amount;
        m_buildingCountState = 0;
        UpdateAmountText();
        UpdateResourceContentAndUI();
    }

    public void Initialize()
    {
        if (m_buildingEntry == null)
        {
            Debug.LogError(ExceptionMessages.ErrorNullNotAllowed);
            return;
        }

        m_buildingCount = m_buildingEntry.m_state.m_amount;
        m_buildingCountState = 0;

        UpdateAmountText();
        UpdateResourceContentAndUI();
    }

    public int GetTotalCount()
    {
        return m_buildingCount + m_buildingCountState;
    }

    // 예상 생산 리소스를 반환
    public List<ResourceAmount> GetProducedResources()
    {
        int _totalCount = m_buildingCountState;

        return m_buildingEntry.CalculateProduction(_totalCount);
    }

    // 예상 필요 리소스를 반환
    public List<ResourceAmount> GetRequiredResources()
    {
        int _totalCount = m_buildingCountState;
        if (_totalCount <= 0)
            _totalCount = 0;

        List<ResourceAmount> required = new List<ResourceAmount>();

        foreach (ResourceAmount baseItem in m_buildingEntry.m_data.m_requireResourceList)
        {
            long totalAmount = baseItem.m_amount * _totalCount;
            required.Add(new ResourceAmount(baseItem.m_type, totalAmount));
        }

        return required;
    }

    public void PlusBtnClick()
    {
        m_buildingCountState += 1;
        UpdateAmountText();
        UpdateResourceContentAndUI();
        m_buildingPanel.OnBuildingBtnChanged(m_code);
    }

    public void MinusBtnClick()
    {
        int _totalCount = m_buildingCount + m_buildingCountState;
        if (_totalCount <= 0)
        {
            GameManager.Instance.Warning(WarningMessages.WarningNegativeValue);
            return;
        }

        m_buildingCountState -= 1;
        UpdateAmountText();
        UpdateResourceContentAndUI();
        m_buildingPanel.OnBuildingBtnChanged(m_code);
    }

    private void UpdateResourceContentAndUI()
    {
        UIUtils.ClearChildren(m_addResourceContentTrans);
        UIUtils.ClearChildren(m_requiredResourceContentTrans);

        int _totalCount = m_buildingCount + m_buildingCountState;

        if (_totalCount <= 0)
        {
            CurrentProducedResources = m_buildingEntry.CalculateProduction(1);
        }
        else
        {
            CurrentProducedResources = m_buildingEntry.CalculateProduction(_totalCount);
        }

        CurrentRequiredResources = new List<ResourceAmount>();

        List<ResourceAmount> baseProduced = m_buildingEntry.CalculateProduction(1);
        foreach (ResourceAmount baseItem in baseProduced)
        {
            long totalAmount = baseItem.m_amount * _totalCount;
            GameObject obj = Instantiate(m_buildingPanel.MainUIManager.ResourceIconTextPrefeb, m_addResourceContentTrans);
            obj.GetComponent<ResourceIconText>().InitializeMainText(baseItem.m_type, baseItem.m_amount);

            obj.GetComponent<ResourceIconText>().InitializeChangeText(totalAmount, false);
        }

        foreach (ResourceAmount baseItem in m_buildingEntry.m_data.m_requireResourceList)
        {
            long baseAmount = baseItem.m_amount;
            long totalAmount = baseItem.m_amount * m_buildingCountState;

            CurrentRequiredResources.Add(new ResourceAmount(baseItem.m_type, totalAmount));

            GameObject obj = Instantiate(m_buildingPanel.MainUIManager.ResourceIconTextPrefeb, m_requiredResourceContentTrans);
            obj.GetComponent<ResourceIconText>().InitializeMainText(baseItem.m_type, baseAmount);

            if(m_buildingCount == totalAmount)
            {
                obj.GetComponent<ResourceIconText>().InitializeChangeText(-totalAmount, false);
            }
            else
            {
                obj.GetComponent<ResourceIconText>().InitializeChangeText(-totalAmount, true);
            }
            
        }
    }

    private void UpdateAmountText()
    {
        int _totalCount = m_buildingCount + m_buildingCountState;
        m_amountText.text = _totalCount.ToString();

        if (m_buildingCountState > 0)
            m_amountText.color = Color.green;
        else if (m_buildingCountState < 0)
            m_amountText.color = Color.red;
        else
            m_amountText.color = Color.black;
    }
}
