using System.Collections;
using UnityEngine;

public interface IConfigurableSkill
{
    void ApplyConfigFromStat(UnitStatBase stat);
}

public abstract class BaseSkill : MonoBehaviour
{
    [Header("°øÅë")]
    public string skillName;
    [Min(0)] public float manaCost = 0f;

    protected Coroutine _co;
    public bool IsCasting { get; protected set; }

    public void StartCast(UnitBase caster, GameObject target)
    {
        if (IsCasting || caster == null) return;

        if (manaCost > 0.0f && !caster.UseMana(manaCost)) return;

        IsCasting = true;
        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastStart, caster, target, 0, skillName);
        _co = StartCoroutine(_Run(caster, target));
    }

    public void StopCast()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
        IsCasting = false;
    }

    private IEnumerator _Run(UnitBase caster, GameObject target)
    {
        yield return PerformSkillRoutine(caster, target);

        UnitImpactEmitter.Emit(caster.gameObject, ImpactEventType.SkillCastFinish, caster, target, 0, skillName);
        IsCasting = false;
    }

    protected abstract IEnumerator PerformSkillRoutine(UnitBase caster, GameObject target);
}
