using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    [Header("UI ������ (�� ������Ʈ�� ����)")]
    [SerializeField] private GameObject allyUI;
    [SerializeField] private GameObject enemyUI;

    private BattleBeforeUI allyUIInstance;
    private GameObject enemyUIInstance; // ������ �ٸ� �� ������ GameObject�� ����
    private BattleBeforeUI enemyUIBattleScript; // ������ ����, ������ null

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// ���� UI �ʱ�ȭ (�Ʊ� UI�� ����)
    /// </summary>
    public void InitializeUI(List<UnitStatBase> allyUnitStats)
    {
        if (allyUI)
        {
            var go = Instantiate(allyUI, transform);
            allyUIInstance = go.GetComponent<BattleBeforeUI>();
            if (allyUIInstance)
            {
                allyUIInstance.InitDeploymentUI(allyUnitStats);
            }
            go.SetActive(true);
        }
    }

    /// <summary>
    /// �Ʊ� UI�� ��ȯ
    /// </summary>
    public void SwitchToAllyUI()
    {
        if (enemyUIInstance) enemyUIInstance.SetActive(false);
        if (allyUIInstance)
        {
            allyUIInstance.gameObject.SetActive(true);
            allyUIInstance.UpdateDeployedUnitCounters();
        }
    }

    /// <summary>
    /// ���� UI�� ��ȯ
    /// </summary>
    public void SwitchToEnemyUI()
    {
        if (!enemyUIInstance && enemyUI)
        {
            enemyUIInstance = Instantiate(enemyUI, transform);
            enemyUIBattleScript = enemyUIInstance.GetComponent<BattleBeforeUI>();
            if (!enemyUIBattleScript)
            {
                enemyUIBattleScript = enemyUIInstance.GetComponentInChildren<BattleBeforeUI>();
            }
            enemyUIInstance.SetActive(false); // ���� ���� ����
        }

        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);
        if (enemyUIInstance)
        {
            enemyUIInstance.SetActive(true);
            enemyUIBattleScript?.UpdateDeployedUnitCounters(); // ��ũ��Ʈ ���� ���� ȣ��
        }
    }

    /// <summary>
    /// ��� UI ����
    /// </summary>
    public void HideAllUI()
    {
        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);
        if (enemyUIInstance) enemyUIInstance.SetActive(false);
    }
}
