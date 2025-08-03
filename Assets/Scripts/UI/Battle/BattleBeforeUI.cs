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
    [SerializeField] private GameObject unitBoxPrefab; // ���� ���� UI ������

    [Header("�� ���� ���� �ؽ�Ʈ")]
    [SerializeField] private GameObject LongUnitCountText;
    [SerializeField] private GameObject ShortUnitCountText;
    [SerializeField] private GameObject DefenseUnitCountText;

    private TextMeshProUGUI LongUnitCountTextMesh;
    private TextMeshProUGUI ShortUnitCountTextMesh;
    private TextMeshProUGUI DefenseUnitCountTextMesh;

    private string defaultNum = "0000";
    private string placementModeText = "Placement mode";
    private string recallModeText = "Recall mode";

    private Color PlacementModeColor = new Color(0.2f, 0.6f, 1f); // �Ķ���
    private Color RecallModeColor = new Color(1f, 0.8f, 0.2f);     // �����

    private Dictionary<string, GameObject> unitBoxMap = new();

    [SerializeField] private GameObject BattleStartBtn; // ���� ���� ��ư
    private int totalUnitToPlaceCount = 0; // �� ��ġ�ؾ� �� ���� ��

    public static bool IsInPlacementMode { get; private set; } = false;

    public enum UnitTagType
    {
        Long,
        Short,
        Defense
    }

    private Dictionary<UnitTagType, string> tagNames;
    private Dictionary<UnitTagType, TextMeshProUGUI> tagTexts;

    //��ġ ��� �ؽ�Ʈ�� ȸ�� ��ư
    [Header("��ġ ��� ���� UI")]
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
        unitBoxMap.Clear(); // ��ųʸ� �ʱ�ȭ

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

            GameObject myUnit = Instantiate(unitBoxPrefab, contentParent);
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

            // ��ųʸ��� ���� �̸����� ����
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

        //�ʱ� ��Ȱ��ȭ
        var text = PlacementStatusText.GetComponent<TextMeshProUGUI>();
        if (text != null)
            text.text = placementModeText;

        var image = UnitRecallBtn.GetComponent<Image>();
        if (image != null)
            image.color = PlacementModeColor;

        if (UnitRecallBtn != null) UnitRecallBtn.SetActive(false);
        IsInPlacementMode = true; // �⺻�� ��ġ ���
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

        //������ �ϳ��� ������ ��� �ʱ�ȭ
        if (!hasAnyUnit)
        {
            IsInPlacementMode = true;

            // ��� �ؽ�Ʈ ���븸 �ʱ�ȭ
            if (PlacementStatusText != null)
            {
                var text = PlacementStatusText.GetComponent<TextMeshProUGUI>();
                if (text != null)
                    text.text = placementModeText;
            }

            // ��ư ���� �ʱ�ȭ
            var image = UnitRecallBtn.GetComponent<Image>();
            if (image != null)
                image.color = PlacementModeColor;

            // ��ư�� ��Ȱ��ȭ
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
            Debug.LogError("unit, unitBoxPrefab �Ǵ� contentParent�� null�Դϴ�.");
            return;
        }

        string unitKey = unit.unitName.Trim().ToLower();

        // �̹� �����ϴ� ���� �ڽ���� ������ ����
        if (unitBoxMap.TryGetValue(unitKey, out GameObject existingBox))
        {
            UnitBox boxComponent = existingBox.GetComponent<UnitBox>();
            if (boxComponent != null)
            {
                boxComponent.IncreaseUnitCount(1);
            }
            else
            {
                Debug.LogWarning($"[UnitBox ����] {unit.unitName} ������Ʈ�� UnitBox ������Ʈ�� �����ϴ�.");
            }
            return;
        }

        // ���ο� �ڽ� ����
        GameObject newBox = Instantiate(unitBoxPrefab, contentParent);
        if (newBox == null)
        {
            Debug.LogError($"[���� ����] UnitBox ������ ���� ����: {unit.unitName}");
            return;
        }
        newBox.transform.localScale = Vector3.one;

        // ������ ����
        Image icon = newBox.transform.Find("UnitImage").GetComponent<Image>();
        if (icon != null && unit.unitIllustration != null)
            icon.sprite = unit.unitIllustration.sprite;

        // ���� �̸� ����
        TextMeshProUGUI nameText = newBox.transform.Find("UnitTexts/UnitNameText").GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = unit.unitName;

        // ���� �ؽ�Ʈ
        TextMeshProUGUI countText = newBox.transform.Find("UnitTexts/UnitCountText").GetComponent<TextMeshProUGUI>();
        if (countText == null)
        {
            Debug.LogWarning($"[UI ����] {unit.unitName}�� UnitCountText�� �����ϴ�.");
        }

        // UnitBox �ʱ�ȭ
        UnitBox boxComponentNew = newBox.GetComponent<UnitBox>();
        if (boxComponentNew != null)
        {
            boxComponentNew.Init(unit, 1, countText);
        }
        else
        {
            Debug.LogWarning($"[UnitBox ����] {unit.unitName} ������Ʈ�� UnitBox ������Ʈ�� �����ϴ�.");
        }

        // ��ųʸ��� ���� (����ȭ�� Ű)
        unitBoxMap[unitKey] = newBox;
    }
}

