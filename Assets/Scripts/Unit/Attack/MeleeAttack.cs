using System.Collections;
using UnityEngine;

// 근접 공격
public class MeleeAttack : BaseAttack
{
    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target)
    {
        IsAttacking = true;

        float intervalBetweenHits = attacker.attackFrequency / attacker.attackCount;

        Debug.Log($"{attacker.unitName} 근접 공격을 시작");

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
        Debug.Log($"{attacker.unitName}의 근접 공격이 끝.");
    }
}