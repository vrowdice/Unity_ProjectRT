using TMPro;
using UnityEngine;

public class EnemyBattleBeforeUI : MonoBehaviour
{
    [Header("�� ���� ���� �ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI rangeUnitCountText;
    [SerializeField] private TextMeshProUGUI meleeUnitCountText;
    [SerializeField] private TextMeshProUGUI defenseUnitCountText;

    private const string NumFormat = "0"; // 3�ڸ� 0�е�

    private void OnEnable()
    {
        // ���� �� 1ȸ ����
        UpdateDeployedUnitCounters();

        if (BattleSystemManager.Instance != null)
            BattleSystemManager.Instance.UnitsChanged += UpdateDeployedUnitCounters;
    }

    private void OnDisable()
    {
        if (BattleSystemManager.Instance != null)
            BattleSystemManager.Instance.UnitsChanged -= UpdateDeployedUnitCounters;
    }

    public void UpdateDeployedUnitCounters()
    {
        var mgr = BattleSystemManager.Instance;
        if (mgr == null) return;

        var counts = mgr.GetEnemyCountsInSpawnAreas();

        if (rangeUnitCountText) rangeUnitCountText.text = counts[UnitTagType.Range].ToString(NumFormat);
        if (meleeUnitCountText) meleeUnitCountText.text = counts[UnitTagType.Melee].ToString(NumFormat);
        if (defenseUnitCountText) defenseUnitCountText.text = counts[UnitTagType.Defense].ToString(NumFormat);
    }
}
