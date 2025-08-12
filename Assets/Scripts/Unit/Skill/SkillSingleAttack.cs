using System.Collections;
using UnityEngine;

// 단일 공격 스킬
public class SkillSingleAttack : BaseSkill
{
    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        _isCasting = true;
        // 수정된 부분: UnitName 프로퍼티 사용
        Debug.Log($"{caster.UnitName}가 단일 공격 스킬 '{skillName}'을 시전");

        // 수정된 부분: AttackPower 프로퍼티 사용
        float damage = caster.AttackPower * skillDamageCoefficient;

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