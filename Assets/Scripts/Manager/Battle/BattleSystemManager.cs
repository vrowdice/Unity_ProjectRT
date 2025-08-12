using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class BattleSystemManager : MonoBehaviour
{
    public static BattleSystemManager Instance { get; private set; }

    [Header("�ε� UI (�ɼ�)")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("���� ���� (�� ������Ʈ)")]
    [SerializeField] private BoxCollider2D attackSpawnArea;
    [SerializeField] private BoxCollider2D defenseSpawnArea;

    [Header("ī�޶� ����Ʈ (�� ������Ʈ)")]
    [SerializeField] private Transform attackCameraPoint;
    [SerializeField] private Transform defenseCameraPoint;
    [SerializeField] private Camera mainCamera;

    [Header("���� ������ (SO)")]
    [SerializeField] private AllyArmyData allyArmyData;
    [SerializeField] private EnemyArmyData enemyArmyData;

    [Header("�� ���� ��ġ (�ɼ�)")]
    [Tooltip("�����̳ʰ� ���� �̸� ��ġ�� �� ���ֵ��� �θ� Transform")]
    [SerializeField] private Transform enemyUnitHolder;
    [SerializeField] private bool useManualEnemyPlacement = true;

    [Header("StartBattle �� ���ġ �ɼ�")]
    [SerializeField] private bool repositionAlliesOnStart = false;
    [SerializeField] private bool repositionEnemiesOnStart = false;

    [Header("��� UI")]
    [SerializeField] private GameObject resultUIPrefab;   
    [SerializeField] private Canvas targetCanvas;       

    [Header("���� �� ī�޶� �巡��")]
    [SerializeField] private CameraDragPan camPan;     
    [SerializeField] private BoxCollider2D mapBounds;    

    // ��Ÿ�� ����
    public List<UnitBase> allyUnits = new();
    public List<UnitBase> enemyUnits = new();

    private bool isPlayerAttacker = true;

    public List<UnitBase> AllyUnits => allyUnits;
    public List<UnitBase> EnemyUnits => enemyUnits;

    public Transform AttackCameraPoint => attackCameraPoint;
    public Transform DefenseCameraPoint => defenseCameraPoint;
    public bool IsPlayerAttacker => isPlayerAttacker;

    public bool IsBattleRunning { get; private set; } = false;
    public bool IsViewingAllyBase { get; private set; } = true;

    public System.Action UnitsChanged;
    private void NotifyUnitsChanged() => UnitsChanged?.Invoke();

    private BattleRuntimeManager runtime;

    private Transform leftBoundTmp;
    private Transform rightBoundTmp;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!mainCamera) mainCamera = Camera.main;
    }

    private void Start()
    {
        StartCoroutine(InitializeBattleScene());
    }

    private IEnumerator InitializeBattleScene()
    {
        if (loadingPanel) loadingPanel.SetActive(true);
        if (progressBar) progressBar.fillAmount = 0.0f;
        if (loadingText) loadingText.text = "Preparing";

        // �� ����
        yield return StartCoroutine(SpawnEnemyUnits());

        // ��ġ UI ����/�ʱ�ȭ 
        BattleUIManager.Instance?.InitializeUI(allyArmyData?.units);

        if (loadingText) loadingText.text = "Finish!";
        yield return new WaitForSeconds(0.2f);
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    public Transform GetAllyCameraPointByState()
    {
        return isPlayerAttacker ? attackCameraPoint : defenseCameraPoint;
    }

    public Transform GetEnemyCameraPointByState()
    {
        return isPlayerAttacker ? defenseCameraPoint : attackCameraPoint;
    }

    public void ToggleView()
    {
        IsViewingAllyBase = !IsViewingAllyBase;

        if (IsViewingAllyBase)
        {
            BattleCameraManager.Instance?.MoveToAllyPosition();
            BattleUIManager.Instance?.SwitchToAllyUI();
        }
        else
        {
            BattleCameraManager.Instance?.MoveToEnemyPosition();
            BattleUIManager.Instance?.SwitchToEnemyUI();
        }

        RefreshDragEnabled();

 
        FindObjectOfType<BattleBeforeUI>(true)?.UpdateDeployedUnitCounters();
    }

    public void RefreshDragEnabled()
    {
        bool allowDrag = IsViewingAllyBase && !IsBattleRunning;
        foreach (var u in allyUnits)
            u?.GetComponent<UnitDragHandler>()?.EnableDrag(allowDrag);
    }

    // ���� ����
    public void StartBattle()
    {
        Debug.Log("[BattleSystemManager] StartBattle");

        // ��ġ UI ����
        BattleUIManager.Instance?.HideAllUI();

        // ���� ���� �� ���ġ
        if (repositionAlliesOnStart) SpawnUnits(allyUnits, GetAllySpawnArea());
        if (repositionEnemiesOnStart) SpawnUnits(enemyUnits, GetEnemySpawnArea());

        // ���� �巡�� ��Ȱ��
        IsBattleRunning = true;
        RefreshDragEnabled();

        // ���� ��Ÿ�� ����
        runtime = gameObject.AddComponent<BattleRuntimeManager>();
        runtime.OnBattleFinished += HandleBattleFinished;
        runtime.Begin(allyUnits, enemyUnits, IsPlayerAttacker);

        if (camPan)
        {
            EnsurePanBoundsTransforms();
            ApplyPanBoundsFromMap(); 
            camPan.Enable(true);
        }

    }

    private void HandleBattleFinished(BattleOutcome outcome)
    {
        IsBattleRunning = false;

        // �巡�� ����
        if (camPan) camPan.Enable(false);

        ShowResult(outcome);
    }

    private void ShowResult(BattleOutcome outcome)
    {
        if (resultUIPrefab && targetCanvas)
        {
            var go = Instantiate(resultUIPrefab, targetCanvas.transform);
            var ui = go.GetComponent<BattleResultUI>();
            if (ui != null)
            {
                ui.Show(outcome);

                ui.OnContinue = () =>
                {
                    Destroy(go);
                    Debug.Log("Continue pressed");
                };

                ui.OnRetry = () =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                };
            }
            else
            {
                Debug.LogWarning("BattleResultUI ������Ʈ�� �����տ��� ã�� ���߽��ϴ�.");
            }
        }
        else
        {
            Debug.Log(
                $"Result => {(outcome.victory ? "VICTORY" : "DEFEAT")} | " +
                $"Allies:{outcome.allyRemaining} Enemies:{outcome.enemyRemaining} " +
                $"Time:{outcome.elapsed:0.0}s"
            );
        }
    }

    private IEnumerator SpawnEnemyUnits()
    {
        enemyUnits.Clear();

        if (useManualEnemyPlacement && enemyUnitHolder)
        {
            if (loadingText) loadingText.text = "���� ���� ��ġ ���� ��...";
            foreach (Transform child in enemyUnitHolder)
            {
                var ub = child.GetComponent<UnitBase>();
                if (!ub) continue;
                if (ub.UnitStat == null)
                {
                    Debug.LogError($"[BattleSystemManager] ���� ��ġ ���� '{ub.name}'�� UnitStat�� null�Դϴ�. " +
                                   $"������ ���ֿ� ����(SO)�� �Ҵ��ϰų� �ڵ�� Initialize(stat)�� ȣ���ϼ���.");
                    continue;
                }
                AddEnemyUnit(ub);
            }
            yield break;
        }

        if (!enemyArmyData)
        {
            Debug.LogWarning("[BattleSystemManager] enemyArmyData ���Ҵ�");
            yield break;
        }

        if (loadingText) loadingText.text = "���� ���� ��ġ ��...";
        int total = enemyArmyData.units.Count;
        int i = 0;

        var enemyArea = GetEnemySpawnArea();
        if (!enemyArea)
        {
            Debug.LogError("[BattleSystemManager] �� ���� ������ ã�� �� ����");
            yield break;
        }

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
                    AddEnemyUnit(ub);
                }
            }
            i++;
            if (progressBar) progressBar.fillAmount = (float)i / Mathf.Max(1, total);
            yield return null;
        }
    }

    public UnitBase RequestSpawnAlly(UnitStatBase stat)
    {
        if (!stat) { Debug.LogWarning("[BattleSystemManager] RequestSpawnAlly: stat null"); return null; }
        if (!stat.prefab) { Debug.LogWarning($"[BattleSystemManager] {stat.unitName} prefab null"); return null; }

        var allyArea = GetAllySpawnArea();
        if (!allyArea)
        {
            Debug.LogWarning("[BattleSystemManager] �Ʊ� ���� ���� null");
            return null;
        }

        Vector3 pos = GetRandomPointInCollider(allyArea);
        var go = Instantiate(stat.prefab, pos, Quaternion.identity);
        var ub = go.GetComponent<UnitBase>();
        if (ub)
        {
            ub.Initialize(stat);
            RegisterUnit(ub);
            return ub;
        }
        Destroy(go);
        return null;
    }

    public bool RecallAlly(UnitBase unit)
    {
        if (unit)
        {
            allyUnits.Remove(unit);
            Destroy(unit.gameObject);
            NotifyUnitsChanged();
            return true;
        }
        return false;
    }

    public void AddEnemyUnit(UnitBase unit)
    {
        if (unit && !enemyUnits.Contains(unit))
        {
            enemyUnits.Add(unit);
            NotifyUnitsChanged();
        }
    }


    public BoxCollider2D GetEnemySpawnArea()
    {
        return isPlayerAttacker ? defenseSpawnArea : attackSpawnArea;
    }

    public BoxCollider2D GetAllySpawnArea()
    {
        return isPlayerAttacker ? attackSpawnArea : defenseSpawnArea;
    }

    // === ���� ��ƿ ===
    private void SpawnUnits(List<UnitBase> list, BoxCollider2D area)
    {
        if (!area) return;
        foreach (var u in list)
        {
            if (!u) continue;
            u.transform.position = GetRandomPointInCollider(area);
        }
    }

    private Vector3 GetRandomPointInCollider(BoxCollider2D col)
    {
        if (!col) { Debug.LogError("[BattleSystemManager] Collider null"); return Vector3.zero; }
        var b = col.bounds;
        return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0f);
    }

    public void RegisterUnit(UnitBase unit)
    {
        if (!unit) return;
        if (unit.Faction == FactionType.TYPE.Owl) allyUnits.Add(unit);
        else enemyUnits.Add(unit);
        NotifyUnitsChanged();
    }

    public void UnregisterUnit(UnitBase unit)
    {
        if (!unit) return;
        if (unit.Faction == FactionType.TYPE.Owl) allyUnits.Remove(unit);
        else enemyUnits.Remove(unit);
        NotifyUnitsChanged();
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (allyUnits.Count == 0) Debug.Log("[BattleSystemManager] �й� �Ʊ� ����");
        else if (enemyUnits.Count == 0) Debug.Log("[BattleSystemManager] �¸� ���� ����");
    }

    public Dictionary<UnitTagType, int> GetCountsInSpawnAreas()
    {
        var counts = new Dictionary<UnitTagType, int>
        {
            { UnitTagType.Melee, 0 },
            { UnitTagType.Range, 0 },
            { UnitTagType.Defense, 0 }
        };
        CountUnitsInAreaByList(GetAllySpawnArea(), counts, allyUnits);
        return counts;
    }

    public Dictionary<UnitTagType, int> GetEnemyCountsInSpawnAreas()
    {
        var counts = new Dictionary<UnitTagType, int>
        {
            { UnitTagType.Melee, 0 },
            { UnitTagType.Range, 0 },
            { UnitTagType.Defense, 0 }
        };
        CountUnitsInAreaByList(GetEnemySpawnArea(), counts, enemyUnits);
        return counts;
    }

    private void CountUnitsInAreaByList(BoxCollider2D area, Dictionary<UnitTagType, int> counts, List<UnitBase> list)
    {
        if (area == null) { Debug.LogError("ī��Ʈ�� �� ���� ������ null"); return; }

        var b = area.bounds;
        foreach (var unit in list)
        {
            if (!unit || unit.UnitStat == null) continue;
            if (b.Contains(unit.transform.position))
            {
                var tag = unit.UnitStat.unitTagType;
                if (counts.ContainsKey(tag)) counts[tag]++;
            }
        }
    }

    private void EnsurePanBoundsTransforms()
    {
        if (!leftBoundTmp)
        {
            var go = new GameObject("[PanBound_L]");
            go.transform.SetParent(transform);
            leftBoundTmp = go.transform;
        }
        if (!rightBoundTmp)
        {
            var go = new GameObject("[PanBound_R]");
            go.transform.SetParent(transform);
            rightBoundTmp = go.transform;
        }
    }

    private void ApplyPanBoundsFromMap()
    {
        if (!camPan) return;

        if (mapBounds)
        {
            var b = mapBounds.bounds;
            var cy = mainCamera ? mainCamera.transform.position.y : 0.0f;
            var cz = mainCamera ? mainCamera.transform.position.z : -10.0f;

            leftBoundTmp.position = new Vector3(b.min.x, cy, cz);
            rightBoundTmp.position = new Vector3(b.max.x, cy, cz);
            camPan.SetBounds(leftBoundTmp, rightBoundTmp);
        }
        else
        {
            var leftT = GetAllyCameraPointByState();
            var rightT = GetEnemyCameraPointByState();
            camPan.SetBounds(leftT, rightT);
        }
    }
}
