using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    private Vector3 defenseCameraPoint;
    private Vector3 attackCameraPoint;
    private Camera mainCamra;
    private Transform allySpawnArea;
    private Transform enemySpawnArea;
    private Vector3 spawnAreaSize;

    [Header("���� ������ �Ŵ���")]
    [SerializeField] private GameDataManager m_gameDataManager = null;

    [Header("���� ������")]
    [SerializeField] private AllyArmyData allyArmyData = null;
    [SerializeField] private EnemyArmyData enemyArmyData = null;

    private List<UnitStatBase> allyArmyDataList = new();
    private List<UnitStatBase> enemyArmyDataList = new();

    [SerializeField] private GameObject DeploymentUI;

    [Header("���� ���� UI")]
    [SerializeField] private GameObject armyBox; // ���� ���� UI ������
    private Transform contentParent;

    //�׽�Ʈ�� �Ŀ� ���� ������ �����ؾ���
    private bool isAttack = true;

    public IEnumerator InitializeBattleScene()
    {
        LodingSetting();

        // �ε� �ܰ� ����Ʈ ����
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

    private IEnumerator LoadArmyData()
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

        if (m_gameDataManager == null || m_gameDataManager.EventEntry == null)
        {
            Debug.LogWarning("[LoadEvent] GameDataManager �Ǵ� EventEntry�� null�Դϴ�.");
            yield break;
        }

        var activeEventList = m_gameDataManager.EventEntry.m_state.m_activeEventList;

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
        //��漼��
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

        GameObject deploymentUI = Instantiate(DeploymentUI, canvas.transform);

        deploymentUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        deploymentUI.SetActive(true);
        GenerateList();
    }

    private void GenerateList()
    {
        contentParent = GameObject.Find("Content").GetComponent<Transform>();

        // ���� �ڽ� ����
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // ���� ������ ��� ������ ����
        foreach (var unit in allyArmyDataList)
        {
            GameObject myUnit = Instantiate(armyBox, contentParent);
            myUnit.transform.localScale = Vector3.one;

            // ���� ������ ����
            Image icon = myUnit.transform.Find("ArmyImage").GetComponent<Image>();
            if (icon != null && unit.unitIllustration != null)
                icon = unit.unitIllustration;

            // ���� �̸� �ؽ�Ʈ ����
            TextMeshProUGUI nameText = myUnit.transform.Find("AmryTexts").transform.Find("AmryNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                Debug.Log($"unitName: {unit.unitName}");
                nameText.text = unit.unitName;
            }

            //// ��ư Ŭ�� �̺�Ʈ ���� (�ʿ� ��)
            //Button btn = myUnit.GetComponent<Button>();
            //if (btn != null)
            //{
            //    UnitStatBase capturedUnit = unit;
            //    btn.onClick.AddListener(() =>
            //    {
            //        Debug.Log($"Ŭ���� ����: {capturedUnit.unitName}");
            //        // ���� ���� ó�� �� �߰� ����
            //    });
            //}
        }
    }
}
