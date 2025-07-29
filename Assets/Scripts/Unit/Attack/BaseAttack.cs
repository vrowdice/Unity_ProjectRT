using System.Collections;
using UnityEngine;

// 기본 공격(추상)
public abstract class BaseAttack : MonoBehaviour
{
    protected bool _isAttacking = false;
    public bool IsAttacking => _isAttacking;

    public void StartAttack(UnitBase attacker, GameObject target)
    {
        if (_isAttacking)
        {
            Debug.LogWarning($"{attacker.unitName}의 공격 로직이 이미 진행 중");
            return;
        }
        if (target == null)
        {
            Debug.LogWarning($"{attacker.unitName}의 공격 대상이 없음");
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
        Debug.Log("공격 루틴 강제 중지.");
    }

    protected virtual void ApplyDamage(UnitBase attacker, GameObject target, float rawDamage)
    {
        if (target == null) return;

        UnitBase targetUnit = target.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(rawDamage);
            Debug.Log($"-> {targetUnit.unitName}에게 {rawDamage:F2} 피해를 입힘. (공격자: {attacker.unitName})");
        }
    }
}
