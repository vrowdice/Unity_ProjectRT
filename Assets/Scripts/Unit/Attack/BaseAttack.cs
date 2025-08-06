using System.Collections;
using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    [Header("공격 데이터")]
    public float attackRange;

    // 공격 상태
    public bool IsAttacking { get; protected set; } = false;

    protected UnitBase owner;
    protected GameObject target;
    private Coroutine attackCoroutine;

    // 공격 로직을 코루틴으로 실행하기 위한 메서드
    public void StartAttack(UnitBase attacker, GameObject targetEnemy)
    {
        if (IsAttacking) return;

        owner = attacker;
        target = targetEnemy;
        attackCoroutine = StartCoroutine(PerformAttackRoutine(owner, target));
    }

    public void StopAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        IsAttacking = false;
        owner = null;
        target = null;
    }

    // 하위 클래스에서 반드시 구현해야 할 추상 코루틴 메서드
    protected abstract IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target);

    // ApplyDamage 메서드는 BaseAttack 클래스에 구현
    protected void ApplyDamage(UnitBase attacker, GameObject target, float rawDamage)
    {
        if (target == null || !target.activeSelf) return;

        UnitBase targetUnit = target.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(rawDamage);
            Debug.Log($"{attacker.unitName}이(가) {targetUnit.unitName}에게 {rawDamage}의 피해를 입혔습니다.");

            attacker.currentMana += attacker.manaRecoveryOnBasicAttack;
        }
    }
}