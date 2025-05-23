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

    private int m_buildingCount = 0;
    private int m_buildingCountState = 0;

    public int TotalBuildingCount => m_buildingCount + m_buildingCountState;

    public List<ResourceAmount> CurrentProducedResources { get; private set; } = new List<ResourceAmount>();
    public List<ResourceAmount> CurrentRequiredResources { get; private set; } = new List<ResourceAmount>();

    public void Initialize(BuildingPanel argBuildingPanel, BuildingEntry argBuildingEntry)
    {
        if (argBuildingEntry == null)
        {
            Debug.LogError(ExceptionMessages.ErrorNullNotAllowed);
            return;
        }

        m_buildingPanel = argBuildingPanel;
        m_buildingEntry = argBuildingEntry;

        ClearChildren(m_addResourceContentTrans);
        ClearChildren(m_requiredResourceContentTrans);

        m_nameText.text = argBuildingEntry.m_data.m_name;
        m_image.sprite = argBuildingEntry.m_data.m_icon;

        m_buildingCount = argBuildingEntry.m_state.m_amount;
        m_buildingCountState = 0;
        UpdateAmountText();
        
        UpdateResourceContentAndUI();
    }

    public void PlusBtnClick()
    {
        m_buildingCountState += 1;
        UpdateAmountText();
        UpdateResourceContentAndUI();
    }

    public void MinusBtnClick()
    {
        int totalCount = m_buildingCount + m_buildingCountState;
        if (totalCount <= 0)
        {
            GameManager.Instance.Warning(WarningMessages.WarningNegativeValue);
            return;
        }

        m_buildingCountState -= 1;
        UpdateAmountText();
        UpdateResourceContentAndUI();
    }

    private void UpdateResourceContentAndUI()
    {
        ClearChildren(m_addResourceContentTrans);
        ClearChildren(m_requiredResourceContentTrans);

        int totalCount = m_buildingCount + m_buildingCountState;

        if (totalCount <= 0)
        {
            CurrentProducedResources = m_buildingEntry.CalculateProduction(1);
        }
        else
        {
            CurrentProducedResources = m_buildingEntry.CalculateProduction(totalCount);
        }

        CurrentRequiredResources = new List<ResourceAmount>();

        List<ResourceAmount> baseProduced = m_buildingEntry.CalculateProduction(1);
        foreach (ResourceAmount baseItem in baseProduced)
        {
            long totalAmount = baseItem.m_amount * totalCount;
            GameObject obj = Instantiate(m_buildingPanel.MainUIManager.ResourceIconTextPrefeb, m_addResourceContentTrans);
            obj.GetComponent<ResourceIconText>().Initialize(baseItem.m_type, baseItem.m_amount, totalAmount);
        }

        foreach (ResourceAmount baseItem in m_buildingEntry.m_data.m_requireResourceList)
        {
            long baseAmount = baseItem.m_amount;
            long totalAmount = baseItem.m_amount * totalCount;

            CurrentRequiredResources.Add(new ResourceAmount(baseItem.m_type, -totalAmount));

            GameObject obj = Instantiate(m_buildingPanel.MainUIManager.ResourceIconTextPrefeb, m_requiredResourceContentTrans);
            obj.GetComponent<ResourceIconText>().Initialize(baseItem.m_type, baseAmount, -totalAmount);
        }
    }


    private void UpdateAmountText()
    {
        int totalCount = m_buildingCount + m_buildingCountState;
        m_amountText.text = totalCount.ToString();

        if (m_buildingCountState > 0)
            m_amountText.color = Color.green;
        else if (m_buildingCountState < 0)
            m_amountText.color = Color.red;
        else
            m_amountText.color = Color.black;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
