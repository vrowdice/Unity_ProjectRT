using System.Collections;
using UnityEngine;

// 방어형 스킬 코어
public class DefensiveSkill : BaseSkill
{
    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        _isCasting = true;
        Debug.Log($"{caster.unitName}가 방어 스킬 '{skillName}'을 시전");

        float healAmount = caster.attackPower * otherSkillCoefficient;
        Collider2D[] allies = Physics2D.OverlapCircleAll(caster.transform.position, otherSkillRange);

        foreach (var ally in allies)
        {
            UnitBase allyUnit = ally.GetComponent<UnitBase>();

            if (allyUnit != null && allyUnit.factionType == caster.factionType)
            {
                float originalHP = allyUnit.currentHealth;
                allyUnit.currentHealth = Mathf.Min(allyUnit.currentHealth + healAmount, allyUnit.maxHealth);
                Debug.Log($"→ {allyUnit.unitName} 치유됨: {originalHP:F1} → {allyUnit.currentHealth:F1}");
            }
        }

        _isCasting = false;
        Debug.Log($"{caster.unitName}의 방어 스킬 종료");
        yield break;
    }
}