using System.Collections;
using UnityEngine;

// ����� ��ų �ھ�
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
                Debug.Log($"�� {allyUnit.unitName} ġ����: {originalHP:F1} �� {allyUnit.currentHealth:F1}");
            }
        }

        caster.isSkillFinished = true; 
        yield return new WaitForSeconds(skillCooldown); 
        caster.isSkillFinished = false; 
        _isCasting = false; 
        Debug.Log($"{caster.unitName}�� ��� ��ų ��ٿ� ����");
    }
}