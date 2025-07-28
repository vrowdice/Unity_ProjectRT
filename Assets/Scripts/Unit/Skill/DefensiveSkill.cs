using System.Collections;
using UnityEngine;

// 방어형 스킬 코어
public class DefensiveSkill : BaseSkill
{

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target, float skillCooldown)
    {
        _isCasting = true; 

        float healAmount = caster.attackPower * caster.otherSkillCoefficient;
        Collider2D[] allies = Physics2D.OverlapCircleAll(caster.transform.position, caster.otherSkillRange);

        foreach (var ally in allies)
        {
            UnitBase allyUnit = ally.GetComponent<UnitBase>();
            if (allyUnit != null && allyUnit.affiliation == caster.affiliation)
            {
                float originalHP = allyUnit.currentHealth;
                allyUnit.currentHealth = Mathf.Min(allyUnit.currentHealth + healAmount, allyUnit.maxHealth);
                Debug.Log($"→ {allyUnit.unitName} 치유됨: {originalHP:F1} → {allyUnit.currentHealth:F1}");
            }
        }

        caster.isSkillFinished = true; 
        yield return new WaitForSeconds(skillCooldown); 
        caster.isSkillFinished = false; 
        _isCasting = false; 
        Debug.Log($"{caster.unitName}의 방어 스킬 쿨다운 종료");
    }
}