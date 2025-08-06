using System.Collections;
using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    [Header("���� ������")]
    public float attackRange;

    // ���� ����
    public bool IsAttacking { get; protected set; } = false;

    protected UnitBase owner;
    protected GameObject target;
    private Coroutine attackCoroutine;

    // ���� ������ �ڷ�ƾ���� �����ϱ� ���� �޼���
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

    // ���� Ŭ�������� �ݵ�� �����ؾ� �� �߻� �ڷ�ƾ �޼���
    protected abstract IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target);

    // ApplyDamage �޼���� BaseAttack Ŭ������ ����
    protected void ApplyDamage(UnitBase attacker, GameObject target, float rawDamage)
    {
        if (target == null || !target.activeSelf) return;

        UnitBase targetUnit = target.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(rawDamage);
            Debug.Log($"{attacker.unitName}��(��) {targetUnit.unitName}���� {rawDamage}�� ���ظ� �������ϴ�.");

            attacker.currentMana += attacker.manaRecoveryOnBasicAttack;
        }
    }
}