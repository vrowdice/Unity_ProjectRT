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
    [SerializeField] private GameObject armyBox; // 병력 슬롯 UI 프리팹

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
        foreach (UnitStatBase unit in m_battleLoadingManager.allyArmyDataList)
        {
            allyUnitDataList.Add(unit);
            Debug.Log($"아군 유닛 이름: {unit.unitName}");
        }

        // 기존 자식 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 유닛 데이터 기반 프리팹 생성
        foreach (var unit in allyUnitDataList)
        {
            GameObject myUnit = Instantiate(armyBox, contentParent);
            myUnit.transform.localScale = Vector3.one;

            // 유닛 아이콘 설정
            Image icon = myUnit.transform.Find("ArmyImage").GetComponent<Image>();
            if (icon != null && unit.unitIllustration != null)
                icon = unit.unitIllustration;

            // 유닛 이름 텍스트 설정
            TextMeshProUGUI nameText = myUnit.transform.Find("AmryTexts").transform.Find("AmryNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                Debug.Log($"unitName: {unit.unitName}");
                nameText.text = unit.unitName;
            }
        }
    }
}
