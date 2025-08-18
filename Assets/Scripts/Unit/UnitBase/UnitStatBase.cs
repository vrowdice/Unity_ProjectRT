using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStat", menuName = "Unit/UnitStatBase")]
public class UnitStatBase : ScriptableObject
{
    [Header("ID/�з�")]
    public string unitName;
    public Sprite unitIcon;
    public FactionType.TYPE factionType;   // ����(�� �ƴ�)
    public UnitTagType unitTagType;        // Melee/Range/Defense

    [Header("�⺻ �ɷ�ġ(���̺� ����)")]
    [Min(0)] public float attackPower;
    [Min(0)] public float defensePower;
    [Min(0)] public float maxHealth;
    [Min(0)] public float moveSpeed;      // MaxSpeed -> moveSpeed
    [Min(0)] public float attackRange;
    [Min(0)] public float attackSpeed;    // APS(�ʴ� ���ݼ�)

    [Tooltip("�� ����Ŭ���� �ĵ� ����(0~1). ���̺� �� ������ 0 ����")]
    [Range(0, 1)] public float attackDelayRatio = 0f;

    [Header("����(���̺� ����)")]
    [Min(0)] public float baseMana;
    [Min(0)] public float maxMana;
    [Min(0)] public float manaRecoveryOnAttack;
    [Min(0)] public float manaRecoveryPerSecond;

    [Header("���� ����(���̺� ����)")]
    [Min(0)] public int attackCount = 1;
    [Min(0)] public float damageCoefficient = 1.0f;
    [Min(0)] public float enemySearchRange = 0.0f;

    [Header("�⺻���� ���ʽ� ȿ�� Ű(����)")]
    public string baseAttackBonusEffect;

    [System.Serializable]
    public class ActiveSkillSpec
    {
        [Header("ǥ/���� �̸�")]
        public string displayName;     // SkillName
        public string skillLogic;      // ActiveSkillLogic(Ŭ������)
        public string activeSkillType; // Ÿ�� ���ڿ�(����)
        public bool isSkillFinished; // �Ϸ��� ����(����)

        [Header("��ġ")]
        public float manaCost;         // ManaCost
        public float damageCoeff;      // ������ ���
        public int attackCount;      // Ÿ��
        public float multiTargetRange;
        public int maxMultiTarget;
        public float otherSkillCoefficient;
        public float otherSkillRange;

        [Header("���ҽ�/����(����)")]
        public GameObject areaAttackPrefab;
        public string areaAttackDirection;
        public string areaAttackSpawnPosition;

        [Header("�ΰ�ȿ�� Ű(����)")]
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

    [Header("��ų �Ķ����")]
    public ActiveSkillSpec active = new ActiveSkillSpec();
    public PassiveSkillSpec passive = new PassiveSkillSpec();

    [Header("����(����)")]
    public GameObject prefab;
    public Sprite unitIllustration;
}
