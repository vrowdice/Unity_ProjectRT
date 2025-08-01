using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Unit", menuName = "Unit")]

public class UnitStatBase : ScriptableObject
{
    public GameObject prefab;

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

    //공/수 여부
    public bool isAttack;

    /////////////////////////////////////////////////
    [Header("연구 및 일러스트")]
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
        many,
        single
    }

    [Header("기본 공격")]
    //이름
    public BasicAttackName basicAttackName;
    //타입
    public BasicAttackType basicAttackType;
    //피해 계수
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
}
