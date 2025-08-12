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

    // 방어 스킬 관련 변수
    public float otherSkillCoefficient;
    public float otherSkillRange;

    protected bool _isCasting = false;
    public bool IsCasting => _isCasting;

    public void StartCast(UnitBase caster, GameObject target)
    {
        if (IsCasting)
        {
            Debug.LogWarning($"{caster.UnitName}의 스킬 '{skillName}'이 이미 시전 중입니다.");
            return;
        }

        if (!caster.UseMana(manaCost))
        {
            Debug.Log($"{caster.UnitName}은(는) '{skillName}' 스킬을 사용할 마나가 부족합니다.");
            return;
        }

        // 이펙트 시작
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastStart, caster, target, 0.0f, skillName);

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
            Debug.Log($"→ {targetUnit.UnitName}에게 스킬로 {rawDamage:F2} 피해를 입힘. (시전자: {caster.UnitName})");

            // 이펙트
            UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastHit, caster, target, rawDamage, skillName);
            UnitImpactEmitter.Emit(target, ImpactEventType.Damaged, caster, target, rawDamage, skillName);
        }
    }
}
