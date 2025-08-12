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
    public UnitBase source;   // ����
    public GameObject target; // ������
    public float value;       // ���ط�/����
}
