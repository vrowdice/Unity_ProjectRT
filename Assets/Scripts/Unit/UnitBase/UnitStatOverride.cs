using UnityEngine;

[DisallowMultipleComponent]
public class UnitStatOverride : MonoBehaviour
{
    [System.Serializable] public struct OptFloat { public bool use; public float value; public float Merge(float baseValue) => use ? value : baseValue; }

    [Header("�⺻ �ɷ�ġ")]
    public OptFloat attackPower;
    public OptFloat defensePower;
    public OptFloat maxHealth;
    public OptFloat moveSpeed;
    public OptFloat attackRange;
    public OptFloat enemySearchRange;

    [Header("���� Ÿ�̹�/����")]
    public OptFloat attackSpeed;  // APS
    [Range(-1, 1)] public float attackDelayRatioOverride = -1.0f; // <0 = �̻��
    public int attackCountOverride = -1; // -1=�̻��
    public OptFloat damageCoefficient;

    [Header("����")]
    public OptFloat baseMana;
    public OptFloat maxMana;
    public OptFloat manaRecoveryOnAttack;
    public OptFloat manaRecoveryPerSecond;
}
