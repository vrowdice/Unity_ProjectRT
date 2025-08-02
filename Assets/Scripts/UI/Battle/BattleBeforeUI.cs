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


    [Header("적 유닛 개수 텍스트")]
    [SerializeField] private GameObject LongUnitCountText;
    [SerializeField] private GameObject ShortUnitCountText;
    [SerializeField] private GameObject DefenseUnitCountText;

    private TextMeshProUGUI LongUnitCountTextMesh;
    private TextMeshProUGUI ShortUnitCountTextMesh;
    private TextMeshProUGUI DefenseUnitCountTextMesh;

    private string defaultNum = "0000";

    public enum UnitTagType
    {
        Long,
        Short,
        Defense
    }

    private Dictionary<UnitTagType, string> tagNames;
    private Dictionary<UnitTagType, TextMeshProUGUI> tagTexts;

    // Start is called before the first frame update
    void Start()
    {
        GenerateList();
        FirstSettingCountText();
    }

    // Update is called once per frame
    void Update()
    {
        CountMyUnit();
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

    private void FirstSettingCountText()
    {
        LongUnitCountTextMesh = LongUnitCountText.GetComponent<TextMeshProUGUI>();
        ShortUnitCountTextMesh = ShortUnitCountText.GetComponent<TextMeshProUGUI>();
        DefenseUnitCountTextMesh = DefenseUnitCountText.GetComponent<TextMeshProUGUI>();

        LongUnitCountTextMesh.text = defaultNum;
        ShortUnitCountTextMesh.text = defaultNum;
        DefenseUnitCountTextMesh.text = defaultNum;

        tagNames = new Dictionary<UnitTagType, string>
    {
        { UnitTagType.Long, "MyLongUnit"},
        { UnitTagType.Short, "MyShortUnit" },
        { UnitTagType.Defense, "MyDefenseUnit"}
    };

        tagTexts = new Dictionary<UnitTagType, TextMeshProUGUI>
    {
        { UnitTagType.Long, LongUnitCountTextMesh},
        { UnitTagType.Short, ShortUnitCountTextMesh},
        { UnitTagType.Defense, DefenseUnitCountTextMesh}
    };
    }

    private void CountMyUnit()
    {
        foreach (UnitTagType tagType in System.Enum.GetValues(typeof(UnitTagType)))
        {
            string tagName = tagNames[tagType];

            int count = 0;
            try
            {
                GameObject[] foundUnits = GameObject.FindGameObjectsWithTag(tagName);
                count = foundUnits?.Length ?? 0;
            }
            catch (UnityException)
            {
                // 태그가 존재하지 않는 경우 (에디터에서 태그 설정이 안 되어 있을 때 등)
                count = 0;
            }

            tagTexts[tagType].text = count.ToString("D4"); // 네 자리 0으로 채움 (ex: 0003)
        }
    }
}

