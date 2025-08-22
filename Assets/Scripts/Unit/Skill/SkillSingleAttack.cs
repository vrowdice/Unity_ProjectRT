using System.Collections;
using UnityEngine;

public class SkillSingleAttack : BaseSkill
{
    [Header("��ų ��ġ")]
    [Min(0)] public float skillDamageCoefficient;
    [Min(0)] public int skillAttackCount;       // 0�̸� 1�� ����
    [Min(0)] public float hitInterval;            // 0�̸� �����Ӹ��� ��Ÿ

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        if (caster == null || target == null || !target.activeSelf)
            yield break;

        Debug.Log($"{caster.UnitName}�� ���� ���� ��ų '{skillName}'�� ����");

        int hits = Mathf.Max(1, skillAttackCount);
        float damage = caster.AttackPower * skillDamageCoefficient;
        float step = Mathf.Max(0f, hitInterval);

        for (int i = 0; i < hits; i++)
        {
            if (!IsValidTarget(target)) break;

            ApplySkillDamage(caster, target, damage);

            if (step > 0f) yield return new WaitForSeconds(step);
            else yield return null; // 0�̸� ������ ��
        }

        Debug.Log("��ų ��� ����");
        yield break;
    }

    // ��ƿ
    private bool IsValidTarget(GameObject t)
    {
        if (t == null || !t.activeSelf) return false;
        var ub = t.GetComponent<UnitBase>();
        return ub != null && !ub.IsDead;
    }

    /// ��ų ������ ���� + ����Ʈ �߻�
    private void ApplySkillDamage(UnitBase caster, GameObject target, float damage)
    {
        var victim = target.GetComponent<UnitBase>();
        if (victim == null || victim.IsDead) return;

        victim.TakeDamage(damage);

        // ����Ʈ/�α�
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit, caster, target, damage, skillName);
    }
}
