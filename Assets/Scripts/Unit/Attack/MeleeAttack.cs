using System.Collections;
using UnityEngine;

public class MeleeAttack : BaseAttack
{
    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target)
    {
        IsAttacking = true;

        // PascalCase ���� ���
        float intervalBetweenHits = attacker.AttackFrequency / attacker.AttackCount;

        // PascalCase ���� ���
        Debug.Log($"{attacker.UnitName} ���� ������ ����");

        // PascalCase ���� ���
        for (int i = 0; i < attacker.AttackCount; i++)
        {
            if (target == null || !target.activeSelf)
            {
                StopAttack();
                yield break;
            }

            // PascalCase ���� ���
            float rawDamage = attacker.AttackPower * attacker.DamageCoefficient;
            ApplyDamage(attacker, target, rawDamage);

            yield return new WaitForSeconds(intervalBetweenHits);
        }

        IsAttacking = false;
        // PascalCase ���� ���
        Debug.Log($"{attacker.UnitName}�� ���� ������ ��.");
    }
}