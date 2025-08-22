using System.Collections;
using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    [Header("공격 데이터")]
    public float attackRange; // 0이면 owner.AttackRange 사용

    public bool IsAttacking { get; protected set; } // 활성 중
    public bool IsBusy { get; protected set; } // 활성+후딜 전체 사이클

    protected UnitBase owner;
    protected GameObject target;
    private Coroutine attackCoroutine;

    protected float ActiveSec { get; private set; } = 0.0f;
    protected float RecoverySec { get; private set; } = 0.0f;

    public void SetTempo(float activeSec, float recoverySec)
    {
        ActiveSec = Mathf.Max(0.0f, activeSec);
        RecoverySec = Mathf.Max(0.0f, recoverySec);
    }

    public virtual bool TryStartSkill(UnitBase attacker, GameObject targetEnemy) => false;

    public void StartAttack(UnitBase attacker, GameObject targetEnemy)
    {
        if (IsBusy) return;
        if (attacker == null || targetEnemy == null || !targetEnemy.activeSelf) return;

        owner = attacker;
        target = targetEnemy;
        attackCoroutine = StartCoroutine(AttackCycle());
    }

    public void StopAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = null;
        IsAttacking = false;
        IsBusy = false;
        owner = null;
        target = null;
    }

    private IEnumerator AttackCycle()
    {
        IsBusy = true;
        IsAttacking = true;

        if (owner != null && target != null)
        {
            owner.NotifyAttackActiveBegin();
            UnitImpactEmitter.Emit(owner.gameObject, ImpactEventType.BasicAttackStart, owner, target, 0.0f, null);
            owner.NotifyBasicAttackStart(target);
        }

        yield return PerformAttackRoutine(owner, target);

        IsAttacking = false;
        owner?.NotifyAttackActiveEnd();

        if (RecoverySec > 0.0f) yield return new WaitForSeconds(RecoverySec);

        IsBusy = false;
        attackCoroutine = null;
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
