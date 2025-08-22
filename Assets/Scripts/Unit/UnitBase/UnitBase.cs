using UnityEngine;

[DisallowMultipleComponent]
public class UnitBase : MonoBehaviour
{
    [Header("초기 템플릿")]
    [SerializeField] private UnitStatBase initialStat;

    private UnitStatBase stat;
    public UnitStatBase UnitStat => stat;

    // 파생/최종 값(컨트롤러가 여길 인식함)
    public string UnitName { get; private set; }
    public float AttackPower { get; private set; }
    public float DefensePower { get; private set; }
    public float MaxHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackRange { get; private set; }
    public float EnemySearchRange { get; private set; }

    public float AttackSpeed { get; private set; }    // APS
    public float AttackCycleSec { get; private set; }
    public float AttackActiveSec { get; private set; }
    public float AttackRecoverySec { get; private set; }
    public int AttackCount { get; private set; }
    public float DamageCoefficient { get; private set; }

    // 자원/체력
    public float CurrentMana { get; private set; }
    public float MaxMana { get; private set; }
    public float ManaRecoveryOnBasicAttack { get; private set; }
    public float ManaRecoveryPerSecond { get; private set; }
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0.0f;

    // 팀/진영
    public FactionType.TYPE Faction => stat ? stat.factionType : FactionType.TYPE.None;
    public TeamSide Team { get; private set; } = TeamSide.Neutral;

    // 상태
    public bool IsCombatInProgress { get; private set; }

    // 캐시
    private UnitMovementController movement;
    private UnitTargetingController targeting;
    private BaseAttack attackLogic;

    private void Awake()
    {
        movement = GetComponent<UnitMovementController>();
        targeting = GetComponent<UnitTargetingController>();
        attackLogic = GetComponent<BaseAttack>();

        if (!stat && initialStat) Initialize(initialStat);
        else EnsureSafeDefaults();
    }

    private void OnEnable()
    {
        ApplyLayerToHierarchy(gameObject, TeamLayers.GetUnitLayer(Team));
    }

    private void EnsureSafeDefaults()
    {
        MaxHealth = Mathf.Max(1.0f, MaxHealth);
        CurrentHealth = MaxHealth;
        CurrentMana = Mathf.Clamp(CurrentMana, 0.0f, MaxMana);
    }

    // 팀/레이어
    // inspector에서 레이어 지정하는거 잊지 않기 제발
    public void AssignTeam(TeamSide newTeam, bool applyLayerRecursive = true)
    {
        Team = newTeam;
        int layer = TeamLayers.GetUnitLayer(newTeam);
        if (applyLayerRecursive) ApplyLayerToHierarchy(gameObject, layer);
        else gameObject.layer = layer;
        targeting?.RefreshMask();
    }

