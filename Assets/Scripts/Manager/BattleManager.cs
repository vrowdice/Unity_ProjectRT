using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("배틀 로딩 매니저")]
    [SerializeField] private BattleLoadingManager m_battleLoadingManager = null;

    [HideInInspector] public bool isSettingClear = false;
    [HideInInspector] public bool isAttackField = true;
    [HideInInspector] public GameObject enemyBattleBeforeUI;
    [HideInInspector] public GameObject battleBeforeUI;

    [Header("전진 목표 지점")]
    public Transform enemyForwardPoint; // 적군 전진 목표 지점
    public Transform allyForwardPoint;

    // 전투중 유닛   
    public List<UnitBase> allyUnits = new List<UnitBase>();
    public List<UnitBase> enemyUnits = new List<UnitBase>();

    // Start -> Awake로 변경 / 싱글톤을 먼저 초기화
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(m_battleLoadingManager.InitializeBattleScene());
    }

    void Update()
    {
        if (isSettingClear == true)
        {
            MoveField();
        }
    }

    // 실제 전투를 시작하는 함수
    public void StartBattle()
    {
        Debug.Log("전투 시작! StartBattle() 함수 호출됨. 이 BattleManager 오브젝트의 이름은: " + this.gameObject.name);

        allyUnits.Clear();
        enemyUnits.Clear();

        UnitBase[] allUnits = FindObjectsOfType<UnitBase>();

        foreach (UnitBase unit in allUnits)
        {
            if (unit.factionType == FactionType.TYPE.Owl) 
            {
                allyUnits.Add(unit);
                if (enemyForwardPoint != null)
                {
                    unit.SetDestination(enemyForwardPoint.position);
                }
                else
                {
                    Debug.LogError("오류: 아군 유닛의 목표 지점(enemyForwardPoint)이 할당되지 않았습니다. BattleManager의 Inspector를 확인하세요!");
                }
            }
            else
            {
                enemyUnits.Add(unit);
                // 오류 방지를 위해 null 체크를 추가합니다.
                if (allyForwardPoint != null)
                {
                    unit.SetDestination(allyForwardPoint.position);
                }
                else
                {
                    Debug.LogError("오류: 적군 유닛의 목표 지점(allyForwardPoint)이 할당되지 않았습니다. BattleManager의 Inspector를 확인하세요!");
                }
            }
            unit.SetCombatStatus(true);
        }
    }

    public void OnUnitDied(UnitBase unit)
    {
        Debug.Log($"[BattleManager] {unit.unitName} 사망. 리스트에서 제거.");
        if (unit.factionType == FactionType.TYPE.Owl)
        {
            allyUnits.Remove(unit);
        }
        else
        {
            enemyUnits.Remove(unit);
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (allyUnits.Count == 0)
        {
            Debug.Log("패배~ 아군 병력 전멸했습니다.");
        }
        else if (enemyUnits.Count == 0)
        {
            Debug.Log("승리! 적군 병력이 전멸했습니다.");
        }
    }

    private void MoveField()

    {

        if (isAttackField != true)

        {
            m_battleLoadingManager.mainCamra.transform.position = m_battleLoadingManager.defenseCameraPoint;
        }
        else if (isAttackField == true)

        {
            m_battleLoadingManager.mainCamra.transform.position = m_battleLoadingManager.attackCameraPoint;
        }

    }
}