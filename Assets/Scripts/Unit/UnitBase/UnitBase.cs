using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitBase : MonoBehaviour
{
    [Header("초기 스탯")]
    [SerializeField] private UnitStatBase initialStat;

    private UnitStatBase stat;
    public UnitStatBase UnitStat => stat;

    // 데이터에서 파생되는 읽기전용 값들
    public string UnitName { get; private set; }
    public float AttackPower { get; private set; }
    public float DefensePower { get; private set; }
    public float MaxHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackSpeed { get; private set; }      // APS
    public float AttackCycleSec { get; private set; }
    public float AttackActiveSec { get; private set; }
    public float AttackRecoverySec { get; private set; }
    public float DamageCoefficient { get; private set; }
    public int AttackCount { get; private set; }
    public float EnemySearchRange { get; private set; }

    // 종족(Race) - 기존 Faction 호환
    public FactionType.TYPE Faction => stat != null ? stat.factionType : FactionType.TYPE.None;
    public FactionType.TYPE Race => Faction;

    // 팀(Ally/Enemy) - 전투 피아 판정
    public TeamSide Team { get; private set; } = TeamSide.Neutral;

    // 자원/체력
    public float CurrentMana { get; private set; }
    public float MaxMana { get; private set; }
    public float ManaRecoveryOnBasicAttack { get; private set; }
    public float ManaRecoveryPerSecond { get; private set; }
    public float CurrentHealth { get; private set; }

    // 상태
    public bool IsCombatInProgress { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    // 컴포넌트
    private UnitMovementController movementLogic;
    private UnitTargetingController targetingLogic;
    private BaseAttack attackLogic;

    private Coroutine manaCoroutine;

    private void Awake()
    {
        movementLogic = GetComponent<UnitMovementController>();
        targetingLogic = GetComponent<UnitTargetingController>();
        attackLogic = GetComponent<BaseAttack>();

        if (stat == null && initialStat != null)
            Initialize(initialStat);
    }

    private void OnDisable()
    {
        if (manaCoroutine != null) { StopCoroutine(manaCoroutine); manaCoroutine = null; }
        IsCombatInProgress = false;
    }

    // 런타임 팀 지정 + 레이어 반영
    public void AssignTeam(TeamSide newTeam, bool applyLayerRecursive = true)
    {
        Team = newTeam;

        // 레이어 동기화
        int layer = TeamLayers.GetUnitLayer(newTeam);
        if (applyLayerRecursive) ApplyLayerRecursive(gameObject, layer);
        else gameObject.layer = layer;

        // 타게팅 마스크 갱신
        GetComponent<UnitTargetingController>()?.RefreshMask();
    }

    // 레이어 재귀 적용(내부 유틸)
    private void ApplyLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            ApplyLayerRecursive(t.gameObject, layer);
    }
    private void OnEnable()
    {
        int layer = TeamLayers.GetUnitLayer(Team);
        ApplyLayerRecursive(gameObject, layer);
    }


    // 초기화
    public void Initialize(UnitStatBase newStat)
    {
        if (newStat == null)
        {
            Debug.LogError($"[{name}] Initialize 실패: stat이 null");
            return;
        }

        stat = newStat;

        UnitName = stat.unitName;
        AttackPower = stat.attackPower;
        DefensePower = stat.defensePower;
        MaxHealth = stat.maxHealth;
        MoveSpeed = stat.moveSpeed;
        AttackRange = stat.attackRange;

        AttackSpeed = Mathf.Max(0.0f, stat.attackSpeed);
        AttackCount = Mathf.Max(1, stat.attackCount);
        DamageCoefficient = stat.damageCoefficient;
        EnemySearchRange = stat.enemySearchRange;

        if (AttackSpeed > 0.0f)
        {
            AttackCycleSec = 1.0f / AttackSpeed;
            float r = Mathf.Clamp01(stat.attackDelayRatio);
            AttackRecoverySec = AttackCycleSec * r;
            AttackActiveSec = AttackCycleSec - AttackRecoverySec;
        }
        else
        {
            AttackCycleSec = AttackActiveSec = AttackRecoverySec = 0.0f;
        }

        MaxMana = stat.maxMana;
        CurrentMana = Mathf.Clamp(stat.baseMana, 0f, MaxMana);
        ManaRecoveryOnBasicAttack = stat.manaRecoveryOnAttack;
        ManaRecoveryPerSecond = stat.manaRecoveryPerSecond;

        CurrentHealth = MaxHealth;
        IsCombatInProgress = false;

        if (manaCoroutine != null) StopCoroutine(manaCoroutine);
        manaCoroutine = StartCoroutine(ManaRecoveryRoutine());

        // 공격 템포 주입(있으면)
        attackLogic?.SetTempo(AttackActiveSec, AttackRecoverySec);
    }

    // 마나
    public void AddMana(float amount)
    {
        if (MaxMana <= 0.0f) return;
        CurrentMana = Mathf.Clamp(CurrentMana + amount, 0.0f, MaxMana);
    }

    public bool UseMana(float amount)
    {
        if (amount <= 0.0f) return true;
        if (CurrentMana >= amount) { CurrentMana -= amount; return true; }
        return false;
    }

    public void OnBasicAttackLanded()
    {
        AddMana(ManaRecoveryOnBasicAttack);
    }

    private IEnumerator ManaRecoveryRoutine()
    {
        var wait = new WaitForSeconds(1.0f);
        while (!IsDead)
        {
            if (!IsCombatInProgress)
                AddMana(ManaRecoveryPerSecond);
            yield return wait;
        }
    }

    // 체력/사망
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
        attackLogic?.StopAttack();
        movementLogic?.StopMove();
        gameObject.SetActive(false); // 풀링 전제
    }

    // 이벤트
    public event System.Action<UnitBase, GameObject> OnBasicAttackStarted;
    public event System.Action<UnitBase, GameObject, float> OnBasicAttackHit;
    public event System.Action<UnitBase, GameObject> OnSkillCastStarted;
    public event System.Action<UnitBase, GameObject> OnSkillCastFinished;
    public event System.Action<UnitBase, float, GameObject> OnDamaged;
    public event System.Action<UnitBase> OnDied;

    public void NotifyBasicAttackStart(GameObject target) => OnBasicAttackStarted?.Invoke(this, target);
    public void NotifyBasicAttackHit(GameObject target, float dmg) => OnBasicAttackHit?.Invoke(this, target, dmg);
    public void NotifySkillCastStart(GameObject target) => OnSkillCastStarted?.Invoke(this, target);
    public void NotifySkillCastFinish(GameObject target) => OnSkillCastFinished?.Invoke(this, target);

    public void NotifyAttackActiveBegin() { IsCombatInProgress = true; }
    public void NotifyAttackActiveEnd() { IsCombatInProgress = false; }
}
