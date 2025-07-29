using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 변수 이름 마음대로 변경 가능 / 주석으로 변경 전 이름만 적어주길 바람...
// 필요없다 판단되는 코드도 편하게 주석 처리 해주시길....
// 디버깅 추가도 자유롭게 해주세요~

// Battle Scene Unit의 코어
public class UnitBase : MonoBehaviour
{
    /////////////////////////////////////////////////
    [Header("기본 정보 및 스탯")]
    public string unitName;
    public string unitType; // 타입
    public string affiliation; // 소속

    // 공격력
    public float attackPower;
    // 방어력
    public float defensePower;
    // 최대 체력
    public float maxHealth;
    // 이동 속도
    public float moveSpeed;
    // 최대 마나
    public float maxMana;
    // 기본 공격 후 회복되는 마나량
    public float manaRecoveryOnBasicAttack;
    // 초당 마나 회복량
    public float manaRecoveryPerSecond;

    // 현재 체력
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

    /////////////////////////////////////////////////
    [Header("연구 및 일러스트")]
    public List<string> activatedResearchList;
    public Image unitIllustration;

    /////////////////////////////////////////////////
    [Header("기본 공격")]
    public float damageCoefficient;
    // 주기
    public float attackFrequency;
    // 타수 
    public int attackCount;
    // 거리
    public float attackRange;

    // 다수 기본 공격 시 타겟 탐색 범위
    public float multiTargetRange;
    // 다수 기본 공격 시 최대 타겟 수
    public int maxMultiTargets;

    public GameObject areaAttackPrefab;
    // 범위 공격이 발동 방향 
    public Vector2 areaAttackDirection;
    // 범위 공격 프리팹이 생성될 위치
    public Vector2 areaAttackSpawnPosition;
    // 범위 공격 이펙트의 좌우 반전 여부
    public bool isAreaAttackFlipped;

    /////////////////////////////////////////////////
    [Header("액티브 스킬 관련")]

    public string activeSkillName;
    // 액티브 스킬의 타입
    public string activeSkillType;
    // 스킬 쿨다운 시간
    public float activeSkillCooldown;
    // 사용 가능 여부
    public bool isSkillFinished;

    // 액티브 스킬 피해 
    public float activeSkillDamageCoefficient;
    // 액티브 스킬의 타수
    public int activeSkillAttackCount;

    // 다수 공격 액티브 스킬 시 타겟 탐색 범위
    [Header("다수 공격 액티브 스킬 데이터")]
    public float activeMultiTargetRange;
    // 다수 공격 액티브 스킬 시 최대 타겟 수
    public int activeMaxMultiTargets;

    // 범위 공격 액티브 스킬 시 사용할 공격 범위
    [Header("범위 공격 액티브 스킬 데이터")]
    public GameObject activeAreaAttackPrefab;
    // 범위 공격 액티브 스킬이 발동 방향
    public Vector2 activeAreaAttackDirection;
    // 범위 공격 액티브 스킬 프리팹이 생성될 위치
    public Vector2 activeAreaAttackSpawnPosition;
    // 범위 공격 액티브 스킬 이펙트의 좌우 반전 여부
    public bool isActiveAreaAttackFlipped;

    // 기타 액티브 스킬
    [Header("기타 액티브 스킬 데이터")]
    public float otherSkillCoefficient;
    public float otherSkillRange;

    /////////////////////////////////////////////////
    [Header("패시브 스킬 관련")]

    // 패시브 스킬의 효과 
    public float passiveSkillCoefficient;
    // 패시브 스킬의 적용 범위
    public float passiveSkillRange;

    /////////////////////////////////////////////////
    [Header("전투 상태 및 타겟팅")]

    // 유닛이 공격 진영인지 방어 진영인지 여부 (True: 공격, False: 수비)
    public bool isAttacker;
    // 현재 전투가 진행 중인지 여부 (True: 전투 중)
    public bool isCombatInProgress;
    // 타겟팅 가능한 적이 현재 존재하는지 여부 (True: 대상 존재, False: 대상 없음)
    public bool isTargetingAvailable;
    // 현재 타겟팅된 적 오브젝트 
    public GameObject targetedEnemy;
    // 타겟팅된 적이 제거되었는지 여부
    public bool isTargetEliminated;

