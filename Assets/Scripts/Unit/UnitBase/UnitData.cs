using System.Collections.Generic;
using UnityEngine;

// 수치 미입력시 0으로
// 이 친구는 원래 이렇게 태어났다라는 느낌
// GameDataManager가 중앙에서 기본값을 오버라이드해서 최종 기본 스냅샷을 지정함 요긴 안전빵

[CreateAssetMenu(fileName = "NewUnitStat", menuName = "Battle/Unit Stat")]
public class UnitData : ScriptableObject
{
    [Header("자기소개")]
    public string unitName;
    public Sprite unitIcon;
    public FactionType.TYPE factionType;
    public UnitTagType unitTagType;

    [Header("식별자")]
    // 있어도 그만 없어도 그만이지만 이거 지정해두면 편함
    public string unitKey;

    [Header("기본 능력치")]
    [Min(0)] public float attackPower;
    [Min(0)] public float defensePower;
    [Min(0)] public float maxHealth;
    [Min(0)] public float moveSpeed;
    [Min(0)] public float attackRange;
    [Min(0)] public float enemySearchRange;

    [Header("공격 속도/타이밍")]
    [Min(0)] public float attackSpeed;           // APS, 0=공격 안 함
    [Range(0, 1)] public float attackDelayRatio;  // 한 사이클 내 후딜 비율(0~1)

    [Header("공격 세부")]
    [Min(0)] public int attackCount;         
    [Min(0)] public float damageCoefficient;  

    [Header("마나")]
    [Min(0)] public float baseMana;
    [Min(0)] public float maxMana;
    [Min(0)] public float manaRecoveryOnAttack;
    [Min(0)] public float manaRecoveryPerSecond;

    [System.Serializable]
    public class ActiveSkillSpec
    {
        [Header("표기")]
        public string displayName;
        public string skillLogic;     
        public string activeSkillType; 
        public bool isSkillFinished;

        [Header("수치")]
        public float manaCost;
        public float damageCoeff;
        public int attackCount;
        public float multiTargetRange;
        public int maxMultiTarget;
        public float otherSkillCoefficient;
        public float otherSkillRange;

        [Header("리소스")]
        public GameObject areaAttackPrefab;
        public string areaAttackDirection;
        public string areaAttackSpawnPosition;

    }

    [System.Serializable]
    public class PassiveSkillSpec
    {
        public string passiveLogic;
        public float passiveCoeff1;
        public float passiveRange1;
        public float passiveCoeff2;
        public float passiveRange2;
    }

    [Header("스킬 파라미터")]
    public ActiveSkillSpec active = new ActiveSkillSpec();
    public PassiveSkillSpec passive = new PassiveSkillSpec();

    [Header("프리팹")]
    public GameObject prefab;
    public Sprite unitIllustration;

    // 에디터 전용 유효성 검사, 안정성을 위해 넣어둔것 신경x
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 음수/NaN 방어
        attackPower = Mathf.Max(0, attackPower);
        defensePower = Mathf.Max(0, defensePower);
        maxHealth = Mathf.Max(1, maxHealth);
        moveSpeed = Mathf.Max(0, moveSpeed);
        attackRange = Mathf.Max(0, attackRange);
        enemySearchRange = Mathf.Max(0, enemySearchRange);
        attackSpeed = Mathf.Max(0, attackSpeed);
        attackDelayRatio = Mathf.Clamp01(attackDelayRatio);
        attackCount = Mathf.Max(1, attackCount);
        damageCoefficient = Mathf.Max(0, damageCoefficient);

        baseMana = Mathf.Max(0, baseMana);
        maxMana = Mathf.Max(0, maxMana);
        baseMana = Mathf.Min(baseMana, maxMana);
        manaRecoveryOnAttack = Mathf.Max(0, manaRecoveryOnAttack);
        manaRecoveryPerSecond = Mathf.Max(0, manaRecoveryPerSecond);
    }
#endif

}
