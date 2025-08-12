using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    [Header("UI 프리팹 (씬 오브젝트도 가능)")]
    [SerializeField] private GameObject allyUI;
    [SerializeField] private GameObject enemyUI;

    private BattleBeforeUI allyUIInstance;
    private GameObject enemyUIInstance; // 구조가 다를 수 있으니 GameObject로 관리
    private BattleBeforeUI enemyUIBattleScript; // 있으면 참조, 없으면 null

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 전투 UI 초기화 (아군 UI만 생성)
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
    /// 아군 UI로 전환
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
    /// 적군 UI로 전환
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
            enemyUIInstance.SetActive(false); // 생성 직후 숨김
        }

        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);
        if (enemyUIInstance)
        {
            enemyUIInstance.SetActive(true);
            enemyUIBattleScript?.UpdateDeployedUnitCounters(); // 스크립트 있을 때만 호출
        }
    }

    /// <summary>
    /// 모든 UI 숨김
    /// </summary>
    public void HideAllUI()
    {
        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);
        if (enemyUIInstance) enemyUIInstance.SetActive(false);
    }
}
