using System.Collections;
using UnityEngine;

public class RangedAttack : BaseAttack
{
    [Header("�߻�ü")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    [Header("���� ���")]
    public HitDeliveryStrategy delivery = HitDeliveryStrategy.Projectile;

    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject targetGO)
    {
        if (attacker == null || !IsTargetAlive(targetGO)) yield break;
        if (!InEffectiveRange(attacker.transform, targetGO.transform, 1.05f)) yield break;

        int shots = Mathf.Max(1, attacker.AttackCount);
        float act = ActiveSec;                               
        float step = (shots > 1 && act > 0.0f) ? act / shots : 0; // ���� �� ����

        for (int i = 0; i < shots; i++)
        {
            if (i > 0 && step > 0f) yield return new WaitForSeconds(step);
            if (!IsTargetAlive(targetGO)) break;

            if (!InEffectiveRange(attacker.transform, targetGO.transform, 0.9f)) break;

            float rawDamage = attacker.AttackPower * attacker.DamageCoefficient;
            var origin = (shootPoint != null) ? shootPoint : attacker.transform;

            // �߻�ü �̼��� �� ���� ����
            // �׳� ���� ����
            var useDelivery = delivery;
            if (useDelivery == HitDeliveryStrategy.Projectile && projectilePrefab == null)
                useDelivery = HitDeliveryStrategy.Instant; 

            HitDeliverer.Deliver(
                attacker, targetGO, rawDamage, useDelivery,
                projectilePrefab, origin, contextName: name);

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
