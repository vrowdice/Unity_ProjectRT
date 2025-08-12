using System.Collections;
using UnityEngine;

// ��Ƽ�� ��ų
public abstract class BaseSkill : MonoBehaviour
{
    [Header("��ų ������")]
    public string skillName;
    public float manaCost;
    public float skillDamageCoefficient;
    public int skillAttackCount;

    // ��� ��ų ���� ����
    public float otherSkillCoefficient;
    public float otherSkillRange;

    protected bool _isCasting = false;
    public bool IsCasting => _isCasting;

    public void StartCast(UnitBase caster, GameObject target)
    {
        if (IsCasting)
        {
            Debug.LogWarning($"{caster.UnitName}�� ��ų '{skillName}'�� �̹� ���� ���Դϴ�.");
            return;
        }

        if (!caster.UseMana(manaCost))
        {
            Debug.Log($"{caster.UnitName}��(��) '{skillName}' ��ų�� ����� ������ �����մϴ�.");
            return;
        }

        // ����Ʈ ����
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastStart, caster, target, 0.0f, skillName);

        StopAllCoroutines();
        StartCoroutine(PerformSkillRoutine(caster, target));
    }

    protected abstract IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target);

    public void StopCast()
    {
        StopAllCoroutines();
        _isCasting = false;
        Debug.Log("��ų ��ƾ ���� ����.");
    }

    protected virtual void ApplySkillDamage(UnitBase caster, GameObject target, float rawDamage)
    {
        if (target == null) return;

        UnitBase targetUnit = target.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(rawDamage);
            Debug.Log($"�� {targetUnit.UnitName}���� ��ų�� {rawDamage:F2} ���ظ� ����. (������: {caster.UnitName})");

            // ����Ʈ
            UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit, caster, target, rawDamage, skillName);
            UnitImpactEmitter.Emit(target, ImpactEventType.Damaged, caster, target, rawDamage, skillName);
        }
    }
}
