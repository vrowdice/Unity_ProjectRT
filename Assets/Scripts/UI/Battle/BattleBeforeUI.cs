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

    [Header("유닛 슬롯 UI")]
    [SerializeField] private GameObject unitBox; // 병력 슬롯 UI 프리팹

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

        // 이름 기준으로 유닛 개수 세기
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

        // 기존 자식 제거
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

            // 유닛 아이콘 설정
            Image icon = myUnit.transform.Find("UnitImage").GetComponent<Image>();
            if (icon != null && unit.unitIllustration != null)
                icon.sprite = unit.unitIllustration.sprite;

            // 유닛 이름 텍스트
            TextMeshProUGUI nameText = myUnit.transform.Find("UnitTexts/UnitNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = unit.unitName;

            // 유닛 수량 텍스트
            TextMeshProUGUI countText = myUnit.transform.Find("UnitTexts/UnitCountText").GetComponent<TextMeshProUGUI>();

            // 유닛 버튼 기능 연결
            UnitBox unitButton = myUnit.GetComponent<UnitBox>();
            if (unitButton != null)
            {
                unitButton.Init(unit, count, countText);
            }
        }
    }
}

