using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleBeforeUI : MonoBehaviour
{
    private Transform contentParent;
    private BattleLoadingManager m_battleLoadingManager = null;

    [HideInInspector] public List<UnitStatBase> allyUnitDataList = new();

    [Header("���� ���� UI")]
    [SerializeField] private GameObject unitBox; // ���� ���� UI ������

    // Start is called before the first frame update
    void Start()
    {
        GenerateList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateList()
    {
        contentParent = GameObject.Find("Content").GetComponent<Transform>();
        m_battleLoadingManager = FindObjectOfType<BattleLoadingManager>();

        allyUnitDataList.Clear();

        // �̸� �������� ���� ���� ����
        Dictionary<string, (UnitStatBase data, int count)> unitCountMap = new();

        foreach (UnitStatBase unit in m_battleLoadingManager.allyArmyDataList)
        {
            if (unitCountMap.ContainsKey(unit.unitName))
            {
                unitCountMap[unit.unitName] = (unit, unitCountMap[unit.unitName].count + 1);
            }
            else
            {
                unitCountMap[unit.unitName] = (unit, 1);
            }

            allyUnitDataList.Add(unit);
        }

        // ���� �ڽ� ����
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in unitCountMap)
        {
            string unitName = entry.Key;
            UnitStatBase unit = entry.Value.data;
            int count = entry.Value.count;

            GameObject myUnit = Instantiate(unitBox, contentParent);
            myUnit.transform.localScale = Vector3.one;

            // ���� ������ ����
            Image icon = myUnit.transform.Find("UnitImage").GetComponent<Image>();
            if (icon != null && unit.unitIllustration != null)
                icon.sprite = unit.unitIllustration.sprite;

            // ���� �̸� �ؽ�Ʈ
            TextMeshProUGUI nameText = myUnit.transform.Find("UnitTexts/UnitNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = unit.unitName;

            // ���� ���� �ؽ�Ʈ
            TextMeshProUGUI countText = myUnit.transform.Find("UnitTexts/UnitCountText").GetComponent<TextMeshProUGUI>();

            // ���� ��ư ��� ����
            UnitBox unitButton = myUnit.GetComponent<UnitBox>();
            if (unitButton != null)
            {
                unitButton.Init(unit, count, countText);
            }
        }
    }
}

