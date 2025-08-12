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

    [Header("�ܺ�")]
    [SerializeField] private BattleSystemManager battleSystemManager;

    [Header("������(SO)")]
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

        // �� ������ BattleSystemManager���� �̹� ó���ϹǷ�,
        // BattleLoadingManager�� ������ �ʱ� �ε� UI ǥ�ÿ� ������ ��� �Ʒ� �ڵ�� �ּ� ó���ϰų� ���� �����մϴ�.
        // ����� BattleSystemManager���� ��� �ʱ�ȭ ������ ����ϹǷ�, �� ��ũ��Ʈ ��ü�� �ʿ伺�� ���� �� �ֽ��ϴ�.
        // yield return StartCoroutine(LoadAndSpawnEnemyUnitsViaManager());

        if (loadingText) loadingText.text = "Finish!";
        yield return new WaitForSeconds(0.2f);
        loadingPanel?.SetActive(false);
    }

    private IEnumerator LoadAndSpawnEnemyUnitsViaManager()
    {
        if (!battleSystemManager)
        {
            Debug.LogError("[BattleLoadingManager] BattleSystemManager is NULL. ��ġ ��ŵ");
            yield break;
        }

        var enemyArea = battleSystemManager.GetEnemySpawnArea();
        if (!enemyArea)
        {
            Debug.LogError("[BattleLoadingManager] EnemySpawnArea is NULL (from manager). ��ġ ��ŵ");
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