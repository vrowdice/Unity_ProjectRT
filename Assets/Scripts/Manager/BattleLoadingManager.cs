using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private Transform allySpawnArea;
    private Transform enemySpawnArea;
    private Vector3 spawnAreaSize;

    [Header("게임 데이터 매니저")]
    [SerializeField] private GameDataManager m_gameDataManager = null;

    [Header("유닛 데이터")]
    [SerializeField] private AllyArmyData allyArmyData = null;
    [SerializeField] private EnemyArmyData enemyArmyData = null;

    private List<UnitStatBase> allyArmyDataList = new();
    private List<UnitStatBase> enemyArmyDataList = new();

    [SerializeField] private GameObject DeploymentUI;

    [Header("유닛 슬롯 UI")]
    [SerializeField] private GameObject armyBox; // 병력 슬롯 UI 프리팹
    private Transform contentParent;

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
        CreateDeploymentUI();
    }

    private void LodingSetting()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다!");
            return;
        }

        loadingPanel = Instantiate(loadingPanel, canvas.transform);

        loadingPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        loadingPanel.SetActive(true);

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
        allySpawnArea = GameObject.Find("AttackSpawnArea").transform;
        enemySpawnArea = GameObject.Find("DefenseSpawnArea").transform;

        Transform mySpawnArea = isAttack ? enemySpawnArea : allySpawnArea;
        spawnAreaSize = mySpawnArea.GetComponent<BoxCollider2D>().size;

        foreach (UnitStatBase unitData in enemyArmyDataList)
        {
            if (unitData.prefab == null)
            {
                Debug.LogWarning($"{unitData.unitName}의 프리팹이 설정되지 않았습니다.");
                continue;
            }

            GameObject test = Instantiate(unitData.prefab, GetRandomPositionInArea(mySpawnArea, spawnAreaSize), Quaternion.identity);
            test.GetComponent<UnitBase>().Initialize(unitData);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private Vector3 GetRandomPositionInArea(Transform area, Vector2 size)
    {
        Vector2 offset = new Vector2(
            Random.Range(-size.x / 2f, size.x / 2f),
            Random.Range(-size.y / 2f, size.y / 2f)
        );

        return area.position + (Vector3)offset;
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

    void CreateDeploymentUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다!");
            return;
        }

        GameObject deploymentUI = Instantiate(DeploymentUI, canvas.transform);

        deploymentUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        deploymentUI.SetActive(true);
        GenerateList();
    }

    private void GenerateList()
    {
        contentParent = GameObject.Find("Content").GetComponent<Transform>();

        // 기존 자식 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 유닛 데이터 기반 프리팹 생성
        foreach (var unit in allyArmyDataList)
        {
            GameObject myUnit = Instantiate(armyBox, contentParent);
            myUnit.transform.localScale = Vector3.one;

            // 유닛 아이콘 설정
            Image icon = myUnit.transform.Find("ArmyImage").GetComponent<Image>();
            if (icon != null && unit.unitIllustration != null)
                icon = unit.unitIllustration;

            // 유닛 이름 텍스트 설정
            TextMeshProUGUI nameText = myUnit.transform.Find("AmryTexts").transform.Find("AmryNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                Debug.Log($"unitName: {unit.unitName}");
                nameText.text = unit.unitName;
            }

            //// 버튼 클릭 이벤트 설정 (필요 시)
            //Button btn = myUnit.GetComponent<Button>();
            //if (btn != null)
            //{
            //    UnitStatBase capturedUnit = unit;
            //    btn.onClick.AddListener(() =>
            //    {
            //        Debug.Log($"클릭한 유닛: {capturedUnit.unitName}");
            //        // 유닛 선택 처리 등 추가 가능
            //    });
            //}
        }
    }
}
