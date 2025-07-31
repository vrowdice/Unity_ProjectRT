using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ���� �̸� ������� ���� ���� / �ּ����� ���� �� �̸��� �����ֱ� �ٶ�...
// �ʿ���� �ǴܵǴ� �ڵ嵵 ���ϰ� �ּ� ó�� ���ֽñ�....
// ����� �߰��� �����Ӱ� ���ּ���~

// Battle Scene Unit�� �ھ�
public class UnitBase : MonoBehaviour
{
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
    [Header("�ܺ� Ŭ����")]

    public BaseAttack attackLogic;
    public BaseSkill skillLogic;
    public PassiveSkillHandler passiveLogic;
    public UnitMovementController movementLogic;

    protected virtual void Awake()
    {
        currentHealth = maxHealth; // ���� �� ���� ü���� �ִ� ü������ �ʱ�ȭ
        currentMana = maxMana;     // ���� �� ���� ������ �ִ� ������ �ʱ�ȭ
        isCombatInProgress = false; // �ʱ⿡�� ���� ���� �ƴ�
        isTargetingAvailable = false; // �ʱ⿡�� Ÿ���� ����� ����
        isTargetEliminated = false; // �ʱ⿡�� ����� ���ŵ��� ����

    }

    protected virtual void Start()
    {
        if (passiveLogic != null)
        {
            passiveLogic.ApplyEffect(this);
        }
    }
    protected virtual void Update()
    {
        currentMana += manaRecoveryPerSecond * Time.deltaTime;
    }

    public virtual void SetTarget(GameObject target)
    {
        targetedEnemy = target;
        isTargetingAvailable = (target != null); // Ÿ���� ������ true, ������ false
        isTargetEliminated = false;

        if (target != null)
        {
            distanceToTargetedEnemy = Vector2.Distance(transform.position, targetedEnemy.transform.position);
        }
        else
        {
            distanceToTargetedEnemy = 0.0f; // Ÿ���� ������ �Ÿ��� 0���� ����
        }
    }

    // �̵�
    public virtual void MoveTo(Vector3 destination)
    {
        if (movementLogic != null)
        {
            movementLogic.StartMove(destination, moveSpeed); 
        }
        else
        {
            Debug.LogWarning($"{unitName}�� �̵� ����(UnitMovementController)�� �Ҵ���� �ʾ� �̵� �Ұ�. Ȯ�� �ʿ�.");
        }
    }

    // �⺻ ����
    public virtual void PerformBasicAttack()
    {
        if (attackLogic != null)
        {
            // Ÿ���� ���ų� Ÿ���� �Ұ��� �����̸� ���� �Ұ�
            if (targetedEnemy == null || !isTargetingAvailable)
            {
                Debug.LogWarning($"{unitName}�� Ÿ���� ���� �⺻ ������ ���� �Ұ�.");
                return;
            }
            // Ÿ���� ���� ��Ÿ� �ۿ� ������ ���� �Ұ�
            if (distanceToTargetedEnemy > attackRange)
            {
                return;
            }

            // ���� ���� ������Ʈ���� ���� ������ �����մϴ�.
            attackLogic.StartAttack(this, targetedEnemy);
        }
        else
        {
            Debug.LogWarning($"{unitName}�� ���� ����(BaseAttack)�� �Ҵ���� �ʾ� ���� �Ұ�.");
        }
    }

