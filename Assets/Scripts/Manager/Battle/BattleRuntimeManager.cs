using System;
using System.Collections.Generic;
using UnityEngine;

public struct BattleOutcome { public bool victory; public int allyRemaining; public int enemyRemaining; public float elapsed; }

[DisallowMultipleComponent]
public sealed class BattleRuntimeManager : MonoBehaviour
{
    public Action<BattleOutcome> OnBattleFinished;

    private readonly List<UnitBase> allies = new();
    private readonly List<UnitBase> enemies = new();
    private readonly List<UnitCombatController> allyCC = new();
    private readonly List<UnitCombatController> enemyCC = new();

    private bool running;
    private float elapsed;

    private Vector3 allyForward = Vector3.right;
    private Vector3 enemyForward = Vector3.left;

    public void Begin(IReadOnlyList<UnitBase> ally, IReadOnlyList<UnitBase> enemy, bool _)
    {
        allies.Clear(); enemies.Clear(); allyCC.Clear(); enemyCC.Clear();
        elapsed = 0.0f;

        if (ally != null) for (int i = 0; i < ally.Count; i++) if (IsAlive(ally[i])) allies.Add(ally[i]);
        if (enemy != null) for (int i = 0; i < enemy.Count; i++) if (IsAlive(enemy[i])) enemies.Add(enemy[i]);

        if (allies.Count == 0 || enemies.Count == 0) { Finish(); return; }

        ComputeForwards(out allyForward, out enemyForward);
        WarmupAndInit(allies, allyForward, allyCC);
        WarmupAndInit(enemies, enemyForward, enemyCC);
        running = true;
    }

    private static void ComputeForwards(out Vector3 a, out Vector3 e)
    {
        var mgr = BattleSystemManager.Instance;
        if (mgr && mgr.GetAllySpawnArea() && mgr.GetEnemySpawnArea())
        {
            float dx = mgr.GetEnemySpawnArea().bounds.center.x - mgr.GetAllySpawnArea().bounds.center.x;
            float s = Mathf.Sign(dx); if (Mathf.Approximately(s, 0f)) s = 1.0f;
            a = new Vector3(s, 0, 0); e = -a; return;
        }
        a = Vector3.right; e = Vector3.left;
    }

    private static void WarmupAndInit(List<UnitBase> units, Vector3 fwd, List<UnitCombatController> cache)
    {
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i]; if (!u) continue;
            if (!u.TryGetComponent<UnitMovementController>(out _)) u.gameObject.AddComponent<UnitMovementController>();
            if (!u.TryGetComponent<UnitTargetingController>(out _)) u.gameObject.AddComponent<UnitTargetingController>();
            if (!u.TryGetComponent<UnitCombatController>(out var cc)) cc = u.gameObject.AddComponent<UnitCombatController>();
            cc.InitForBattle(fwd);
            cache.Add(cc);
        }
    }

    private void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        CompactAlive(allies);
        CompactAlive(enemies);
        if (allies.Count == 0 || enemies.Count == 0) Finish();
    }

    private static void CompactAlive(List<UnitBase> list)
    {
        int w = 0, c = list.Count;
        for (int r = 0; r < c; r++) { var u = list[r]; if (IsAlive(u)) { if (w != r) list[w] = u; w++; } }
        if (w < c) list.RemoveRange(w, c - w);
    }
    private static bool IsAlive(UnitBase u) => u && u.gameObject.activeSelf && !u.IsDead;

    private void StopAll()
    {
        for (int i = 0; i < allyCC.Count; i++) allyCC[i]?.StopBattle();
        for (int i = 0; i < enemyCC.Count; i++) enemyCC[i]?.StopBattle();
    }

    private void Finish()
    {
        bool victory = allies.Count > 0;
        running = false;
        StopAll();
        OnBattleFinished?.Invoke(new BattleOutcome
        {
            victory = victory,
            allyRemaining = allies.Count,
            enemyRemaining = enemies.Count,
            elapsed = elapsed
        });
        Destroy(this);
    }
}
