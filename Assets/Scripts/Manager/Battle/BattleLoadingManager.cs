using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class BattleLoadingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("외부")]
    [SerializeField] private BattleSystemManager battleSystemManager;

    [Header("데이터(SO)")]
    [SerializeField] private AllyArmyData allyArmyData;
    [SerializeField] private EnemyArmyData enemyArmyData;

    private void Awake()
    {
        if (!battleSystemManager) battleSystemManager = FindObjectOfType<BattleSystemManager>();
    }

    private void Start()
    {
        StartCoroutine(InitializeBattleScene());
    }

    public IEnumerator InitializeBattleScene()
    {
        loadingPanel?.SetActive(true);
        if (progressBar) progressBar.fillAmount = 0.0f;
        if (loadingText) loadingText.text = "Checking allies..";

        // 이 로직은 BattleSystemManager에서 이미 처리하므로,
        // BattleLoadingManager의 역할이 초기 로딩 UI 표시에 한정될 경우 아래 코드는 주석 처리하거나 삭제 가능합니다.
        // 현재는 BattleSystemManager에서 모든 초기화 로직을 담당하므로, 이 스크립트 자체의 필요성이 낮을 수 있습니다.
        // yield return StartCoroutine(LoadAndSpawnEnemyUnitsViaManager());

        if (loadingText) loadingText.text = "Finish!";
        yield return new WaitForSeconds(0.2f);
        loadingPanel?.SetActive(false);
    }

    private IEnumerator LoadAndSpawnEnemyUnitsViaManager()
    {
        if (!battleSystemManager)
        {
            Debug.LogError("[BattleLoadingManager] BattleSystemManager is NULL. 배치 스킵");
            yield break;
        }

        var enemyArea = battleSystemManager.GetEnemySpawnArea();
        if (!enemyArea)
        {
            Debug.LogError("[BattleLoadingManager] EnemySpawnArea is NULL (from manager). 배치 스킵");
            yield break;
        }

        int total = enemyArmyData ? enemyArmyData.units.Count : 0;
        int i = 0;

        if (loadingText) loadingText.text = "preparing...";
        if (progressBar) progressBar.fillAmount = 0f;

        if (enemyArmyData)
        {
            foreach (var stat in enemyArmyData.units)
            {
                if (stat?.prefab)
                {
                    Vector3 pos = GetRandomPointInCollider(enemyArea);
                    var go = Instantiate(stat.prefab, pos, Quaternion.identity);
                    var ub = go.GetComponent<UnitBase>();
                    if (ub)
                    {
                        ub.Initialize(stat);
                        battleSystemManager.AddEnemyUnit(ub);
                    }
                }
                i++;
                if (progressBar) progressBar.fillAmount = (float)i / Mathf.Max(1, total);
                yield return null;
            }
        }

        if (loadingText) loadingText.text = "Finish!";
        if (progressBar) progressBar.fillAmount = 1.0f;
    }

    private Vector3 GetRandomPointInCollider(BoxCollider2D col)
    {
        if (!col) { Debug.LogError("[BattleLoadingManager] Collider null"); return Vector3.zero; }
        var b = col.bounds;
        return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0f);
    }
}