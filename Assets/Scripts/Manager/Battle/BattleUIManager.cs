using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    [Header("UI 프리팹")]
    [SerializeField] private GameObject allyUI;
    [SerializeField] private GameObject enemyUI;

    private BattleBeforeUI allyUIInstance;
    private GameObject enemyUIInstance;
    private BattleBeforeUI enemyUIBattleScript;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// 아군 배치 UI 초기화
    public void InitializeUI(List<UnitStatBase> allyUnitStats)
    {
        if (allyUI && !allyUIInstance)
        {
            var go = Instantiate(allyUI, transform);
            allyUIInstance = go.GetComponent<BattleBeforeUI>();
            if (allyUIInstance) allyUIInstance.InitDeploymentUI(allyUnitStats);
            go.SetActive(true);
        }
        else if (allyUIInstance)
        {
            allyUIInstance.InitDeploymentUI(allyUnitStats);
            allyUIInstance.gameObject.SetActive(true);
        }
    }

    public void SwitchToAllyUI()
    {
        if (enemyUIInstance) enemyUIInstance.SetActive(false);
        if (enemyUIInstance && enemyUIBattleScript) enemyUIBattleScript.gameObject.SetActive(false);

        if (allyUIInstance)
        {
            allyUIInstance.gameObject.SetActive(true);
            allyUIInstance.UpdateDeployedUnitCounters();
        }
    }

    public void SwitchToEnemyUI()
    {
        if (!enemyUIInstance && enemyUI)
        {
            enemyUIInstance = Instantiate(enemyUI, transform);
            enemyUIBattleScript = enemyUIInstance.GetComponent<BattleBeforeUI>() ??
                                  enemyUIInstance.GetComponentInChildren<BattleBeforeUI>();
            enemyUIInstance.SetActive(false); // 생성 직후 숨김
        }

        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);

        if (enemyUIInstance)
        {
            enemyUIInstance.SetActive(true);
            enemyUIBattleScript?.UpdateDeployedUnitCounters();
        }
    }

    public void HideAllUI()
    {
        if (allyUIInstance) allyUIInstance.gameObject.SetActive(false);
        if (enemyUIInstance) enemyUIInstance.SetActive(false);
    }
}