    // ����
    public virtual void TakeDamage(float rawDamage)
    {
        float finalDamage = DamageCalculator.CalculateDamage(rawDamage, defensePower);
        currentHealth -= finalDamage; // ���� ���ط���ŭ ü�� ����

        // ü���� 0 ���ϰ� �Ǹ� ���� ��� ó��
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ��Ƽ�� ��ų�� ��� ���
    public virtual bool UseActiveSkill()
    {
        if (skillLogic != null)
        {
            skillLogic.StartCast(this, targetedEnemy, activeSkillCooldown); // ��ٿ� �ð� ����
            return true;
        }
        else
        {
            return false;
        }
    }

    // �нú� ��ų ȿ���� ����
    public virtual void ApplyPassiveSkills()
    {
        if (passiveLogic != null)
        {
            passiveLogic.ApplyEffect(this);
            Debug.Log($"{unitName}��(��) �нú� ��ų ȿ���� ����.");
        }
        else
        {
            Debug.LogWarning($"{unitName}�� �нú� ����(PassiveSkillHandler)�� �Ҵ���� ����. Ȯ�� �ʿ�.");
        }
    }

    // ���� ó��
    protected virtual void Die()
    {
        Debug.Log($"{unitName}��(��) ����");
        isCombatInProgress = false; // ���� �ߴ� ���·� ����

        attackLogic?.StopAttack();
        skillLogic?.StopCast();
        movementLogic?.StopMoveRoutine();

        Destroy(gameObject); 
    }

    // ���� ���¸� ���� or �ߴ�
    public void SetCombatStatus(bool inCombat)
    {
        isCombatInProgress = inCombat;
        if (!inCombat)
        {
            Debug.Log($"{unitName}��(��) ������ �ߴ�");
            attackLogic?.StopAttack();
            skillLogic?.StopCast();
            movementLogic?.StopMoveRoutine();
        }
        else
        {
            Debug.Log($"{unitName}��(��) ������ ����");
        }
    }

    /// <summary>
    /// ���� �����ͷ� ���� ���� �ʱ�ȭ
    /// </summary>
    public virtual void Initialize(UnitStatBase stat)
    {
        // �⺻ ����
        unitName = stat.unitName;
        unitType = stat.unitType;
        affiliation = stat.affiliation;

        attackPower = stat.attackPower;
        defensePower = stat.defensePower;
        maxHealth = stat.maxHealth;
        moveSpeed = stat.moveSpeed;
        maxMana = stat.maxMana;
        manaRecoveryOnBasicAttack = stat.manaRecoveryOnBasicAttack;
        manaRecoveryPerSecond = stat.manaRecoveryPerSecond;

        currentHealth = stat.maxHealth;
        currentMana = 0f;

        isAttack = stat.isAttack;

        // �Ϸ���Ʈ �� ���� ����
        activatedResearchList = new List<string>(stat.activatedResearchList);
        unitIllustration = stat.unitIllustration;

        // �⺻ ����
        basicAttackName = (BasicAttackName)stat.basicAttackName;
        basicAttackType = (BasicAttackType)stat.basicAttackType;
        damageCoefficient = stat.damageCoefficient;
        attackFrequency = stat.attackFrequency;
        attackCount = stat.attackCount;
        attackRange = stat.attackRange;
        multiTargetRange = stat.multiTargetRange;
        maxMultiTargets = stat.maxMultiTargets;
        areaAttackPrefab = stat.areaAttackPrefab;
        areaAttackDirection = stat.areaAttackDirection;
        areaAttackSpawnPosition = stat.areaAttackSpawnPosition;
        isAreaAttackFlipped = stat.isAreaAttackFlipped;

        // ��Ƽ�� ��ų
        activeSkillName = stat.activeSkillName;
        activeSkillType = stat.activeSkillType;
        activeSkillCooldown = stat.activeSkillCooldown;
        isSkillFinished = stat.isSkillFinished;
        activeSkillDamageCoefficient = stat.activeSkillDamageCoefficient;
        activeSkillAttackCount = stat.activeSkillAttackCount;
        activeMultiTargetRange = stat.activeMultiTargetRange;
        activeMaxMultiTargets = stat.activeMaxMultiTargets;
        activeAreaAttackPrefab = stat.activeAreaAttackPrefab;
        activeAreaAttackDirection = stat.activeAreaAttackDirection;
        activeAreaAttackSpawnPosition = stat.activeAreaAttackSpawnPosition;
        isActiveAreaAttackFlipped = stat.isActiveAreaAttackFlipped;
        otherSkillCoefficient = stat.otherSkillCoefficient;
        otherSkillRange = stat.otherSkillRange;

        // �нú� ��ų
        passiveSkillCoefficient = stat.passiveSkillCoefficient;
        passiveSkillRange = stat.passiveSkillRange;

        // ���� �� Ÿ���� ����
        isAttacker = stat.isAttacker;
        isCombatInProgress = stat.isCombatInProgress;
        isTargetingAvailable = stat.isTargetingAvailable;
        targetedEnemy = stat.targetedEnemy;
        isTargetEliminated = stat.isTargetEliminated;
        enemySearchRange = stat.enemySearchRange;
        distanceToTargetedEnemy = stat.distanceToTargetedEnemy;
    }
}