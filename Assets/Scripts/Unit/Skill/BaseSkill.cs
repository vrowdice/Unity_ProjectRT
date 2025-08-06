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

    // [�߰�] ��� ��ų ���� ����
    public float otherSkillCoefficient;
    public float otherSkillRange;

    protected bool _isCasting = false;
    public bool IsCasting => _isCasting;

    public void StartCast(UnitBase caster, GameObject target)
    {
        if (IsCasting)
        {
            Debug.LogWarning($"{caster.unitName}�� ��ų '{skillName}'�� �̹� ���� ���Դϴ�.");
            return;
        }

        if (caster.currentMana < manaCost)
        {
            Debug.Log($"{caster.unitName}��(��) '{skillName}' ��ų�� ����� ������ �����մϴ�.");
            return;
        }

        caster.currentMana -= manaCost;

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
            Debug.Log($"�� {targetUnit.unitName}���� ��ų�� {rawDamage:F2} ���ظ� ����. (������: {caster.unitName})");
        }
    }
}