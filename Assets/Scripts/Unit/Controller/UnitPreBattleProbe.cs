using UnityEngine;

public class UnitPreBattleProbe : MonoBehaviour
{
    [ContextMenu("Probe Units")]
    public void Probe()
    {
        var all = FindObjectsOfType<UnitBase>(true);
        if (all.Length == 0) { Debug.LogWarning("[Probe] ¾À¿¡ UnitBase°¡ ¾øÀ½"); return; }

        int aliveAllies = 0, aliveEnemies = 0;
        foreach (var u in all)
        {
            string go = u.gameObject.name;
            bool hasStat = u.UnitStat != null;
            float hp = u.CurrentHealth;
            float max = u.MaxHealth;
            var faction = u.Faction;  
            var team = u.Team;

            string why = "";
            if (!hasStat) why += " [NO_STAT]";
            if (max <= 0f) why += " [MAX_HEALTH=0]";
            if (hp <= 0f) why += " [DEAD]";

            Debug.Log($"[Probe] {go} team={team} faction={faction} hp={hp}/{max} stat={(hasStat ? u.UnitStat.name : "null")}{why}");

            if (!u.IsDead && team == TeamSide.Ally) aliveAllies++;
            if (!u.IsDead && team == TeamSide.Enemy) aliveEnemies++;
        }

        Debug.Log($"[Probe] Alive Allies={aliveAllies}, Alive Enemies={aliveEnemies}");
    }
}
