using System.Collections;
using UnityEngine;

// ���� ���� 
public class MeleeAttack : BaseAttack
{
    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target)
    {
        _isAttacking = true; 

        float rawDamage = attacker.attackPower * attacker.damageCoefficient * attacker.attackCount;
        ApplyDamage(attacker, target, rawDamage); 

        attacker.currentMana += attacker.manaRecoveryOnBasicAttack;

        yield return new WaitForSeconds(attacker.attackFrequency);

        _isAttacking = false; 
        Debug.Log($"{attacker.unitName}�� ���� ���� ��ٿ� ����.");
    }
}