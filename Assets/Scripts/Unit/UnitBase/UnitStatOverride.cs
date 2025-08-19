using UnityEngine;

[DisallowMultipleComponent]
public class UnitStatOverride : MonoBehaviour
{
    [System.Serializable] public struct OptFloat { public bool use; public float value; public float Merge(float baseValue) => use ? value : baseValue; }

    [Header("기본 능력치")]
    public OptFloat attackPower;
    public OptFloat defensePower;
    public OptFloat maxHealth;
    public OptFloat moveSpeed;
    public OptFloat attackRange;
    public OptFloat enemySearchRange;

    [Header("공격 타이밍/세부")]
    public OptFloat attackSpeed;  // APS
    [Range(-1, 1)] public float attackDelayRatioOverride = -1.0f; // <0 = 미사용
    public int attackCountOverride = -1; // -1=미사용
    public OptFloat damageCoefficient;

    [Header("마나")]
    public OptFloat baseMana;
    public OptFloat maxMana;
    public OptFloat manaRecoveryOnAttack;
    public OptFloat manaRecoveryPerSecond;
}
