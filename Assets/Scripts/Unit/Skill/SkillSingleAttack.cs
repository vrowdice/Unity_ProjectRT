using System.Collections;
using UnityEngine;

// ���� ���� ��ų
public class SkillSingleAttack : BaseSkill
{
    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        _isCasting = true;
        // ������ �κ�: UnitName ������Ƽ ���
        Debug.Log($"{caster.UnitName}�� ���� ���� ��ų '{skillName}'�� ����");

        // ������ �κ�: AttackPower ������Ƽ ���
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
        Debug.Log("��ų ��� ����");
    }
}