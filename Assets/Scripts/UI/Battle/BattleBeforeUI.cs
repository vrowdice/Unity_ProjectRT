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
    [SerializeField] private GameObject unitBoxPrefab; // 병력 슬롯 UI 프리팹

    [Header("적 유닛 개수 텍스트")]
    [SerializeField] private GameObject LongUnitCountText;
    [SerializeField] private GameObject ShortUnitCountText;
    [SerializeField] private GameObject DefenseUnitCountText;

    private TextMeshProUGUI LongUnitCountTextMesh;
    private TextMeshProUGUI ShortUnitCountTextMesh;
    private TextMeshProUGUI DefenseUnitCountTextMesh;

    private string defaultNum = "0000";
    private string placementModeText = "Placement mode";
    private string recallModeText = "Recall mode";

    private Color PlacementModeColor = new Color(0.2f, 0.6f, 1f); // 파란색
    private Color RecallModeColor = new Color(1f, 0.8f, 0.2f);     // 노란색

    private Dictionary<string, GameObject> unitBoxMap = new();

    [SerializeField] private GameObject BattleStartBtn; // 전투 시작 버튼
    private int totalUnitToPlaceCount = 0; // 총 배치해야 할 유닛 수

    public static bool IsInPlacementMode { get; private set; } = false;

    public enum UnitTagType
    {
        Long,
        Short,
        Defense
    }

    private Dictionary<UnitTagType, string> tagNames;
    private Dictionary<UnitTagType, TextMeshProUGUI> tagTexts;

    //배치 모드 텍스트와 회수 버튼
    [Header("배치 모드 관련 UI")]
    [SerializeField] private GameObject PlacementStatusText;
    [SerializeField] private GameObject UnitRecallBtn;

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

    public void TogglePlacementMode()
    {
        IsInPlacementMode = !IsInPlacementMode;

        var text = PlacementStatusText.GetComponent<TextMeshProUGUI>();
        text.text = IsInPlacementMode ? placementModeText : recallModeText;

        var buttonImage = UnitRecallBtn.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = IsInPlacementMode ? PlacementModeColor : RecallModeColor;
        }
    }

    private void GenerateList()
    {
        contentParent = GameObject.Find("Content").GetComponent<Transform>();
        m_battleLoadingManager = FindObjectOfType<BattleLoadingManager>();

        totalUnitToPlaceCount = m_battleLoadingManager.allyArmyDataList.Count;

        allyUnitDataList.Clear();
        unitBoxMap.Clear(); // 딕셔너리 초기화

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

            GameObject myUnit = Instantiate(unitBoxPrefab, contentParent);
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

            // 딕셔너리에 유닛 이름으로 저장
            string unitKey = unitName.Trim().ToLower();
            unitBoxMap[unitKey] = myUnit;
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

        //초기 비활성화
        var text = PlacementStatusText.GetComponent<TextMeshProUGUI>();
        if (text != null)
            text.text = placementModeText;

        var image = UnitRecallBtn.GetComponent<Image>();
        if (image != null)
            image.color = PlacementModeColor;

        if (UnitRecallBtn != null) UnitRecallBtn.SetActive(false);
        IsInPlacementMode = true; // 기본은 배치 모드
    }

    private void CountMyUnit()
    {
        int totalCount = 0;



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
                count = 0;
            }

            tagTexts[tagType].text = count.ToString();
            totalCount += count;
        }

        bool hasAnyUnit = totalCount > 0;

        if (UnitRecallBtn != null)
        {
            UnitRecallBtn.SetActive(hasAnyUnit);
        }

        //유닛이 하나도 없으면 모드 초기화
        if (!hasAnyUnit)
        {
            IsInPlacementMode = true;

            // 모드 텍스트 내용만 초기화
            if (PlacementStatusText != null)
            {
                var text = PlacementStatusText.GetComponent<TextMeshProUGUI>();
                if (text != null)
                    text.text = placementModeText;
            }

            // 버튼 색상 초기화
            var image = UnitRecallBtn.GetComponent<Image>();
            if (image != null)
                image.color = PlacementModeColor;

            // 버튼은 비활성화
            UnitRecallBtn.SetActive(false);
        }

        if (BattleStartBtn != null)
        {
            BattleStartBtn.SetActive(totalCount >= totalUnitToPlaceCount && totalUnitToPlaceCount > 0);
        }
    }

    public void AddUnitToList(UnitStatBase unit)
    {
        if (unit == null || unitBoxPrefab == null || contentParent == null)
        {
            Debug.LogError("unit, unitBoxPrefab 또는 contentParent가 null입니다.");
            return;
        }

        string unitKey = unit.unitName.Trim().ToLower();

        // 이미 존재하는 유닛 박스라면 수량만 증가
        if (unitBoxMap.TryGetValue(unitKey, out GameObject existingBox))
        {
            UnitBox boxComponent = existingBox.GetComponent<UnitBox>();
            if (boxComponent != null)
            {
                boxComponent.IncreaseUnitCount(1);
            }
            else
            {
                Debug.LogWarning($"[UnitBox 누락] {unit.unitName} 오브젝트에 UnitBox 컴포넌트가 없습니다.");
            }
            return;
        }

        // 새로운 박스 생성
        GameObject newBox = Instantiate(unitBoxPrefab, contentParent);
        if (newBox == null)
        {
            Debug.LogError($"[생성 실패] UnitBox 프리팹 생성 실패: {unit.unitName}");
            return;
        }
        newBox.transform.localScale = Vector3.one;

        // 아이콘 설정
        Image icon = newBox.transform.Find("UnitImage").GetComponent<Image>();
        if (icon != null && unit.unitIllustration != null)
            icon.sprite = unit.unitIllustration.sprite;

        // 유닛 이름 설정
        TextMeshProUGUI nameText = newBox.transform.Find("UnitTexts/UnitNameText").GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = unit.unitName;

        // 수량 텍스트
        TextMeshProUGUI countText = newBox.transform.Find("UnitTexts/UnitCountText").GetComponent<TextMeshProUGUI>();
        if (countText == null)
        {
            Debug.LogWarning($"[UI 오류] {unit.unitName}에 UnitCountText가 없습니다.");
        }

        // UnitBox 초기화
        UnitBox boxComponentNew = newBox.GetComponent<UnitBox>();
        if (boxComponentNew != null)
        {
            boxComponentNew.Init(unit, 1, countText);
        }
        else
        {
            Debug.LogWarning($"[UnitBox 누락] {unit.unitName} 오브젝트에 UnitBox 컴포넌트가 없습니다.");
        }

        // 딕셔너리에 저장 (정규화된 키)
        unitBoxMap[unitKey] = newBox;
    }
}

