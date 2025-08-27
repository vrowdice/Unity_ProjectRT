using System.Collections;
using System.Reflection;
using UnityEngine;

public class DefensiveSkill : BaseSkill, IConfigurableSkill
{
    [Header("기본값")]
    public float otherSkillRange = 0.0f;        // 반경
    public float otherSkillCoefficient = 0.0f;  
    public bool includeCaster = true;

    public bool freezeRigidbody2D = true;
    public bool freezeAnimatorRootMotion = true;
    public bool lockYByMover = true;

    public void ApplyConfigFromStat(UnitData stat)
    {
        if (stat == null || stat.active == null) return;
        if (stat.active.otherSkillRange > 0.0f) otherSkillRange = stat.active.otherSkillRange;
        if (stat.active.otherSkillCoefficient > 0.0f) otherSkillCoefficient = stat.active.otherSkillCoefficient;

        manaCost = stat.active.manaCost;
        skillName = string.IsNullOrEmpty(stat.active.displayName) ? stat.active.skillLogic : stat.active.displayName;
    }

    protected override IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target)
    {
        if (caster == null || otherSkillRange <= 0f || otherSkillCoefficient <= 0f) yield break;

        // (선택) 시전 중 움직임/루트모션 잠금
        Rigidbody2D rb2d = null; float oldG = 0; RigidbodyConstraints2D oldC = default; bool appliedRb = false;
        if (freezeRigidbody2D && (rb2d = caster.GetComponentInParent<Rigidbody2D>()) != null)
        { appliedRb = true; oldG = rb2d.gravityScale; oldC = rb2d.constraints; rb2d.gravityScale = 0; rb2d.constraints |= RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY; }

        Animator anim = null; bool appliedAnim = false; bool oldRM = false;
        if (freezeAnimatorRootMotion && (anim = caster.GetComponentInParent<Animator>()) != null)
        { appliedAnim = true; oldRM = anim.applyRootMotion; anim.applyRootMotion = false; }

        var mover = caster.GetComponent<UnitMovementController>();
        MethodInfo mi = null; bool lockedY = false;
        if (lockYByMover && mover != null && (mi = typeof(UnitMovementController).GetMethod("SetLockY")) != null)
        { mi.Invoke(mover, new object[] { true }); lockedY = true; mover.StopMove(); }

        var mask = TeamLayers.GetAllyMask(caster.Team);
        var cols = Physics2D.OverlapCircleAll(caster.transform.position, otherSkillRange, mask);
        float heal = caster.AttackPower * otherSkillCoefficient;

        foreach (var c in cols)
        {
            var ally = c.GetComponent<UnitBase>();
            if (ally == null || ally.IsDead) continue;
            if (!includeCaster && ally == caster) continue;

            ally.Heal(heal);
            UnitImpactEmitter.Emit(ally.gameObject, ImpactEventType.SkillCastHit, caster, ally.gameObject, heal, skillName);
        }

        if (appliedRb) { rb2d.gravityScale = oldG; rb2d.constraints = oldC; }
        if (appliedAnim) anim.applyRootMotion = oldRM;
        if (lockedY && mover != null && mi != null) mi.Invoke(mover, new object[] { false });
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (otherSkillRange > 0) { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, otherSkillRange); }
    }
#endif
}
