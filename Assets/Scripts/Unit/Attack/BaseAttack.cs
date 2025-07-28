using System.Collections;
using UnityEngine;

// �⺻ ����(�߻�)
public abstract class BaseAttack : MonoBehaviour
{
    protected bool _isAttacking = false;
    public bool IsAttacking => _isAttacking;

    public void StartAttack(UnitBase attacker, GameObject target)
    {
        if (_isAttacking)
        {
            Debug.LogWarning($"{attacker.unitName}�� ���� ������ �̹� ���� ��");
            return;
        }
        if (target == null)
        {
            Debug.LogWarning($"{attacker.unitName}�� ���� ����� ����");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PerformAttackRoutine(attacker, target));
    }

    protected abstract IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target);

    public void StopAttack()
    {
        StopAllCoroutines();
        _isAttacking = false; 
        Debug.Log("���� ��ƾ ���� ����.");
    }

    protected virtual void ApplyDamage(UnitBase attacker, GameObject target, float rawDamage)
    {
        if (target == null) return;

        UnitBase targetUnit = target.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(rawDamage);
            Debug.Log($"-> {targetUnit.unitName}���� {rawDamage:F2} ���ظ� ����. (������: {attacker.unitName})");
        }
    }
}
