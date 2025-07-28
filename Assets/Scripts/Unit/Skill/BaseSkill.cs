using System.Collections;
using UnityEngine;

// ��Ƽ�� ��ų 
public abstract class BaseSkill : MonoBehaviour
{
    protected bool _isCasting = false;
    public bool IsCasting => _isCasting;

    public float manaCost;

    public void StartCast(UnitBase caster, GameObject target, float skillCooldown)
    {
        if (_isCasting)
        {
            Debug.LogWarning($"{caster.unitName}�� ��ų ������ �̹� ���� ��");
            return;
        }

        if (caster.currentMana < manaCost)
        {
            Debug.Log($"{caster.unitName}��(��) '{caster.activeSkillName}' ��ų�� ����� ������ ����");
            return;
        }
        caster.currentMana -= manaCost;

        StopAllCoroutines();
        StartCoroutine(PerformSkillRoutine(caster, target, skillCooldown));
    }

    protected abstract IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target, float skillCooldown);
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