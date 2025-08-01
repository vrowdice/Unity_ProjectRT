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
            buttonImage.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.5f); // 흐리게
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

        //unitType → 태그명 변환 매핑
        Dictionary<string, string> typeToTagMap = new()
    {
        { "근거리", "ShortUnit" },
        { "원거리", "LongUnit" },
        { "방어", "DefenseUnit" }
        
    };

        //태그 설정
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
                    Debug.LogWarning($"[태그 오류] '{tagName}' 태그가 유효하지 않거나 등록되지 않았습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"[태그 매핑 누락] '{unitData.unitType}'에 대한 태그가 등록되어 있지 않습니다.");
            }
        }

        // UnitStatBase 정보 전달
        UnitBase unitBase = newUnit.GetComponent<UnitBase>();
        if (unitBase != null)
        {
            unitBase.Initialize(unitData);
        }
        else
        {
            Debug.LogWarning("[생성 실패] 생성된 유닛에 UnitBase 컴포넌트가 없습니다.");
        }

        // 수량 및 UI 처리
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
            Debug.LogWarning("BoxCollider2D가 없습니다.");
            return Vector3.zero;
        }

        Bounds bounds = box.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(x, y, 0f);
    }
}
