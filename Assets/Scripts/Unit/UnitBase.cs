using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Battle Scene Unit�� ������ ���� �ھ�
public class UnitBase : MonoBehaviour
{
    /////////////////////////////////////////////////
    [Header("�⺻ ���� �� ����")]
    public UnitStatBase stat;

    // �⺻ ����
    public string unitName;
    public UnitType unitType;
    public FactionType.TYPE factionType;

    public float attackPower;
    public float defensePower;
    public float maxHealth;
    public float movementSpeed;
    public float maxMana;
    public float manaRecoveryOnBasicAttack;
    public float manaRecoveryPerSecond;
    public float attackFrequency;
    public int attackCount;
    public float damageCoefficient;

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
    public bool isAttacker;

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
        MultiAttack,
        Single
    }

    /////////////////////////////////////////////////
    [Header("���� ���� �� Ÿ����")]
    public bool isCombatInProgress;
    public bool isTargetingAvailable;
    public GameObject targetedEnemy;
    public bool isTargetEliminated;
    public float enemySearchRange;
    public float distanceToTargetedEnemy;

    /////////////////////////////////////////////////

    [Header("���� ��ǥ ����")]
    private Vector3 forwardDestination;
    private Vector3 startPosition;

    /////////////////////////////////////////////////
    [Header("�ܺ� Ŭ����")]
    public BaseAttack attackLogic;
    public BaseSkill skillLogic;
    public PassiveSkillHandler passiveLogic;
    public UnitMovementController movementLogic;

    private float attackTimer = 0f;

    protected virtual void Awake()
    {
        if (stat != null)
        {
            Initialize(stat);
        }
        else
        {
            Debug.LogError($"Error: UnitStatBase ������ {gameObject.name}�� UnitBase ��ũ��Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        currentHealth = maxHealth;
        currentMana = stat.manaStart;
        isCombatInProgress = false;
        isTargetingAvailable = false;
        isTargetEliminated = false;
    }

    public virtual void Initialize(UnitStatBase statData)
    {
        unitName = statData.unitName;
        unitType = statData.unitType;
        factionType = statData.factionType;

        attackPower = statData.attackPower;
        defensePower = statData.defensePower;
        maxHealth = statData.maxHealth;
        movementSpeed = statData.movementSpeed;
        maxMana = statData.maxMana;
        manaRecoveryOnBasicAttack = statData.manaRecoveryOnBasicAttack;
        manaRecoveryPerSecond = statData.manaRecoveryPerSecond;
        isAttacker = statData.isAttacker;
        enemySearchRange = statData.enemySearchRange;

        attackFrequency = statData.attackFrequency;
        attackCount = statData.attackCount;
        damageCoefficient = statData.damageCoefficient;

        activatedResearchList = new List<string>(statData.activatedResearchList);

        if (unitIllustration != null && statData.unitIllustration != null)
        {
            unitIllustration.sprite = statData.unitIllustration;
        }

        Debug.Log($"{unitName} ���� �ʱ�ȭ �Ϸ�.");
    }

    public void SetDestination(Vector3 destination)
    {
        forwardDestination = destination;
        startPosition = transform.position;
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
        if (isCombatInProgress)
        {
            currentMana += manaRecoveryPerSecond * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);

            if (targetedEnemy == null || !targetedEnemy.activeSelf)
            {
                FindNewTarget();
                if (targetedEnemy == null)
                {
                    if (BattleManager.Instance != null && movementLogic != null)
                    {
                        Vector3 forwardDestination = (factionType == FactionType.TYPE.Owl) ?
                            BattleManager.Instance.enemyForwardPoint.position :
                            BattleManager.Instance.allyForwardPoint.position;

                        MoveTo(forwardDestination);
                    }
                }
            }

            if (targetedEnemy != null)
            {
                distanceToTargetedEnemy = Vector2.Distance(transform.position, targetedEnemy.transform.position);

                if (distanceToTargetedEnemy <= attackLogic.attackRange)
                {
                    movementLogic?.StopMoveRoutine();

                    if (skillLogic != null && currentMana >= skillLogic.manaCost && !skillLogic.IsCasting)
                    {
                        UseActiveSkill();
                    }
                    else if (attackLogic != null && !attackLogic.IsAttacking)
                    {
                        PerformBasicAttack();
                    }
                }
                else
                {
                    MoveTo(targetedEnemy.transform.position);
                }
            }
        }
    }

    private void FindNewTarget()
    {
        List<UnitBase> potentialTargets = (factionType == FactionType.TYPE.Owl) ? BattleManager.Instance.enemyUnits : BattleManager.Instance.allyUnits;

        GameObject nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (UnitBase potentialTarget in potentialTargets)
        {
            if (potentialTarget == null || !potentialTarget.gameObject.activeSelf)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);

            if (distance < minDistance && distance <= enemySearchRange)
            {
                minDistance = distance;
                nearestEnemy = potentialTarget.gameObject;
            }
        }
        if (nearestEnemy != null)
        {
            Debug.Log($"{unitName} Ÿ���� ã��: {nearestEnemy.name}");
        }
        else
        {
            Debug.LogWarning($"{unitName} Ÿ���� ��ã��. EnemySearchRange: {enemySearchRange}");
        }
        SetTarget(nearestEnemy);
    }

    public virtual void SetTarget(GameObject target)
    {
        targetedEnemy = target;
        isTargetingAvailable = (target != null);
        isTargetEliminated = false;

        if (target != null)
        {
            distanceToTargetedEnemy = Vector2.Distance(transform.position, targetedEnemy.transform.position);
        }
        else
        {
            distanceToTargetedEnemy = 0.0f;
        }
    }

    public virtual void MoveTo(Vector3 destination)
    {
        if (movementLogic != null)
        {
            movementLogic.StartMove(destination, movementSpeed);
        }
        else
        {
            Debug.LogWarning($"{unitName}�� �̵� ����(UnitMovementController)�� �Ҵ���� �ʾ� �̵� �Ұ�. Ȯ�� �ʿ�.");
        }
    }

    public virtual void PerformBasicAttack()
    {
        if (attackLogic != null)
        {
            if (targetedEnemy == null || !isTargetingAvailable)
            {
                Debug.LogWarning($"{unitName}�� Ÿ���� ���� �⺻ ������ ���� �Ұ�.");
                return;
            }
            if (distanceToTargetedEnemy > attackLogic.attackRange)
            {
                return;
            }

            attackLogic.StartAttack(this, targetedEnemy);
        }
        else
        {
            Debug.LogWarning($"{unitName}�� ���� ����(BaseAttack)�� �Ҵ���� �ʾ� ���� �Ұ�.");
        }
    }

    public virtual void TakeDamage(float rawDamage)
    {
        float finalDamage = DamageCalculator.CalculateDamage(rawDamage, defensePower);
        currentHealth -= finalDamage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual bool UseActiveSkill()
    {
        if (skillLogic != null && currentMana >= skillLogic.manaCost && !skillLogic.IsCasting)
        {
            skillLogic.StartCast(this, targetedEnemy);
            currentMana -= skillLogic.manaCost;
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void ApplyPassiveSkills()
    {
        if (passiveLogic != null)
        {
            passiveLogic.ApplyEffect(this);
        }
        else
        {
            Debug.LogWarning($"{unitName}�� �нú� ����(PassiveSkillHandler)�� �Ҵ���� ����. Ȯ�� �ʿ�.");
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{unitName}��(��) ����");
        isCombatInProgress = false;

        attackLogic?.StopAttack();
        skillLogic?.StopCast();
        movementLogic?.StopMoveRoutine();

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnUnitDied(this);
        }

        Destroy(gameObject);
    }

    public void SetCombatStatus(bool inCombat)
    {
        isCombatInProgress = inCombat;

        if (movementLogic != null)
        {
            movementLogic.gameObject.SetActive(inCombat);
        }

        if (inCombat)
        {
            FindNewTarget();
        }
    }
}