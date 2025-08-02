using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBattleBeforeUI : MonoBehaviour
{
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
        FirstSettingCountText();
    }

    // Update is called once per frame
    void Update()
    {
        CountEnemyUnit();
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
        { UnitTagType.Long, "LongUnit"},
        { UnitTagType.Short, "ShortUnit" },
        { UnitTagType.Defense, "DefenseUnit"}
    };

        tagTexts = new Dictionary<UnitTagType, TextMeshProUGUI>
    {
        { UnitTagType.Long, LongUnitCountTextMesh},
        { UnitTagType.Short, ShortUnitCountTextMesh},
        { UnitTagType.Defense, DefenseUnitCountTextMesh}
    };
    }

    private void CountEnemyUnit()
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

            tagTexts[tagType].text = count.ToString();
        }
    }
}
