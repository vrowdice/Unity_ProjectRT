using System.Collections;
using UnityEngine;

public class OffensiveSkill : BaseSkill, IConfigurableSkill
{
    [Header("기본값(테이블이 오면 덮어씀)")]
    [Min(0)] public float skillDamageCoefficient = 1.0f;
    [Min(0)] public int skillAttackCount = 1;
    [Min(0)] public float hitInterval = 0.1f;

    public void ApplyConfigFromStat(UnitData stat)
    {
        if (stat == null || stat.active == null) return;
        if (stat.active.damageCoeff > 0f) skillDamageCoefficient = stat.active.damageCoeff;
        if (stat.active.attackCount > 0) skillAttackCount = stat.active.attackCount;

        manaCost = stat.active.manaCost;
        skillName = string.IsNullOrEmpty(stat.active.displayName) ? stat.active.skillLogic : stat.active.displayName;
    }

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        if (caster == null) yield break;

        int hits = Mathf.Max(1, skillAttackCount);
        float dmg = caster.AttackPower * skillDamageCoefficient;

        for (int i = 0; i < hits; i++)
        {
            if (!ValidTarget(caster, target)) break;

            var victim = target.GetComponent<UnitBase>();
            victim.TakeDamage(dmg);
            UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit, caster, target, dmg, skillName);

            if (hitInterval > 0.0f) yield return new WaitForSeconds(hitInterval);
            else yield return null;

            if (caster == null) yield break;
            if (!ValidTarget(caster, target)) break;
        }
    }

    private bool ValidTarget(UnitBase caster, GameObject t)
    {
        if (t == null || !t.activeSelf) return false;
        var ub = t.GetComponent<UnitBase>();
        if (ub == null || ub.IsDead) return false;
        if (ub.Team == caster.Team) return false;
        return true;
    }
}
