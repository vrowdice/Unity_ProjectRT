using UnityEngine;

// ��ġ ���Է½� 0����

[CreateAssetMenu(fileName = "NewUnitStat", menuName = "Battle/Unit Stat")]
public class UnitData : ScriptableObject
{
    [Header("�ڱ�Ұ�")]
    public string unitName;
    public Sprite unitIcon;
    public FactionType.TYPE factionType;
    public UnitTagType unitTagType;

    [Header("�⺻ �ɷ�ġ")]
    [Min(0)] public float attackPower;
    [Min(0)] public float defensePower;
    [Min(0)] public float maxHealth;
    [Min(0)] public float moveSpeed;
    [Min(0)] public float attackRange;
    [Min(0)] public float enemySearchRange;

    [Header("���� �ӵ�/Ÿ�̹�")]
    [Min(0)] public float attackSpeed;           // APS, 0=���� �� ��
    [Range(0, 1)] public float attackDelayRatio;  // �� ����Ŭ �� �ĵ� ����(0~1)

    [Header("���� ����")]
    [Min(0)] public int attackCount;         
    [Min(0)] public float damageCoefficient;  

    [Header("����")]
    [Min(0)] public float baseMana;
    [Min(0)] public float maxMana;
    [Min(0)] public float manaRecoveryOnAttack;
    [Min(0)] public float manaRecoveryPerSecond;

    [System.Serializable]
    public class ActiveSkillSpec
    {
        [Header("ǥ��")]
        public string displayName;
        public string skillLogic;     
        public string activeSkillType; 
        public bool isSkillFinished;

        [Header("��ġ")]
        public float manaCost;
        public float damageCoeff;
        public int attackCount;
        public float multiTargetRange;
        public int maxMultiTarget;
        public float otherSkillCoefficient;
        public float otherSkillRange;

        [Header("���ҽ�")]
        public GameObject areaAttackPrefab;
        public string areaAttackDirection;
        public string areaAttackSpawnPosition;

        [Header("�ΰ�ȿ�� Ű")]
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

    [Header("������")]
    public GameObject prefab;
    public Sprite unitIllustration;
}
