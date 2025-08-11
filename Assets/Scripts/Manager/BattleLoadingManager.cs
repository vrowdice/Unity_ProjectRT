using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleLoadingManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("�ε� UI")]
    public GameObject loadingPanel;
    private Image loadingBar;

    [Header("���� �ʵ�")]
    public GameObject battleField;

    [HideInInspector] public Vector3 defenseCameraPoint;
    [HideInInspector] public Vector3 attackCameraPoint;
    [HideInInspector] public Camera mainCamra;
    private Transform allySpawnArea;
    private Transform enemySpawnArea;
    private Vector3 spawnAreaSize;
    public GameObject battleBeforeUI;

    [Header("���� ������ �Ŵ���")]
    [SerializeField] private GameDataManager m_gameDataManager = null;


    [Header("���� ������")]
    [SerializeField] private AllyArmyData allyArmyData = null;
    [SerializeField] private EnemyArmyData enemyArmyData = null;


    [HideInInspector] public List<UnitStatBase> allyArmyDataList = new();
    private List<UnitStatBase> enemyArmyDataList = new();

    [Header("���� ��ġ UI")]
    [SerializeField] private GameObject DeploymentUI;

    //�׽�Ʈ�� �Ŀ� ���� ������ �����ؾ���
    private bool isAttack = true;


    public IEnumerator InitializeBattleScene()
    {
        LodingSetting();

        // �ε� �ܰ� ����Ʈ ����
        List<IEnumerator> initSteps = new()
    {
        LoadUnitData(),
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
        BattleManager battleManager = FindObjectOfType<BattleManager>();
        battleManager.isSettingClear = true;
    }

    private void LodingSetting()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas�� ã�� �� �����ϴ�!");
            return;
        }

        loadingPanel = Instantiate(loadingPanel, canvas.transform);

        loadingPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        loadingPanel.SetActive(true);

        //�ε� ȭ�� ����
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

    private IEnumerator LoadUnitData()
    {
        Debug.Log("���� �ε� ��...");

        // ���� ������ �ʱ�ȭ
        allyArmyDataList.Clear();
        enemyArmyDataList.Clear();

        //�Ʊ� ���� �ε�
        foreach (var unit in allyArmyData.units)
        {
            allyArmyDataList.Add(unit);
            Debug.Log($"�Ʊ� ���� �̸�: {unit.unitName}");
        }

        //���� ���� �ε�
        foreach (var unit in enemyArmyData.units)
        {
            enemyArmyDataList.Add(unit);
            Debug.Log($"���� ���� �̸�: {unit.unitName}");
        }

        yield return new WaitForSeconds(0.5f);
    }

    //���� �ʿ�
    private IEnumerator CombatTypeCheck()
    {
        Debug.Log("����Ÿ�� Ȯ�� ��...");
        yield return new WaitForSeconds(0.5f);
    }

    //Ȱ��ȭ�� �̺�Ʈ Ȯ��
    private IEnumerator LoadEvent()
    {
        Debug.Log("�̺�Ʈ Ȯ�� ��...");

        if (m_gameDataManager == null || m_gameDataManager.EventState == null)
        {
            Debug.LogWarning("[LoadEvent] GameDataManager �Ǵ� EventEntry�� null�Դϴ�.");
            yield break;
        }

        var activeEventList = m_gameDataManager.EventState.m_activeEventList;

        if (activeEventList.Count == 0)
        {
            Debug.Log("���� Ȱ��ȭ�� �̺�Ʈ�� �����ϴ�.");
        }
        else
        {
            Debug.Log($"���� Ȱ��ȭ�� �̺�Ʈ ��: {activeEventList.Count}");
            foreach (var evt in activeEventList)
            {
                Debug.Log($"- �̺�Ʈ �̸�: {evt.m_eventData}, ���� ������: {evt.m_remainingDuration}");
            }
        }

        yield return new WaitForSeconds(0.5f);
    }


    private IEnumerator LoadMapSetting()
    {
        Debug.Log("�� ���� ��...");

        // ��� ����
        battleField = Instantiate(battleField);
        allySpawnArea = GameObject.Find("AttackSpawnArea").transform;
        enemySpawnArea = GameObject.Find("DefenseSpawnArea").transform;

        Transform mySpawnArea = isAttack ? enemySpawnArea : allySpawnArea;
        spawnAreaSize = mySpawnArea.GetComponent<BoxCollider2D>().size;

        foreach (UnitStatBase unitData in enemyArmyDataList)
        {
            if (unitData.prefab == null)
            {
                Debug.LogWarning($"{unitData.unitName}�� �������� �������� �ʾҽ��ϴ�.");
                continue;
            }

            Vector3 spawnPos = GetRandomPositionInArea(mySpawnArea, spawnAreaSize);
            GameObject enemyUnit = Instantiate(unitData.prefab, spawnPos, Quaternion.identity);

            UnitBase unitBase = enemyUnit.GetComponent<UnitBase>();
            if (unitBase != null)
            {
                unitBase.Initialize(unitData);
            }

            if (unitBase.unitType == UnitType.Melee)
            {
                enemyUnit.tag = "ShortUnit";
            }

            else if (unitBase.unitType == UnitType.Range)
            {
                enemyUnit.tag = "LongUnit";
            }
            else if (unitBase.unitType == UnitType.Defense)
            {
                enemyUnit.tag = "DefenseUnit";
            }
            else
            {
                Debug.LogWarning($"{unitData.unitName}�� �±� ������ �����ϴ�.");
            }

            // ��������Ʈ ����
            SpriteRenderer sr = enemyUnit.GetComponent<SpriteRenderer>();
            if (sr != null && unitData.unitIllustration != null)
            {
                // [����] Sprite ���¿� .sprite�� ������� �ʰ� ���� �Ҵ�
                sr.sprite = unitData.unitIllustration;
            }
            else
            {
                Debug.LogWarning($"{unitData.unitName}�� SpriteRenderer�� ���ų� unitIllustration�� ����ֽ��ϴ�.");
            }
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
        Debug.Log("ī�޶� ���� ��...");

        //ī�޶� ��ġ ã��
        mainCamra = GameObject.Find("Main Camera").GetComponent<Camera>();
        defenseCameraPoint = GameObject.Find("DefenseCameraPoint").transform.position;
        attackCameraPoint = GameObject.Find("AttackCameraPoint").transform.position;

        if (isAttack == true)
        {
            //������ ��� ī�޶� ����
            mainCamra.transform.position = attackCameraPoint;
        }
        else
        {
            //����� ��� ī�޶� ����
            mainCamra.transform.position = defenseCameraPoint;
        }
        yield return new WaitForSeconds(0.5f);
    }

    void CreateDeploymentUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas�� ã�� �� �����ϴ�!");
            return;
        }

        battleBeforeUI = Instantiate(DeploymentUI, canvas.transform);

        battleBeforeUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        battleBeforeUI.SetActive(true);
    }
}
