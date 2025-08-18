using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class EnemyBattleBeforeUI : MonoBehaviour
{
    [Header("적 유닛 개수 텍스트")]
    [SerializeField] private TextMeshProUGUI rangeUnitCountText;
    [SerializeField] private TextMeshProUGUI meleeUnitCountText;
    [SerializeField] private TextMeshProUGUI defenseUnitCountText;

    [Header("표시 옵션")]
    [SerializeField] private bool padTo3Digits = true; 
    private const string PadFormat = "D3";

    private void OnEnable()
    {
        UpdateDeployedUnitCounters(); // 켜질 때 1회 갱신
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
        if (counts == null) 
        {
            WriteCount(rangeUnitCountText, 0);
            WriteCount(meleeUnitCountText, 0);
            WriteCount(defenseUnitCountText, 0);
            return;
        }

        WriteCount(rangeUnitCountText, GetSafe(counts, UnitTagType.Range));
        WriteCount(meleeUnitCountText, GetSafe(counts, UnitTagType.Melee));
        WriteCount(defenseUnitCountText, GetSafe(counts, UnitTagType.Defense));
    }

    [ContextMenu("Force Update")]
    private void ForceUpdateInEditor() => UpdateDeployedUnitCounters();

    private static int GetSafe(Dictionary<UnitTagType, int> map, UnitTagType key)
        => map != null && map.TryGetValue(key, out int v) ? v : 0;

    private void WriteCount(TextMeshProUGUI label, int value)
    {
        if (!label) return;
        label.text = padTo3Digits ? value.ToString(PadFormat) : value.ToString();
    }
}
