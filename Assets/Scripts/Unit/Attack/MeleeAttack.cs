using System.Collections;
using UnityEngine;

public class MeleeAttack : BaseAttack
{
    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target)
    {
        IsAttacking = true;

        // PascalCase 변수 사용
        float intervalBetweenHits = attacker.AttackFrequency / attacker.AttackCount;

        // PascalCase 변수 사용
        Debug.Log($"{attacker.UnitName} 근접 공격을 시작");

        // PascalCase 변수 사용
        for (int i = 0; i < attacker.AttackCount; i++)
        {
            if (target == null || !target.activeSelf)
            {
                StopAttack();
                yield break;
            }

            // PascalCase 변수 사용
            float rawDamage = attacker.AttackPower * attacker.DamageCoefficient;
            ApplyDamage(attacker, target, rawDamage);

            yield return new WaitForSeconds(intervalBetweenHits);
        }

        IsAttacking = false;
        // PascalCase 변수 사용
        Debug.Log($"{attacker.UnitName}의 근접 공격이 끝.");
    }
}