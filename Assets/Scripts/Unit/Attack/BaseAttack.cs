using System.Collections;
using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    [Header("공격 데이터")]
    public float attackRange; 

    public bool IsAttacking { get; protected set; }

    protected UnitBase owner;
    protected GameObject target;
    private Coroutine attackCoroutine;

    public void StartAttack(UnitBase attacker, GameObject targetEnemy)
    {
        if (IsAttacking) return;
        if (attacker == null || targetEnemy == null || !targetEnemy.activeSelf) return;

        owner = attacker;
        target = targetEnemy;
        IsAttacking = true;

        // 이펙트, 기본공격 시작
        UnitImpactEmitter.Emit(attacker.gameObject, ImpactEventType.BasicAttackStart, attacker, targetEnemy, 0.0f, null);

        attackCoroutine = StartCoroutine(PerformAttackRoutine(owner, target));
    }

    public void StopAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = null;
        IsAttacking = false;
        owner = null;
        target = null;
    }

    protected abstract IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target);

    protected void ApplyDamage(UnitBase attacker, GameObject target, float rawDamage)
    {
        if (target == null || !target.activeSelf) return;

        var unit = target.GetComponent<UnitBase>();
        if (unit == null) return;

        unit.TakeDamage(rawDamage);
        attacker.AddMana(attacker.ManaRecoveryOnBasicAttack);

        // 이펙트
        UnitImpactEmitter.Emit(attacker.gameObject, target ? ImpactEventType.BasicAttackHit : ImpactEventType.BasicAttackHit, attacker, target, rawDamage, null);
        UnitImpactEmitter.Emit(target, ImpactEventType.Damaged, attacker, target, rawDamage, null);
    }

    protected float GetRange(UnitBase attacker)
    {
        return attackRange > 0.0f ? attackRange : attacker.AttackRange;
    }

    private void OnDisable() => StopAttack();
}
