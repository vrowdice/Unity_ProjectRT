using System.Collections;
using UnityEngine;

public class RangedAttack : BaseAttack
{
    [Header("발사체")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    [Header("전달 방식")]
    public HitDeliveryStrategy delivery = HitDeliveryStrategy.Projectile;

    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject targetGO)
    {
        if (attacker == null || !IsTargetAlive(targetGO)) yield break;
        if (!InEffectiveRange(attacker.transform, targetGO.transform, 1.05f)) yield break;

        int shots = Mathf.Max(1, attacker.AttackCount);
        float act = ActiveSec;                               
        float step = (shots > 1 && act > 0.0f) ? act / shots : 0; // 연사 간 간격

        for (int i = 0; i < shots; i++)
        {
            if (!IsTargetAlive(targetGO)) break;
            if (i > 0 && step > 0f) yield return new WaitForSeconds(step);

            float rawDamage = attacker.AttackPower * attacker.DamageCoefficient;
            var origin = (shootPoint != null) ? shootPoint : attacker.transform;

            HitDeliverer.Deliver(
                attacker,
                targetGO,
                rawDamage,
                delivery,
                projectilePrefab,
                origin,
                contextName: name);
        }

        float consumed = (shots > 1) ? step * (shots - 1) : 0.0f;
        float remain = Mathf.Max(0f, act - consumed);
        if (remain > 0.0f) yield return new WaitForSeconds(remain);
    }
}
