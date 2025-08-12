using System.Collections;
using UnityEngine;

public class UnitBase : MonoBehaviour
{
    [Header("�ʱ� ���� ")]
    [SerializeField] private UnitStatBase initialStat;

    // ���� ���� ���� 
    private UnitStatBase stat;
    public UnitStatBase UnitStat => stat;

    public string UnitName { get; private set; }
    public float AttackPower { get; private set; }
    public float DefensePower { get; private set; }
    public float MaxHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackSpeed { get; private set; }
    public float AttackFrequency { get; private set; }
    public float DamageCoefficient { get; private set; }
    public int AttackCount { get; private set; }
    public float EnemySearchRange { get; private set; }

    // ������ ���ȿ��� �Ļ� 
    public FactionType.TYPE Faction => stat != null ? stat.factionType : FactionType.TYPE.None;

    // ����/ü��
    public float CurrentMana { get; private set; }
    public float MaxMana { get; private set; }
    public float ManaRecoveryOnBasicAttack { get; private set; }
    public float ManaRecoveryPerSecond { get; private set; }
    public float CurrentHealth { get; private set; }

    // ����
    public bool IsCombatInProgress { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    // ������Ʈ
    private UnitMovementController movementLogic;
    private UnitTargetingController targetingLogic;
    private BaseAttack attackLogic;

    private Transform currentTarget;
    private bool isBattleStarted = false;
    private Coroutine battleCoroutine;
    private Coroutine manaCoroutine;

    private void Awake()
    {
        movementLogic = GetComponent<UnitMovementController>();
        targetingLogic = GetComponent<UnitTargetingController>();
        attackLogic = GetComponent<BaseAttack>();

        if (stat == null && initialStat != null)
            Initialize(initialStat);
    }

    private void OnEnable()
    {
        if (IsDead)
        {

        }
    }

    private void OnDisable()
    {
        // �ڷ�ƾ ���� ����
        if (battleCoroutine != null) { StopCoroutine(battleCoroutine); battleCoroutine = null; }
        if (manaCoroutine != null) { StopCoroutine(manaCoroutine); manaCoroutine = null; }
        isBattleStarted = false;
        IsCombatInProgress = false;
    }

    // �ʱ�ȭ
    public void Initialize(UnitStatBase newStat)
    {
        if (newStat == null)
        {
            Debug.LogError($"[{name}] Initialize ����: stat�� null");
            return;
        }

        stat = newStat;

        UnitName = stat.unitName;
        AttackPower = stat.attackPower;
        DefensePower = stat.defensePower;
        MaxHealth = stat.maxHealth;
        MoveSpeed = stat.moveSpeed;
        AttackRange = stat.attackRange;
        AttackSpeed = stat.attackSpeed;
        AttackFrequency = (AttackSpeed > 0.0f) ? 1.0f / AttackSpeed : 0.0f;
        AttackCount = stat.attackCount;
        DamageCoefficient = stat.damageCoefficient;
        EnemySearchRange = stat.enemySearchRange;

        MaxMana = stat.maxMana;
        CurrentMana = stat.baseMana;
        ManaRecoveryOnBasicAttack = stat.manaRecoveryOnAttack;
        ManaRecoveryPerSecond = stat.manaRecoveryPerSecond;

        CurrentHealth = MaxHealth;
        IsCombatInProgress = false;

        // ���� �ڵ� ȸ�� ����
        if (manaCoroutine != null) StopCoroutine(manaCoroutine);
        manaCoroutine = StartCoroutine(ManaRecoveryRoutine());
    }

    // ����
    public void AddMana(float amount)
    {
        if (MaxMana <= 0.0f) return;
        CurrentMana = Mathf.Clamp(CurrentMana + Mathf.Max(0.0f, amount), 0.0f, MaxMana);
    }

    public bool UseMana(float amount)
    {
        if (amount <= 0.0f) return true;
        if (CurrentMana >= amount)
        {
            CurrentMana -= amount;
            return true;
        }
        return false;
    }

    public void OnBasicAttackLanded()
    {
        CurrentMana += ManaRecoveryOnBasicAttack;
        if (MaxMana > 0.0f) CurrentMana = Mathf.Min(CurrentMana, MaxMana);
    }

    private IEnumerator ManaRecoveryRoutine()
    {
        var wait = new WaitForSeconds(1f);
        while (!IsDead)
        {
            if (!IsCombatInProgress)
            {
                AddMana(ManaRecoveryPerSecond);
            }
            yield return wait;
        }
    }

    // ü��/���
    public void Heal(float amount)
    {
        if (MaxHealth <= 0.0f) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + Mathf.Max(0.0f, amount), 0.0f, MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Max(CurrentHealth - Mathf.Max(0.0f, damage), 0.0f);
        OnDamaged?.Invoke(this, damage, null);
        if (IsDead) OnDeath();
    }

    private void OnDeath()
    {
        OnDied?.Invoke(this);

        isBattleStarted = false;
        attackLogic?.StopAttack();
        movementLogic?.StopMove();

        // Ǯ���� ���ٸ� Destroy ��� ��Ȱ��ȭ�� �����ϴ��� 
        gameObject.SetActive(false);
    }

    // ����
    public void StartBattle(bool isAttacker)
    {
        if (battleCoroutine != null) return;

        isBattleStarted = true;
        battleCoroutine = StartCoroutine(BattleRoutine(isAttacker));
    }

    private IEnumerator BattleRoutine(bool isAttacker)
    {
        while (isBattleStarted && !IsDead)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeSelf)
            {
                targetingLogic?.FindNewTarget();
                currentTarget = targetingLogic?.TargetedEnemy?.transform;
            }

            if (currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, currentTarget.position);

                if (dist <= AttackRange * 0.9f)
                {
                    if (attackLogic != null && !attackLogic.IsAttacking)
                    {
                        attackLogic.StartAttack(this, currentTarget.gameObject);
                        IsCombatInProgress = true;
                        movementLogic?.StopMove();
                    }
                }
                else
                {
                    attackLogic?.StopAttack();
                    IsCombatInProgress = false;
                    movementLogic?.MoveTo(currentTarget.position, MoveSpeed);
                }
            }
            else
            {
                attackLogic?.StopAttack();
                IsCombatInProgress = false;
                Vector3 moveDir = isAttacker ? Vector3.left : Vector3.right;
                movementLogic?.StartMoveInDirection(moveDir, MoveSpeed);
            }

            yield return null;
        }
    }

    // ����Ʈ

    public event System.Action<UnitBase, GameObject> OnBasicAttackStarted;
    public event System.Action<UnitBase, GameObject, float> OnBasicAttackHit;
    public event System.Action<UnitBase, GameObject> OnSkillCastStarted;
    public event System.Action<UnitBase, GameObject> OnSkillCastFinished;
    public event System.Action<UnitBase, float, GameObject> OnDamaged;
    public event System.Action<UnitBase> OnDied;

    public void NotifyBasicAttackStart(GameObject target)
    {
        OnBasicAttackStarted?.Invoke(this, target);
    }
    public void NotifyBasicAttackHit(GameObject target, float damage)
    {
        OnBasicAttackHit?.Invoke(this, target, damage);
    }
    public void NotifySkillCastStart(GameObject target)
    {
        OnSkillCastStarted?.Invoke(this, target);
    }
    public void NotifySkillCastFinish(GameObject target)
    {
        OnSkillCastFinished?.Invoke(this, target);
    }
}
