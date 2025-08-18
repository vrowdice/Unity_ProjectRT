using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStat", menuName = "Unit/UnitStatBase")]
public class UnitStatBase : ScriptableObject
{
    [Header("ID/분류")]
    public string unitName;
    public Sprite unitIcon;
    public FactionType.TYPE factionType;   // 종족(팀 아님)
    public UnitTagType unitTagType;        // Melee/Range/Defense

    [Header("기본 능력치(테이블 매핑)")]
    [Min(0)] public float attackPower;
    [Min(0)] public float defensePower;
    [Min(0)] public float maxHealth;
    [Min(0)] public float moveSpeed;      // MaxSpeed -> moveSpeed
    [Min(0)] public float attackRange;
    [Min(0)] public float attackSpeed;    // APS(초당 공격수)

    [Tooltip("한 사이클에서 후딜 비율(0~1). 테이블 값 없으면 0 유지")]
    [Range(0, 1)] public float attackDelayRatio = 0f;

    [Header("마나(테이블 매핑)")]
    [Min(0)] public float baseMana;
    [Min(0)] public float maxMana;
    [Min(0)] public float manaRecoveryOnAttack;
    [Min(0)] public float manaRecoveryPerSecond;

    [Header("공격 세부(테이블 매핑)")]
    [Min(0)] public int attackCount = 1;
    [Min(0)] public float damageCoefficient = 1.0f;
    [Min(0)] public float enemySearchRange = 0.0f;

    [Header("기본공격 보너스 효과 키(선택)")]
    public string baseAttackBonusEffect;

    [System.Serializable]
    public class ActiveSkillSpec
    {
        [Header("표/노출 이름")]
        public string displayName;     // SkillName
        public string skillLogic;      // ActiveSkillLogic(클래스명)
        public string activeSkillType; // 타입 문자열(선택)
        public bool isSkillFinished; // 완료형 여부(선택)

        [Header("수치")]
        public float manaCost;         // ManaCost
        public float damageCoeff;      // 데미지 계수
        public int attackCount;      // 타수
        public float multiTargetRange;
        public int maxMultiTarget;
        public float otherSkillCoefficient;
        public float otherSkillRange;

        [Header("리소스/스폰(선택)")]
        public GameObject areaAttackPrefab;
        public string areaAttackDirection;
        public string areaAttackSpawnPosition;

        [Header("부가효과 키(선택)")]
        public string bonusEffectKey;
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

    [Header("에셋(선택)")]
    public GameObject prefab;
    public Sprite unitIllustration;
}
