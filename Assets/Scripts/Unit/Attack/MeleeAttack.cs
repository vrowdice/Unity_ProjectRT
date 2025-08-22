using System.Collections;
using UnityEngine;

public class MeleeAttack : BaseAttack
{
    protected override IEnumerator PerformAttackRoutine(UnitBase attacker, GameObject targetGO)
    {
        if (attacker == null || !IsTargetAlive(targetGO)) yield break;
        if (!InEffectiveRange(attacker.transform, targetGO.transform, 1.05f)) yield break;

        int hits = Mathf.Max(1, attacker.AttackCount);
        float act = ActiveSec;
        float step = (hits > 1 && act > 0.0f) ? act / hits : 0.0f;

        for (int i = 0; i < hits; i++)
        {
            if (i > 0 && step > 0.0f) yield return new WaitForSeconds(step);
            if (!IsTargetAlive(targetGO)) break;

            if (!InEffectiveRange(attacker.transform, targetGO.transform, 1.05f)) break;

            float raw = attacker.AttackPower * attacker.DamageCoefficient;
            ApplyDamage(attacker, targetGO, raw);
        }

        float consumed = (hits > 1) ? step * (hits - 1) : 0.0f;
        float remain = Mathf.Max(0.0f, act - consumed);
        if (remain > 0.0f) yield return new WaitForSeconds(remain);

    }
}
