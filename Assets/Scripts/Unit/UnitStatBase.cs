using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Unit", menuName = "Unit")]

public class UnitStatBase : ScriptableObject
{
    public GameObject prefab;

    /////////////////////////////////////////////////
    [Header("�⺻ ���� �� ����")]
    public string unitName;
    public string unitType; // Ÿ��
    public string affiliation; // �Ҽ�

    // ���ݷ�
    public float attackPower;
    // ����
    public float defensePower;
    // �ִ� ü��
    public float maxHealth;
    // �̵� �ӵ�
    public float moveSpeed;
    // �ִ� ����
    public float maxMana;
    // �⺻ ���� �� ȸ���Ǵ� ������
    public float manaRecoveryOnBasicAttack;
    // �ʴ� ���� ȸ����
    public float manaRecoveryPerSecond;

    // ���� ü��
    private float _currentHealth;
    public float currentHealth
    {
        get { return _currentHealth; }
        set { _currentHealth = Mathf.Max(0, Mathf.Min(value, maxHealth)); }
    }

    private float _currentMana;
    public float currentMana
    {
        get { return _currentMana; }
        set { _currentMana = Mathf.Max(0, Mathf.Min(value, maxMana)); }
    }

    //��/�� ����
    public bool isAttack;

    /////////////////////////////////////////////////
    [Header("���� �� �Ϸ���Ʈ")]
    public List<string> activatedResearchList;
    public Image unitIllustration;

    /////////////////////////////////////////////////
    public enum BasicAttackName
    {
        longDistanceDefault,
        shortDistanceDefault,
        defenseDefault
    }

    public enum BasicAttackType
    {
        many,
        single
    }

    [Header("�⺻ ����")]
    //�̸�
    public BasicAttackName basicAttackName;
    //Ÿ��
    public BasicAttackType basicAttackType;
    //���� ���
    public float damageCoefficient;
    // �ֱ�
    public float attackFrequency;
    // Ÿ�� 
    public int attackCount;
    // �Ÿ�
    public float attackRange;

    // �ټ� �⺻ ���� �� Ÿ�� Ž�� ����
    public float multiTargetRange;
    // �ټ� �⺻ ���� �� �ִ� Ÿ�� ��
    public int maxMultiTargets;

    public GameObject areaAttackPrefab;
    // ���� ������ �ߵ� ���� 
    public Vector2 areaAttackDirection;
    // ���� ���� �������� ������ ��ġ
    public Vector2 areaAttackSpawnPosition;
    // ���� ���� ����Ʈ�� �¿� ���� ����
    public bool isAreaAttackFlipped;

    /////////////////////////////////////////////////

    [Header("��Ƽ�� ��ų ����")]
    public string activeSkillName;
    // ��Ƽ�� ��ų�� Ÿ��
    public string activeSkillType;
    // ��ų ��ٿ� �ð�
    public float activeSkillCooldown;
    // ��� ���� ����
    public bool isSkillFinished;

    // ��Ƽ�� ��ų ���� 
    public float activeSkillDamageCoefficient;
    // ��Ƽ�� ��ų�� Ÿ��
    public int activeSkillAttackCount;

    // �ټ� ���� ��Ƽ�� ��ų �� Ÿ�� Ž�� ����
    [Header("�ټ� ���� ��Ƽ�� ��ų ������")]
    public float activeMultiTargetRange;
    // �ټ� ���� ��Ƽ�� ��ų �� �ִ� Ÿ�� ��
    public int activeMaxMultiTargets;

    // ���� ���� ��Ƽ�� ��ų �� ����� ���� ����
    [Header("���� ���� ��Ƽ�� ��ų ������")]
    public GameObject activeAreaAttackPrefab;
    // ���� ���� ��Ƽ�� ��ų�� �ߵ� ����
    public Vector2 activeAreaAttackDirection;
    // ���� ���� ��Ƽ�� ��ų �������� ������ ��ġ
    public Vector2 activeAreaAttackSpawnPosition;
    // ���� ���� ��Ƽ�� ��ų ����Ʈ�� �¿� ���� ����
    public bool isActiveAreaAttackFlipped;

    // ��Ÿ ��Ƽ�� ��ų
    [Header("��Ÿ ��Ƽ�� ��ų ������")]
    public float otherSkillCoefficient;
    public float otherSkillRange;

    /////////////////////////////////////////////////
    [Header("�нú� ��ų ����")]

    // �нú� ��ų�� ȿ�� 
    public float passiveSkillCoefficient;
    // �нú� ��ų�� ���� ����
    public float passiveSkillRange;

    /////////////////////////////////////////////////
    [Header("���� ���� �� Ÿ����")]

    // ������ ���� �������� ��� �������� ���� (True: ����, False: ����)
    public bool isAttacker;
    // ���� ������ ���� ������ ���� (True: ���� ��)
    public bool isCombatInProgress;
    // Ÿ���� ������ ���� ���� �����ϴ��� ���� (True: ��� ����, False: ��� ����)
    public bool isTargetingAvailable;
    // ���� Ÿ���õ� �� ������Ʈ 
    public GameObject targetedEnemy;
    // Ÿ���õ� ���� ���ŵǾ����� ����
    public bool isTargetEliminated;

    // �� Ž�� ����
    public float enemySearchRange;
    // ���� Ÿ���õ� ������ �Ÿ�
    public float distanceToTargetedEnemy;

    /////////////////////////////////////////////////
}
