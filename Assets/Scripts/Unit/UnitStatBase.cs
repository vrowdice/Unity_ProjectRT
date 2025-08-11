using UnityEngine;
using System.Collections.Generic;

// UnitType enum
public enum UnitType
{
    None,
    Melee, // 근거리
    Range, // 원거리
    Defense // 방어형
}

[CreateAssetMenu(fileName = "UnitStat", menuName = "Scriptable Object/UnitStat", order = 1)]
public class UnitStatBase : ScriptableObject
{
    [Header("프리팹")]
    public GameObject prefab;

    [Header("기본 정보")]
    public string unitName;
    public string unitDescription;
    public Sprite unitIllustration;
    public FactionType.TYPE factionType;
    public UnitType unitType;
    public bool isAttacker;

    [Header("스탯")]
    public float maxHealth;
    public float attackPower;
    public float defensePower;
    public float attackFrequency;
    public int attackCount;
    public float damageCoefficient;

    [Header("이동")]
    public float movementSpeed;

    [Header("마나")]
    public float manaStart;
    public float maxMana;
    public float manaRecoveryPerSecond;
    public float manaRecoveryOnBasicAttack;

    [Header("탐색")]
    public float enemySearchRange;

    [Header("연구")]
    public List<string> activatedResearchList = new List<string>();
}