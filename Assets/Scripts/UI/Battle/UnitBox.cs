using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitBox : MonoBehaviour
{
    // === UI ���� ===
    [SerializeField] private Image unitIcon;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button spawnButton;

    // === ������ ===
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
            Debug.Log($"[UnitBox] '{stat.unitName}'�� ���� Init �Լ� ȣ��. UnitStat ������ ����.");
        }
        else
        {
            Debug.LogError("[UnitBox] Init �Լ� ȣ�� �� UnitStat �����Ͱ� null�Դϴ�.");
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
            Debug.Log("ȸ�� ��忡���� ���� �ִ� ������ ���� Ŭ���Ͽ� ȸ���ؾ� �մϴ�.");
        }
    }
}