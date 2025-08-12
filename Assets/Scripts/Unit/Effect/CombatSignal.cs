using UnityEngine;

public enum CombatSignalType
{
    BasicAttackStart,
    BasicAttackHit,
    SkillStart,
    SkillFinish,
    Damaged,
    Died
}

public struct CombatSignal
{
    public CombatSignalType type;
    public UnitBase source;   // 누가
    public GameObject target; // 누구를
    public float value;       // 피해량/힐량
}
