using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitBox : MonoBehaviour
{
    // === UI 참조 ===
    [SerializeField] private Image unitIcon;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button spawnButton;

    // === 데이터 ===
    private UnitStatBase unitStat;
    private int currentCount;
    private BattleBeforeUI battleBeforeUI;

    public int CurrentCount => currentCount;

    private void Awake()
    {
        if (spawnButton)
        {
            spawnButton.onClick.AddListener(OnSpawnButtonClicked);
        }
    }

    public void Init(UnitStatBase stat, int count, BattleBeforeUI beforeUI)
    {
        if (stat != null)
        {
            Debug.Log($"[UnitBox] '{stat.unitName}'에 대한 Init 함수 호출. UnitStat 데이터 정상.");
        }
        else
        {
            Debug.LogError("[UnitBox] Init 함수 호출 시 UnitStat 데이터가 null입니다.");
        }

        unitStat = stat;
        currentCount = count;
        battleBeforeUI = beforeUI;

        UpdateUI();
    }

    public void IncreaseUnitCount(int amount)
    {
        currentCount += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (unitStat != null)
        {
            if (unitIcon) unitIcon.sprite = unitStat.unitIcon;
            if (unitNameText) unitNameText.text = unitStat.unitName;
        }

        if (countText)
        {
            countText.text = currentCount.ToString();
            if (spawnButton) spawnButton.interactable = currentCount > 0;
        }
    }

    private void OnSpawnButtonClicked()
    {
        if (BattleBeforeUI.IsInPlacementMode)
        {
            if (currentCount <= 0) return;
            battleBeforeUI.RequestPlaceUnit(unitStat);
        }
        else
        {
            Debug.Log("회수 모드에서는 씬에 있는 유닛을 직접 클릭하여 회수해야 합니다.");
        }
    }
}