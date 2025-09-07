using System.Collections;
using UnityEngine;

public class SkillSingleAttack : BaseSkill
{
    [Header("스킬 수치")]
    [Min(0)] public float skillDamageCoefficient;
    [Min(0)] public int skillAttackCount;       // 0이면 1로 보정
    [Min(0)] public float hitInterval;            // 0이면 프레임마다 연타

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        if (caster == null || target == null || !target.activeSelf)
            yield break;

        Debug.Log($"{caster.UnitName}가 단일 공격 스킬 '{skillName}'을 시전");

        int hits = Mathf.Max(1, skillAttackCount);
        float damage = caster.AttackPower * skillDamageCoefficient;
        float step = Mathf.Max(0f, hitInterval);

        for (int i = 0; i < hits; i++)
        {
            if (!IsValidTarget(target)) break;

            ApplySkillDamage(caster, target, damage);

            if (step > 0f) yield return new WaitForSeconds(step);
            else yield return null; // 0이면 프레임 텀
        }

        Debug.Log("스킬 사용 종료");
        yield break;
    }

    // 유틸
    private bool IsValidTarget(GameObject t)
    {
        if (t == null || !t.activeSelf) return false;
        var ub = t.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    /// 스킬 데미지 적용 + 이펙트 발생
    private void ApplySkillDamage(UnitBase caster, GameObject target, float damage)
    {
        var victim = target.GetComponent<UnitBase>();
        if (victim == null || victim.IsDead) return;

        victim.TakeDamage(damage);

        // 이펙트/로그
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit, caster, target, damage, skillName);
    }
}
