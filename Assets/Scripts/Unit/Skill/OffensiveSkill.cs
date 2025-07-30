using System.Collections;
using UnityEngine;

// 공격형 스킬 코어

public class OffensiveSkill : BaseSkill
{

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target, float skillCooldown)
    {
        _isCasting = true;

        switch (caster.activeSkillType)
        {
            case "단일":
                // 단일 대상 공격 스킬
                if (target != null)
                {
                    float rawDamage = caster.attackPower * caster.activeSkillDamageCoefficient * caster.activeSkillAttackCount;
                    ApplySkillDamage(caster, target, rawDamage); 
                }
                else
                {
                    Debug.LogWarning($"여기 스킬 구현");
                }
                break;
        }

        caster.isSkillFinished = true; 
        yield return new WaitForSeconds(skillCooldown); 
        _isCasting = false; 
        caster.isSkillFinished = false; 
        Debug.Log($"{caster.unitName}의 '{caster.activeSkillName}' 스킬 쿨다운 종료.");
    }
}