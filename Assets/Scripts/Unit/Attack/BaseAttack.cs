using System;
using System.Collections;
using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    [Header("공격 데이터")]
    public float attackRange;

    // 상태
    public bool IsAttacking { get; protected set; } // Active 구간(실제 공격 동작) 중
    public bool IsBusy { get; protected set; }      // Active+Recovery 전체 사이클 중
    public float ActiveSec { get; protected set; }
    public float RecoverySec { get; protected set; }

    // 내부 참조
    protected UnitBase owner;
    protected GameObject target;
    private Coroutine attackCoroutine;

    // 종료 보장/중복 방지용
    protected UnitBase attackerCache;
    private bool _cycleEndedRaised = false;

    // 이벤트
    public event Action<UnitBase> OnAttackCycleStarted;
    public event Action<UnitBase> OnAttackCycleEnded;
    public event Action<UnitBase, int> OnHit;

    protected void RaiseAttackCycleStarted(UnitBase attacker)
    {
        attackerCache = attacker;   // 강제중단 대비
        _cycleEndedRaised = false;      // 새 사이클 시작
        OnAttackCycleStarted?.Invoke(attacker);
    }
    protected void RaiseAttackCycleEnded(UnitBase attacker)
    {
        if (_cycleEndedRaised) return;  // 중복 방지
        _cycleEndedRaised = true;
        OnAttackCycleEnded?.Invoke(attacker);
        attackerCache = null;
    }
    protected void RaiseHit(UnitBase attacker, int hitIndex) => OnHit?.Invoke(attacker, hitIndex);

    public void SetTempo(float activeSec, float recoverySec)
    {
        ActiveSec = Mathf.Max(0.0f, activeSec);
        RecoverySec = Mathf.Max(0.0f, recoverySec);
    }

    public virtual bool TryStartSkill(UnitBase attacker, GameObject targetEnemy) => false;

    protected bool CanRunCoroutine() => isActiveAndEnabled && gameObject.activeInHierarchy;
    protected Coroutine StartRoutineSafe(IEnumerator routine) => CanRunCoroutine() ? StartCoroutine(routine) : null;

    public void StartAttack(UnitBase attacker, GameObject targetEnemy)
    {
        if (IsBusy) return;
        if (attacker == null || targetEnemy == null || !targetEnemy.activeSelf) return;
        if (!CanRunCoroutine()) return; // 비활성일 때 시작 금지

        owner = attacker;
        target = targetEnemy;

        attackerCache = attacker;
        _cycleEndedRaised = false;

        attackCoroutine = StartRoutineSafe(AttackCycle());
    }

    public void StopAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = null;

        // 사이클 도중 강제 중단 시에도 종료 이벤트를 한 번 보장
        if (!_cycleEndedRaised && attackerCache != null)
            RaiseAttackCycleEnded(attackerCache);

        IsAttacking = false;
        IsBusy = false;

        owner = null;
        target = null;
        attackerCache = null;
    }

    private IEnumerator AttackCycle()
    {
        if (!CanRunCoroutine()) yield break;

        IsBusy = true;
        IsAttacking = true;

        if (owner != null && target != null)
        {
            owner.NotifyAttackActiveBegin();
            UnitImpactEmitter.Emit(owner.gameObject, ImpactEventType.BasicAttackStart, owner, target, 0.0f, null);
            owner.NotifyBasicAttackStart(target);
        }

        // 실제 공격 동작(파생 클래스 구현)
        yield return PerformAttackRoutine(owner, target);

        // Active 종료
        IsAttacking = false;
        owner?.NotifyAttackActiveEnd();

        // 후딜
        if (RecoverySec > 0.0f)
            yield return new WaitForSeconds(RecoverySec);

        // 파생이 종료 이벤트를 깜박해도 여기서 1회 보장
        if (!_cycleEndedRaised && owner != null)
            RaiseAttackCycleEnded(owner);

        IsBusy = false;
        attackCoroutine = null;

        // 참조 정리(메모리/풀링 안전)
        owner = null;
        target = null;
    }

    protected abstract IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target);

    protected void ApplyDamage(UnitBase attacker, GameObject target, float rawDamage)
    {
        if (!IsTargetAlive(target)) return;

        var unit = target.GetComponent<UnitBase>();
        if (unit == null) return;
        if (unit.Team == attacker.Team) return; // 아군 보호

        unit.TakeDamage(rawDamage);
        attacker.OnBasicAttackLanded();
        attacker.NotifyBasicAttackHit(target, rawDamage);

        UnitImpactEmitter.Emit(attacker.gameObject, ImpactEventType.BasicAttackHit, attacker, target, rawDamage, null);
        UnitImpactEmitter.Emit(target, ImpactEventType.Damaged, attacker, target, rawDamage, null);
    }

    protected float GetRange(UnitBase attacker)
        => (attackRange > 0.0f) ? attackRange : attacker.AttackRange;

    protected bool IsTargetAlive(GameObject go)
    {
        if (go == null || !go.activeSelf) return false;
        var ub = go.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    protected bool InEffectiveRange(Transform self, Transform tgt, float mul = 1.0f)
    {
        if (self == null || tgt == null) return false;
        float r = GetRange(owner) * mul;
        return Vector3.Distance(self.position, tgt.position) <= r;
    }

    private void OnDisable() => StopAttack();
}
