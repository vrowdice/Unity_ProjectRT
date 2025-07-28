using System.Collections;
using UnityEngine;

// 액티브 스킬 
public abstract class BaseSkill : MonoBehaviour
{
    protected bool _isCasting = false;
    public bool IsCasting => _isCasting;

    public float manaCost;

    public void StartCast(UnitBase caster, GameObject target, float skillCooldown)
    {
        if (_isCasting)
        {
            Debug.LogWarning($"{caster.unitName}의 스킬 시전이 이미 진행 중");
            return;
        }

        if (caster.currentMana < manaCost)
        {
            Debug.Log($"{caster.unitName}은(는) '{caster.activeSkillName}' 스킬을 사용할 마나가 부족");
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
        Debug.Log("스킬 루틴 강제 중지.");
    }

    protected virtual void ApplySkillDamage(UnitBase caster, GameObject target, float rawDamage)
    {
        if (target == null) return;

        UnitBase targetUnit = target.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(rawDamage);
            Debug.Log($"→ {targetUnit.unitName}에게 스킬로 {rawDamage:F2} 피해를 입힘. (시전자: {caster.unitName})");
        }
    }
}