    private static void ApplyLayerToHierarchy(GameObject root, int layer)
    {
        var s = new System.Collections.Generic.Stack<Transform>(32);
        s.Push(root.transform);
        while (s.Count > 0)
        {
            var t = s.Pop();
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++) s.Push(t.GetChild(i));
        }
    }

    public void Initialize(UnitStatBase newStat)
    {
        if (!newStat) { Debug.LogError($"[{name}] Initialize 실패: stat null"); return; }
        stat = newStat;

        stat = ScriptableObject.Instantiate(newStat);
        stat.name += " (Runtime)";
        stat.hideFlags = HideFlags.DontSave;

        // SO 기본값
        float attackPower = Mathf.Max(0.0f, stat.attackPower);
        float defensePower = Mathf.Max(0.0f, stat.defensePower);
        float maxHealth = Mathf.Max(1.0f, stat.maxHealth);
        float moveSpeed = Mathf.Max(0.0f, stat.moveSpeed);
        float attackRange = Mathf.Max(0.0f, stat.attackRange);
        float enemySearch = Mathf.Max(0.0f, stat.enemySearchRange);
        float attackSpeed = Mathf.Max(0.0f, stat.attackSpeed);
        int attackCount = stat.attackCount > 0 ? stat.attackCount : 1;
        float damageCoeff = stat.damageCoefficient > 0.0f ? stat.damageCoefficient : 1.0f;
        float delayRatio = Mathf.Clamp01(stat.attackDelayRatio);

        float maxMana = Mathf.Max(0.0f, stat.maxMana);
        float baseMana = Mathf.Clamp(stat.baseMana, 0.0f, maxMana);
        float manaOnAtk = Mathf.Max(0.0f, stat.manaRecoveryOnAttack);
        float manaPerSec = Mathf.Max(0.0f, stat.manaRecoveryPerSecond);

        // 프리팹 오버라이드 병합
        var ov = GetComponent<UnitStatOverride>();
        if (ov)
        {
            attackPower = ov.attackPower.Merge(attackPower);
            defensePower = ov.defensePower.Merge(defensePower);
            maxHealth = Mathf.Max(1.0f, ov.maxHealth.Merge(maxHealth));
            moveSpeed = ov.moveSpeed.Merge(moveSpeed);
            attackRange = ov.attackRange.Merge(attackRange);
            enemySearch = ov.enemySearchRange.Merge(enemySearch);

            attackSpeed = ov.attackSpeed.Merge(attackSpeed);
            if (ov.attackDelayRatioOverride >= 0.0f) delayRatio = Mathf.Clamp01(ov.attackDelayRatioOverride);
            if (ov.attackCountOverride >= 1) attackCount = ov.attackCountOverride;
            damageCoeff = ov.damageCoefficient.Merge(damageCoeff);

            maxMana = ov.maxMana.Merge(maxMana);
            baseMana = Mathf.Clamp(ov.baseMana.Merge(baseMana), 0.0f, maxMana);
            manaOnAtk = ov.manaRecoveryOnAttack.Merge(manaOnAtk);
            manaPerSec = ov.manaRecoveryPerSecond.Merge(manaPerSec);
        }

        // 최종 반영
        UnitName = stat.unitName;
        AttackPower = attackPower;
        DefensePower = defensePower;
        MaxHealth = maxHealth;
        MoveSpeed = moveSpeed;
        AttackRange = attackRange;
        EnemySearchRange = enemySearch;
        AttackSpeed = attackSpeed;
        AttackCount = attackCount;
        DamageCoefficient = damageCoeff;

        if (AttackSpeed > 0.0f)
        {
            AttackCycleSec = 1.0f / AttackSpeed;
            AttackRecoverySec = AttackCycleSec * delayRatio;
            AttackActiveSec = Mathf.Max(0f, AttackCycleSec - AttackRecoverySec);
        }
        else
        {
            AttackCycleSec = AttackActiveSec = AttackRecoverySec = 0.0f;
        }

        MaxMana = maxMana;
        CurrentMana = baseMana;
        ManaRecoveryOnBasicAttack = manaOnAtk;
        ManaRecoveryPerSecond = manaPerSec;

        CurrentHealth = MaxHealth;
        IsCombatInProgress = false;

        // 공격 템포있으면 발동
        attackLogic?.SetTempo(AttackActiveSec, AttackRecoverySec);
    }

    // 자원/체력 
    public void AddMana(float amount)
    {
        if (MaxMana <= 0.0f || amount == 0.0f) return;
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
        if (ManaRecoveryOnBasicAttack > 0.0f) AddMana(ManaRecoveryOnBasicAttack);
    }

    public void Heal(float amount)
    {
        if (MaxHealth <= 0.0f || amount <= 0.0f) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0.0f, MaxHealth);
    }
    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        float applied = Mathf.Max(0.0f, damage) - Mathf.Max(0.0f, DefensePower);
        if (applied < 0.0f) applied = 0.0f;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - applied);
        OnDamaged?.Invoke(this, applied, null);
        if (IsDead) OnDeath();
    }
    private void OnDeath()
    {
        OnDied?.Invoke(this);
        attackLogic?.StopAttack();
        movement?.StopMove();
        gameObject.SetActive(false);
    }

    // 이벤트/알림
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
