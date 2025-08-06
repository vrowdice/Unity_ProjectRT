using System.Collections;
using UnityEngine;

// ���� ����
public class MeleeAttack : BaseAttack
{
    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target)
    {
        IsAttacking = true;

        float intervalBetweenHits = attacker.attackFrequency / attacker.attackCount;

        Debug.Log($"{attacker.unitName} ���� ������ ����");

        for (int i = 0; i < attacker.attackCount; i++)
        {
            if (target == null || !target.activeSelf)
            {
                StopAttack();
                yield break;
            }

            float rawDamage = attacker.attackPower * attacker.damageCoefficient;
            ApplyDamage(attacker, target, rawDamage);

            yield return new WaitForSeconds(intervalBetweenHits);
        }

        IsAttacking = false;
        Debug.Log($"{attacker.unitName}�� ���� ������ ��.");
    }
}