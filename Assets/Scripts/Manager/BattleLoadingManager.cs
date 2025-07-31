using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLoadingManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("로딩 UI")]
    public GameObject loadingPanel;
    private Image loadingBar;

    [Header("게임 필드")]
    public GameObject battleField;

    private Vector3 defenseCameraPoint;
    private Vector3 attackCameraPoint;
    private Camera mainCamra;

    [Header("게임 데이터 매니저")]
    [SerializeField] private GameDataManager m_gameDataManager = null;

    [Header("유닛 데이터")]
    [SerializeField] private AllyArmyData allyArmyData = null;
    [SerializeField] private EnemyArmyData enemyArmyData = null;

    private List<UnitStatBase> allyArmyDataList = new();
    private List<UnitStatBase> enemyArmyDataList = new();


    //테스트용 후에 실제 값으로 변경해야함
    private bool isAttack = true;

    public IEnumerator InitializeBattleScene()
    {
        LodingSetting();

        // 로드 단계 리스트 구성
        List<IEnumerator> initSteps = new()
    {
        LoadArmyData(),
        CombatTypeCheck(),
        LoadEvent(),
        LoadMapSetting(),
        SetupCamera()
    };

        int stepCount = initSteps.Count;
        for (int i = 0; i < stepCount; i++)
        {
            yield return StartCoroutine(initSteps[i]);

            float progress = (i + 1f) / stepCount;
            UpdateLoadingBar(progress);
        }

        yield return new WaitForSeconds(0.5f);
        loadingPanel.SetActive(false);
    }

    private void LodingSetting()
    {
        //로딩 화면 세팅
        loadingPanel.SetActive(true);
        loadingBar = loadingPanel.transform.Find("LoadingBar").GetComponent<Image>();
        UpdateLoadingBar(0.0f);
    }


    private void UpdateLoadingBar(float value)
    {
        if (loadingBar != null)
        {
            loadingBar.fillAmount = Mathf.Clamp01(value);
        }
    }

    private IEnumerator LoadArmyData()
    {
        Debug.Log("병력 로딩 중...");
        // 이전 데이터 초기화
        allyArmyDataList.Clear();
        enemyArmyDataList.Clear();

        //아군 병력 로딩
        foreach (var unit in allyArmyData.units)
        {
            allyArmyDataList.Add(unit);
            Debug.Log($"아군 유닛 이름: {unit.unitName}");
        }

        //적군 병력 로딩
        foreach (var unit in enemyArmyData.units)
        {
            enemyArmyDataList.Add(unit);
            Debug.Log($"적군 유닛 이름: {unit.unitName}");
        }

        yield return new WaitForSeconds(0.5f);
    }

    //구현 필요
    private IEnumerator CombatTypeCheck()
    {
        Debug.Log("전투타입 확인 중...");
        yield return new WaitForSeconds(0.5f);
    }

    //활성화된 이벤트 확인
    private IEnumerator LoadEvent()
    {
        Debug.Log("이벤트 확인 중...");

        if (m_gameDataManager == null || m_gameDataManager.EventEntry == null)
        {
            Debug.LogWarning("[LoadEvent] GameDataManager 또는 EventEntry가 null입니다.");
            yield break;
        }

        var activeEventList = m_gameDataManager.EventEntry.m_state.m_activeEventList;

        if (activeEventList.Count == 0)
        {
            Debug.Log("현재 활성화된 이벤트가 없습니다.");
        }
        else
        {
            Debug.Log($"현재 활성화된 이벤트 수: {activeEventList.Count}");
            foreach (var evt in activeEventList)
            {
                Debug.Log($"- 이벤트 이름: {evt.m_eventData}, 남은 지속일: {evt.m_remainingDuration}");
            }
        }

        yield return new WaitForSeconds(0.5f);
    }


    private IEnumerator LoadMapSetting()
    {
        Debug.Log("맵 설정 중...");
        //배경세팅
        battleField = Instantiate(battleField);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator SetupCamera()
    {
        Debug.Log("카메라 설정 중...");

        //카메라 위치 찾음
        mainCamra = GameObject.Find("Main Camera").GetComponent<Camera>();
        defenseCameraPoint = GameObject.Find("DefenseCameraPoint").transform.position;
        attackCameraPoint = GameObject.Find("AttackCameraPoint").transform.position;

        if (isAttack == true)
        {
            //공격인 경우 카메라 설정
            mainCamra.transform.position = attackCameraPoint;
        }
        else
        {
            //방어인 경우 카메라 설정
            mainCamra.transform.position = defenseCameraPoint;
        }

        yield return new WaitForSeconds(0.5f);
    }
}
