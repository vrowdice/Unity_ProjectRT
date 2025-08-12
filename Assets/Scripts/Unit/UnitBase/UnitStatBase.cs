using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStat", menuName = "Unit/UnitStatBase")]
public class UnitStatBase : ScriptableObject
{

    public string unitName;
    public Sprite unitIcon;
    public FactionType.TYPE factionType;
    public UnitTagType unitTagType;

    [Header("Base Stats")]
    public float attackPower;
    public float defensePower;
    public float maxHealth;
    public float moveSpeed;
    public float attackRange;
    public float attackSpeed;

    [Header("����")]
    public float baseMana;
    public float maxMana;
    public float manaRecoveryOnAttack;
    public float manaRecoveryPerSecond;

    [Header("����")]
    public int attackCount;
    public float damageCoefficient;
    public float enemySearchRange;

    [Header("Assets")]
    public GameObject prefab;
    public Sprite unitIllustration; // UI�� ǥ�õ� ���� �̹���
}
