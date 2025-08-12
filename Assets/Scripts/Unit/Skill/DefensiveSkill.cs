using System.Collections;
using UnityEngine;

// 방어형 스킬 코어
public class DefensiveSkill : BaseSkill
{
    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        _isCasting = true;
        Debug.Log($"{caster.UnitName}가 방어 스킬 '{skillName}'을 시전");

        var rb2d = caster.GetComponentInParent<Rigidbody2D>();
        float oldGravity2D = 0.0f;
        RigidbodyConstraints2D oldCons2D = default;
        bool hadRb2D = false;
        if (rb2d)
        {
            hadRb2D = true;
            oldGravity2D = rb2d.gravityScale;
            oldCons2D = rb2d.constraints;
            rb2d.gravityScale = 0f;
            rb2d.constraints |= RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        }

        var anim = caster.GetComponentInParent<Animator>();
        bool hadAnimator = false;
        bool oldRootMotion = false;
        if (anim)
        {
            hadAnimator = true;
            oldRootMotion = anim.applyRootMotion;
            anim.applyRootMotion = false;
        }

        var mover = caster.GetComponent<UnitMovementController>();
        bool lockedYBefore = false;
        if (mover != null)
        {
            var mi = typeof(UnitMovementController).GetMethod("SetLockY");
            if (mi != null)
            {
                lockedYBefore = true;
                mi.Invoke(mover, new object[] { true });
            }
        }

        float healAmount = caster.AttackPower * otherSkillCoefficient;
        Collider2D[] allies = Physics2D.OverlapCircleAll(caster.transform.position, otherSkillRange);
        foreach (var ally in allies)
        {
            UnitBase allyUnit = ally.GetComponent<UnitBase>();
            if (allyUnit != null && allyUnit.Faction == caster.Faction)
            {
                float before = allyUnit.CurrentHealth;
                allyUnit.Heal(healAmount);
                Debug.Log($"→ {allyUnit.UnitName} 치유: {before:F1} → {allyUnit.CurrentHealth:F1}");

                UnitImpactEmitter.Emit(allyUnit.gameObject, ImpactEventType.SkillCastHit, caster, allyUnit.gameObject, healAmount, skillName);
            }
        }

        _isCasting = false;
        Debug.Log($"{caster.UnitName}의 방어 스킬 종료");

        // 이펙트 종료
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastFinish, caster, target, 0.0f, skillName);

        if (hadRb2D)
        {
            rb2d.gravityScale = oldGravity2D;
            rb2d.constraints = oldCons2D;
        }
        if (hadAnimator)
        {
            anim.applyRootMotion = oldRootMotion;
        }
        if (lockedYBefore && mover != null)
        {
            var mi = typeof(UnitMovementController).GetMethod("SetLockY");
            if (mi != null) mi.Invoke(mover, new object[] { false });
        }

        yield break;
    }
}