    // 적 탐색 범위
    public float enemySearchRange;
    // 현재 타겟팅된 적과의 거리
    public float distanceToTargetedEnemy;

    /////////////////////////////////////////////////
    [Header("외부 클래스")]

    public BaseAttack attackLogic;
    public BaseSkill skillLogic;
    public PassiveSkillHandler passiveLogic;
    public UnitMovementController movementLogic;

    protected virtual void Awake()
    {
        currentHealth = maxHealth; // 시작 시 현재 체력을 최대 체력으로 초기화
        currentMana = maxMana;     // 시작 시 현재 마나를 최대 마나로 초기화
        isCombatInProgress = false; // 초기에는 전투 중이 아님
        isTargetingAvailable = false; // 초기에는 타겟팅 대상이 없음
        isTargetEliminated = false; // 초기에는 대상이 제거되지 않음

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
        isTargetingAvailable = (target != null); // 타겟이 있으면 true, 없으면 false
        isTargetEliminated = false;

        if (target != null)
        {
            distanceToTargetedEnemy = Vector2.Distance(transform.position, targetedEnemy.transform.position);
        }
        else
        {
            distanceToTargetedEnemy = 0.0f; // 타겟이 없으면 거리를 0으로 설정
        }
    }

    // 이동
    public virtual void MoveTo(Vector3 destination)
    {
        if (movementLogic != null)
        {
            movementLogic.StartMove(destination, moveSpeed); 
        }
        else
        {
            Debug.LogWarning($"{unitName}에 이동 로직(UnitMovementController)이 할당되지 않아 이동 불가. 확인 필요.");
        }
    }

    // 기본 공격
    public virtual void PerformBasicAttack()
    {
        if (attackLogic != null)
        {
            // 타겟이 없거나 타겟팅 불가능 상태이면 공격 불가
            if (targetedEnemy == null || !isTargetingAvailable)
            {
                Debug.LogWarning($"{unitName}의 타겟이 없어 기본 공격을 수행 불가.");
                return;
            }
            // 타겟이 공격 사거리 밖에 있으면 공격 불가
            if (distanceToTargetedEnemy > attackRange)
            {
                return;
            }

            // 공격 로직 컴포넌트에게 공격 시작을 지시합니다.
            attackLogic.StartAttack(this, targetedEnemy);
        }
        else
        {
            Debug.LogWarning($"{unitName}에 공격 로직(BaseAttack)이 할당되지 않아 공격 불가.");
        }
    }

    // 피해
    public virtual void TakeDamage(float rawDamage)
    {
        float finalDamage = DamageCalculator.CalculateDamage(rawDamage, defensePower);
        currentHealth -= finalDamage; // 최종 피해량만큼 체력 감소

        // 체력이 0 이하가 되면 유닛 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 액티브 스킬을 사용 명령
    public virtual bool UseActiveSkill()
    {
        if (skillLogic != null)
        {
            skillLogic.StartCast(this, targetedEnemy, activeSkillCooldown); // 쿨다운 시간 전달
            return true;
        }
        else
        {
            return false;
        }
    }

    // 패시브 스킬 효과를 적용
    public virtual void ApplyPassiveSkills()
    {
        if (passiveLogic != null)
        {
            passiveLogic.ApplyEffect(this);
            Debug.Log($"{unitName}이(가) 패시브 스킬 효과를 적용.");
        }
        else
        {
            Debug.LogWarning($"{unitName}에 패시브 로직(PassiveSkillHandler)이 할당되지 않음. 확인 필요.");
        }
    }

    // 제거 처리
    protected virtual void Die()
    {
        Debug.Log($"{unitName}이(가) 제거");
        isCombatInProgress = false; // 전투 중단 상태로 변경

        attackLogic?.StopAttack();
        skillLogic?.StopCast();
        movementLogic?.StopMoveRoutine();

        Destroy(gameObject); 
    }

    // 전투 상태를 시작 or 중단
    public void SetCombatStatus(bool inCombat)
    {
        isCombatInProgress = inCombat;
        if (!inCombat)
        {
            Debug.Log($"{unitName}이(가) 전투를 중단");
            attackLogic?.StopAttack();
            skillLogic?.StopCast();
            movementLogic?.StopMoveRoutine();
        }
        else
        {
            Debug.Log($"{unitName}이(가) 전투를 시작");
        }
    }
}