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
    [SerializeField] private GameObject armyBox; // ���� ���� UI ������

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
            Debug.Log($"�Ʊ� ���� �̸�: {unit.unitName}");
        }

        // ���� �ڽ� ����
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // ���� ������ ��� ������ ����
        foreach (var unit in allyUnitDataList)
        {
            GameObject myUnit = Instantiate(armyBox, contentParent);
            myUnit.transform.localScale = Vector3.one;

            // ���� ������ ����
            Image icon = myUnit.transform.Find("ArmyImage").GetComponent<Image>();
            if (icon != null && unit.unitIllustration != null)
                icon = unit.unitIllustration;

            // ���� �̸� �ؽ�Ʈ ����
            TextMeshProUGUI nameText = myUnit.transform.Find("AmryTexts").transform.Find("AmryNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                Debug.Log($"unitName: {unit.unitName}");
                nameText.text = unit.unitName;
            }
        }
    }
}
