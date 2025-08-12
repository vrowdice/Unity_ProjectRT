using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : BaseAttack
{
    public GameObject projectilePrefab;
    public Transform shootPoint;

    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject target)
    {
        IsAttacking = true;

        for (int i = 0; i < attacker.AttackCount; i++)
        {
            if (target == null || !target.activeSelf)
            {
                StopAttack();
                yield break;
            }

            if (projectilePrefab != null && shootPoint != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
               // 공격 데미지
                ProjectileContoller projectileComponent = projectile.GetComponent<ProjectileContoller>();
                if (projectileComponent != null)
                {
                    projectileComponent.Initialize(attacker, target, attacker.AttackPower * attacker.DamageCoefficient);
                }

            }

            yield return new WaitForSeconds(attacker.AttackFrequency / attacker.AttackCount);
        }

        IsAttacking = false;
        Debug.Log(attacker.UnitName + "의 원거리 공격이 끝.");
    }
}