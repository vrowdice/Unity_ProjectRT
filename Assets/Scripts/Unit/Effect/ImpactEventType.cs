using UnityEngine;

public enum ImpactEventType
{
    BasicAttackStart,
    BasicAttackHit,
    SkillCastStart,
    SkillCastHit,
    SkillCastFinish,
    CustomEvent,
    Damaged,
    Died
}

public interface IImpactReceiver
{
    void OnImpact(ImpactEventType type, UnitBase source, GameObject target, float value, string extra);
}
