using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitBox : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject attackSpawnArea;

    private UnitStatBase unitData;
    private int unitCount;
    private int createdCount = 0;

    private TextMeshProUGUI countText;
    private Image buttonImage;
    private Color defaultColor;
    private bool hasColorChanged = false;

    public void Init(UnitStatBase data, int count, TextMeshProUGUI countTextRef)
    {
        unitData = data;
        unitCount = count;
        createdCount = 0;
        countText = countTextRef;
        attackSpawnArea = GameObject.Find("AttackSpawnArea");

        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            defaultColor = buttonImage.color;
            buttonImage.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.5f); // �帮��
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickUnitButton);
        }

        UpdateCountText();
    }

    public void OnClickUnitButton()
    {
        if (unitCount <= 0 || unitPrefab == null || attackSpawnArea == null) return;

        Vector3 spawnPos = GetRandomPositionInBox();
        GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);

        //unitType �� �±׸� ��ȯ ����
        Dictionary<string, string> typeToTagMap = new()
    {
        { "�ٰŸ�", "ShortUnit" },
        { "���Ÿ�", "LongUnit" },
        { "���", "DefenseUnit" }
        
    };

        //�±� ����
        if (!string.IsNullOrEmpty(unitData.unitType))
        {
            if (typeToTagMap.TryGetValue(unitData.unitType, out string tagName))
            {
                try
                {
                    newUnit.tag = tagName;
                }
                catch
                {
                    Debug.LogWarning($"[�±� ����] '{tagName}' �±װ� ��ȿ���� �ʰų� ��ϵ��� �ʾҽ��ϴ�.");
                }
            }
            else
            {
                Debug.LogWarning($"[�±� ���� ����] '{unitData.unitType}'�� ���� �±װ� ��ϵǾ� ���� �ʽ��ϴ�.");
            }
        }

        // UnitStatBase ���� ����
        UnitBase unitBase = newUnit.GetComponent<UnitBase>();
        if (unitBase != null)
        {
            unitBase.Initialize(unitData);
        }
        else
        {
            Debug.LogWarning("[���� ����] ������ ���ֿ� UnitBase ������Ʈ�� �����ϴ�.");
        }

        // ���� �� UI ó��
        unitCount--;
        createdCount++;
        UpdateCountText();

        if (!hasColorChanged && buttonImage != null && createdCount >= 1)
        {
            buttonImage.color = defaultColor;
            hasColorChanged = true;
        }

        if (unitCount <= 0)
        {
            GetComponent<Button>().interactable = false;
        }
    }

    private void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = unitCount.ToString();
        }
    }

    private Vector3 GetRandomPositionInBox()
    {
        BoxCollider2D box = attackSpawnArea.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogWarning("BoxCollider2D�� �����ϴ�.");
            return Vector3.zero;
        }

        Bounds bounds = box.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(x, y, 0f);
    }
}
