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

    // 런타임 내부에서만 쓰는 로컬 복사본 (원본 BSM 리스트는 건드리지 않음)
    private List<UnitBase> allies;
    private List<UnitBase> enemies;

    private bool running;
    private float elapsed;

    private Vector3 allyForward;
    private Vector3 enemyForward;

    // === 전투 시작: BSM에서 호출 ===
    public void Begin(List<UnitBase> ally, List<UnitBase> enemy, bool /*isPlayerAttacker*/ _)
    {
        // 방어적 복사: 외부 리스트는 변경하지 않음
        allies = new List<UnitBase>(ally ?? new List<UnitBase>());
        enemies = new List<UnitBase>(enemy ?? new List<UnitBase>());

        allyForward = ComputeAllyForward();   // 스폰 구역 기준 +x 또는 -x
        enemyForward = -allyForward;

        // 의존성 보장 후 전투 초기화
        foreach (var u in allies) StartUnit(u, allyForward);
        foreach (var u in enemies) StartUnit(u, enemyForward);

        running = true;
        elapsed = 0f;
    }

    // 스폰 두 구역 중심을 비교해서 아군의 전진 방향 결정
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
                if (Mathf.Approximately(sign, 0f)) sign = 1f; // 같은 곳이면 +x
                return new Vector3(sign, 0f, 0f);
            }
        }
        return Vector3.right; // 폴백
    }

    // 유닛 하나 전투 준비
    private void StartUnit(UnitBase u, Vector3 forward)
    {
        if (!u) return;

        // 1) 의존성 먼저 보장(RequireComponent 자동 추가 로그 방지)
        if (!u.GetComponent<UnitMovementController>())
            u.gameObject.AddComponent<UnitMovementController>();

        if (!u.GetComponent<UnitTargetingController>())
            u.gameObject.AddComponent<UnitTargetingController>();

        // 2) Combat Controller 준비
        var cc = u.GetComponent<UnitCombatController>();
        if (!cc) cc = u.gameObject.AddComponent<UnitCombatController>();

        // 3) 초기화(전진 시작 포함)
        cc.InitForBattle(forward);
    }

    // 모든 유닛 전투 정지
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

        // 죽었거나 비활성화된 유닛 제거 (로컬 복사본만 정리)
        allies?.RemoveAll(u => !IsAlive(u));
        enemies?.RemoveAll(u => !IsAlive(u));

        // 한 쪽이 전멸하면 종료
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
        if (!running) return;
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
