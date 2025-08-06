using UnityEngine;
using System.Collections.Generic;

// UnitType enum
public enum UnitType
{
    None,
    Melee, // �ٰŸ�
    Range, // ���Ÿ�
    Defense // �����
}

[CreateAssetMenu(fileName = "UnitStat", menuName = "Scriptable Object/UnitStat", order = 1)]
public class UnitStatBase : ScriptableObject
{
    [Header("������")]
    public GameObject prefab;

    [Header("�⺻ ����")]
    public string unitName;
    public string unitDescription;
    public Sprite unitIllustration;
    public FactionType.TYPE factionType;
    public UnitType unitType;
    public bool isAttacker;

    [Header("����")]
    public float maxHealth;
    public float attackPower;
    public float defensePower;
    public float attackFrequency;
    public int attackCount;
    public float damageCoefficient;

    [Header("�̵�")]
    public float movementSpeed;

    [Header("����")]
    public float manaStart;
    public float maxMana;
    public float manaRecoveryPerSecond;
    public float manaRecoveryOnBasicAttack;

    [Header("Ž��")]
    public float enemySearchRange;

    [Header("����")]
    public List<string> activatedResearchList = new List<string>();
}