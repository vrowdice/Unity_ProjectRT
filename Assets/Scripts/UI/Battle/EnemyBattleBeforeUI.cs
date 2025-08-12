using TMPro;
using UnityEngine;

public class EnemyBattleBeforeUI : MonoBehaviour
{
    [Header("적 유닛 개수 텍스트")]
    [SerializeField] private TextMeshProUGUI rangeUnitCountText;
    [SerializeField] private TextMeshProUGUI meleeUnitCountText;
    [SerializeField] private TextMeshProUGUI defenseUnitCountText;

    private const string NumFormat = "0"; // 3자리 0패딩

    private void OnEnable()
    {
        // 켜질 때 1회 갱신
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
