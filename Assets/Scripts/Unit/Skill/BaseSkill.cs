using System.Collections;
using UnityEngine;

// 액티브 스킬
public abstract class BaseSkill : MonoBehaviour
{
    [Header("스킬 데이터")]
    public string skillName;
    public float manaCost;
    public float skillDamageCoefficient;
    public int skillAttackCount;

    // [추가] 방어 스킬 관련 변수
    public float otherSkillCoefficient;
    public float otherSkillRange;

    protected bool _isCasting = false;
    public bool IsCasting => _isCasting;

    public void StartCast(UnitBase caster, GameObject target)
    {
        if (IsCasting)
        {
            Debug.LogWarning($"{caster.unitName}의 스킬 '{skillName}'이 이미 시전 중입니다.");
            return;
        }

        if (caster.currentMana < manaCost)
        {
            Debug.Log($"{caster.unitName}은(는) '{skillName}' 스킬을 사용할 마나가 부족합니다.");
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