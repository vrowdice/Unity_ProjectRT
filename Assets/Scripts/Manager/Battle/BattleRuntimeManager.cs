using System;
using System.Collections.Generic;
using UnityEngine;

public struct BattleOutcome
{
    public bool victory;
    public int allyRemaining;
    public int enemyRemaining;
    public float elapsed;
}

public class BattleRuntimeManager : MonoBehaviour
{
    public Action<BattleOutcome> OnBattleFinished;

    private List<UnitBase> allies;
    private List<UnitBase> enemies;

    private bool running;
    private float elapsed;

    private Vector3 allyForward;
    private Vector3 enemyForward;

    public void Begin(List<UnitBase> ally, List<UnitBase> enemy, bool /*isPlayerAttacker*/ _)
    {
        allies = new List<UnitBase>(ally ?? new List<UnitBase>());
        enemies = new List<UnitBase>(enemy ?? new List<UnitBase>());

        // 시작 즉시 전멸 케이스(튜토리얼 등 제외 시 바로 종료)
        if ((allies?.Count ?? 0) == 0 || (enemies?.Count ?? 0) == 0)
        {
            Finish();
            return;
        }

        allyForward = ComputeAllyForward();
        enemyForward = -allyForward;

        foreach (var u in allies) StartUnit(u, allyForward);
        foreach (var u in enemies) StartUnit(u, enemyForward);

        running = true;
        elapsed = 0.0f;
    }

    private Vector3 ComputeAllyForward()
    {
        var mgr = BattleSystemManager.Instance;
        if (mgr != null)
        {
            var a = mgr.GetAllySpawnArea();
            var e = mgr.GetEnemySpawnArea();
            if (a && e)
            {
                float dx = e.bounds.center.x - a.bounds.center.x;
                float sign = Mathf.Sign(dx);
                if (Mathf.Approximately(sign, 0.0f)) sign = 1.0f;
                return new Vector3(sign, 0.0f, 0.0f);
            }
        }
        return Vector3.right;
    }

    private void StartUnit(UnitBase u, Vector3 forward)
    {
        if (!u) return;

        if (!u.GetComponent<UnitMovementController>()) u.gameObject.AddComponent<UnitMovementController>();
        if (!u.GetComponent<UnitTargetingController>()) u.gameObject.AddComponent<UnitTargetingController>();

        var cc = u.GetComponent<UnitCombatController>();
        if (!cc) cc = u.gameObject.AddComponent<UnitCombatController>();
        cc.InitForBattle(forward);
    }

    private void StopAll()
    {
        if (allies != null)
            foreach (var u in allies) u?.GetComponent<UnitCombatController>()?.StopBattle();

        if (enemies != null)
            foreach (var u in enemies) u?.GetComponent<UnitCombatController>()?.StopBattle();
    }

    private void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;

        // 사망/비활성 제거
        allies?.RemoveAll(u => !IsAlive(u));
        enemies?.RemoveAll(u => !IsAlive(u));

        // 한쪽 전멸 → 종료
        if (allies == null || enemies == null || allies.Count == 0 || enemies.Count == 0)
        {
            Finish();
        }
    }

    private static bool IsAlive(UnitBase u)
    {
        return u && u.gameObject.activeSelf && !u.IsDead;
    }

    private void Finish()
    {
        if (!running)
        {
            // Begin에서 즉시 Finish 호출된 케이스도 outcome은 내보낸다
        }

        running = false;
        StopAll();

        var outcome = new BattleOutcome
        {
            victory = allies != null && allies.Count > 0,
            allyRemaining = allies?.Count ?? 0,
            enemyRemaining = enemies?.Count ?? 0,
            elapsed = elapsed
        };

        OnBattleFinished?.Invoke(outcome);
        Destroy(this);
    }
}
