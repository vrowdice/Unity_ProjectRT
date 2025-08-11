using System.Collections;
using UnityEngine;

// 공격형 스킬 코어 -> 단일 공격 스킬로 역할 변경
public class OffensiveSkill : BaseSkill
{
    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        _isCasting = true;
        Debug.Log($"{caster.unitName}가 단일 공격 스킬 '{skillName}'을 시전");

        float damage = caster.attackPower * skillDamageCoefficient;

        for (int i = 0; i < skillAttackCount; i++)
        {
            if (target == null || !target.activeSelf)
            {
                StopCast();
                yield break;
            }

            ApplySkillDamage(caster, target, damage);

            yield return new WaitForSeconds(0.1f);
        }

        _isCasting = false;
        Debug.Log("스킬 사용 종료");
    }
}