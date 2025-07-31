using System.Collections;
using System.Collections.Generic;
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

    [Header("���� ������ �Ŵ���")]
    [SerializeField] private GameDataManager m_gameDataManager = null;

    [Header("���� ������")]
    [SerializeField] private AllyArmyData allyArmyData = null;
    [SerializeField] private EnemyArmyData enemyArmyData = null;

    private List<UnitStatBase> allyArmyDataList = new();
    private List<UnitStatBase> enemyArmyDataList = new();


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
    }

    private void LodingSetting()
    {
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
        yield return new WaitForSeconds(0.5f);
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
}
