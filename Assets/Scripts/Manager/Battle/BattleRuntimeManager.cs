using System;
using System.Collections.Generic;
using UnityEngine;

#region Result DTO
public struct BattleOutcome
{
    public bool victory;
    public int allyRemaining;
    public int enemyRemaining;
    public float elapsed;
}
#endregion

[DisallowMultipleComponent]
public sealed class BattleRuntimeManager : MonoBehaviour
{
    public Action<BattleOutcome> OnBattleFinished;

    // 내부 리스트(재사용)
    private readonly List<UnitBase> allies = new();
    private readonly List<UnitBase> enemies = new();

    private readonly List<UnitCombatController> allyCC = new();
    private readonly List<UnitCombatController> enemyCC = new();

    private bool running;
    private float elapsed;

    private Vector3 allyForward = Vector3.right;
    private Vector3 enemyForward = Vector3.left;

    public void Begin(IReadOnlyList<UnitBase> ally, IReadOnlyList<UnitBase> enemy, bool /*isPlayerAttacker*/ _)
    {
        // 내부 상태 초기화
        allies.Clear();
        enemies.Clear();
        allyCC.Clear();
        enemyCC.Clear();
        elapsed = 0f;

        int aCount = ally?.Count ?? 0;
        int eCount = enemy?.Count ?? 0;

        Reserve(allies, aCount);
        Reserve(enemies, eCount);
        Reserve(allyCC, aCount);
        Reserve(enemyCC, eCount);

        if (aCount > 0)
        {
            for (int i = 0; i < aCount; i++)
            {
                var u = ally[i];
                if (IsAlive(u)) allies.Add(u);
            }
        }
        if (eCount > 0)
        {
            for (int i = 0; i < eCount; i++)
            {
                var u = enemy[i];
                if (IsAlive(u)) enemies.Add(u);
            }
        }

        // 시작 즉시 전멸 케이스
        if (allies.Count == 0 || enemies.Count == 0)
        {
            Finish();
            return;
        }

        ComputeForwards(out allyForward, out enemyForward);

        WarmupAndInit(allies, allyForward, allyCC);
        WarmupAndInit(enemies, enemyForward, enemyCC);

        running = true;
    }

    private static void Reserve<T>(List<T> list, int needed)
    {
        if (needed <= 0) return;
        int cap = list.Capacity;
        if (cap >= needed) return;

        int target = cap > 0 ? cap : 4;
        while (target < needed) target <<= 1;
        list.Capacity = target;
    }

    // 스폰 구역 기준으로 아군 전진 방향 결정
    private static void ComputeForwards(out Vector3 allyFwd, out Vector3 enemyFwd)
    {
        var mgr = BattleSystemManager.Instance;
        if (mgr)
        {
            var a = mgr.GetAllySpawnArea();
            var e = mgr.GetEnemySpawnArea();
            if (a && e)
            {
                float dx = e.bounds.center.x - a.bounds.center.x;
                float sign = Mathf.Sign(dx);
                if (Mathf.Approximately(sign, 0.0f)) sign = 1.0f;
                allyFwd = new Vector3(sign, 0.0f, 0.0f);
                enemyFwd = -allyFwd;
                return;
            }
        }
        allyFwd = Vector3.right;
        enemyFwd = Vector3.left;
    }

    private static void WarmupAndInit(List<UnitBase> units, Vector3 forward, List<UnitCombatController> cacheCC)
    {
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            if (!u) continue;

            if (!u.TryGetComponent<UnitMovementController>(out _))
                u.gameObject.AddComponent<UnitMovementController>();

            if (!u.TryGetComponent<UnitTargetingController>(out _))
                u.gameObject.AddComponent<UnitTargetingController>();

            if (!u.TryGetComponent<UnitCombatController>(out var cc))
                cc = u.gameObject.AddComponent<UnitCombatController>();

            cc.InitForBattle(forward);
            cacheCC.Add(cc);
        }
    }

    private void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;

        // 생존자만 남기는 제자리 필터
        CompactAlive(allies);
        CompactAlive(enemies);

        // 전멸 확인
        if (allies.Count == 0 || enemies.Count == 0)
        {
            Finish();
        }
    }

    private static void CompactAlive(List<UnitBase> list)
    {
        int write = 0;
        int count = list.Count;
        for (int read = 0; read < count; read++)
        {
            var u = list[read];
            if (IsAlive(u))
            {
                if (write != read) list[write] = u;
                write++;
            }
        }
        if (write < count)
            list.RemoveRange(write, count - write);
    }

    private static bool IsAlive(UnitBase u)
        => u && u.gameObject.activeSelf && !u.IsDead;

    private void StopAll()
    {
        for (int i = 0; i < allyCC.Count; i++)
            allyCC[i]?.StopBattle();
        for (int i = 0; i < enemyCC.Count; i++)
            enemyCC[i]?.StopBattle();
    }

    private void Finish()
    {
        bool victory = allies.Count > 0;

        running = false;
        StopAll();

        var outcome = new BattleOutcome
        {
            victory = victory,
            allyRemaining = allies.Count,
            enemyRemaining = enemies.Count,
            elapsed = elapsed
        };

        OnBattleFinished?.Invoke(outcome);
        Destroy(this);
    }
}
