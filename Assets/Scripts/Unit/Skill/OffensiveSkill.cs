using System.Collections;
using UnityEngine;

// ������ ��ų �ھ� -> ���� ���� ��ų�� ���� ����
public class OffensiveSkill : BaseSkill
{
    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        _isCasting = true;
        Debug.Log($"{caster.unitName}�� ���� ���� ��ų '{skillName}'�� ����");

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
        Debug.Log("��ų ��� ����");
    }
}