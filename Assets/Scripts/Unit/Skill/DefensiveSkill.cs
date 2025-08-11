using System.Collections;
using UnityEngine;

// ����� ��ų �ھ�
public class DefensiveSkill : BaseSkill
{
    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        _isCasting = true;
        Debug.Log($"{caster.unitName}�� ��� ��ų '{skillName}'�� ����");

        float healAmount = caster.attackPower * otherSkillCoefficient;
        Collider2D[] allies = Physics2D.OverlapCircleAll(caster.transform.position, otherSkillRange);

        foreach (var ally in allies)
        {
            UnitBase allyUnit = ally.GetComponent<UnitBase>();

            if (allyUnit != null && allyUnit.factionType == caster.factionType)
            {
                float originalHP = allyUnit.currentHealth;
                allyUnit.currentHealth = Mathf.Min(allyUnit.currentHealth + healAmount, allyUnit.maxHealth);
                Debug.Log($"�� {allyUnit.unitName} ġ����: {originalHP:F1} �� {allyUnit.currentHealth:F1}");
            }
        }

        _isCasting = false;
        Debug.Log($"{caster.unitName}�� ��� ��ų ����");
        yield break;
    }
}