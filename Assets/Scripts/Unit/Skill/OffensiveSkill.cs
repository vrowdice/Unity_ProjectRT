using System.Collections;
using UnityEngine;

// ������ ��ų �ھ�

public class OffensiveSkill : BaseSkill
{

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target, float skillCooldown)
    {
        _isCasting = true;

        switch (caster.activeSkillType)
        {
            case "����":
                // ���� ��� ���� ��ų
                if (target != null)
                {
                    float rawDamage = caster.attackPower * caster.activeSkillDamageCoefficient * caster.activeSkillAttackCount;
                    ApplySkillDamage(caster, target, rawDamage); 
                }
                else
                {
                    Debug.LogWarning($"���� ��ų ����");
                }
                break;
        }

        caster.isSkillFinished = true; 
        yield return new WaitForSeconds(skillCooldown); 
        _isCasting = false; 
        caster.isSkillFinished = false; 
        Debug.Log($"{caster.unitName}�� '{caster.activeSkillName}' ��ų ��ٿ� ����.");
    }
}