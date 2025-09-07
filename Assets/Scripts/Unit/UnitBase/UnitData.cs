using System.Collections.Generic;
using UnityEngine;

// ��ġ ���Է½� 0����
// �� ģ���� ���� �̷��� �¾�ٶ�� ����
// GameDataManager�� �߾ӿ��� �⺻���� �������̵��ؼ� ���� �⺻ �������� ������ ��� ������

[CreateAssetMenu(fileName = "NewUnitStat", menuName = "Battle/Unit Stat")]
public class UnitData : ScriptableObject
{
    [Header("�ڱ�Ұ�")]
    public string unitName;
    public Sprite unitIcon;
    public FactionType.TYPE factionType;
    public UnitTagType unitTagType;

    [Header("�ĺ���")]
    // �־ �׸� ��� �׸������� �̰� �����صθ� ����
    public string unitKey;

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

    // ������ ���� ��ȿ�� �˻�, �������� ���� �־�а� �Ű�x
#if UNITY_EDITOR
    private void OnValidate()
    {
        // ����/NaN ���
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
