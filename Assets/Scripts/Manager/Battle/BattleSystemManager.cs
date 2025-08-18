using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class BattleSystemManager : MonoBehaviour
{
    public static BattleSystemManager Instance { get; private set; }


    [Header("로딩 UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("스폰 영역")]
    [SerializeField] private BoxCollider2D attackSpawnArea;
    [SerializeField] private BoxCollider2D defenseSpawnArea;

    [Header("카메라 포인트")]
    [SerializeField] private Transform attackCameraPoint;
    [SerializeField] private Transform defenseCameraPoint;
    [SerializeField] private Camera mainCamera;

    [Header("유닛 데이터")]
    [SerializeField] private AllyArmyData allyArmyData;
    [SerializeField] private EnemyArmyData enemyArmyData;

    [Header("적 수동 배치")]
    [SerializeField] private Transform enemyUnitHolder;
    [SerializeField] private bool useManualEnemyPlacement = true;

    [Header("결과 UI")]
    [SerializeField] private GameObject resultUIPrefab;
    [SerializeField] private Canvas targetCanvas;

    [Header("전투 중 카메라 드래그")]
    [SerializeField] private CameraDragPan camPan;
    [SerializeField] private BoxCollider2D mapBounds;

    [Header("스폰 충돌 검사(무할당)")]
    [SerializeField] private LayerMask unitLayerMask = -1;

    private readonly List<UnitBase> allyUnits = new();
    private readonly List<UnitBase> enemyUnits = new();

    public IReadOnlyList<UnitBase> AllyUnits => allyUnits;
    public IReadOnlyList<UnitBase> EnemyUnits => enemyUnits;

    private bool isPlayerAttacker = true;
    public bool IsPlayerAttacker => isPlayerAttacker;
    public bool IsBattleRunning { get; private set; }
    public bool IsViewingAllyBase { get; private set; } = true;

    public Transform AttackCameraPoint => attackCameraPoint;
    public Transform DefenseCameraPoint => defenseCameraPoint;

    // 전투 중 유닛 변경 이벤트
    public event UnityAction UnitsChanged;

    // 내부 캐시
    private BattleRuntimeManager runtime;
    private BattleUIManager ui;

    private Transform leftBoundTmp;
    private Transform rightBoundTmp;

    private static readonly Collider2D[] s_overlapBuf = new Collider2D[1];

    private static readonly WaitForSeconds s_waitShort = new WaitForSeconds(0.2f);

    private void Awake()
    {
        if (Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (!mainCamera) mainCamera = Camera.main;
        ui = BattleUIManager.Instance;

        EnsurePanBoundsTransforms();
        ApplyPanBoundsFromMap();
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
        yield return null; 

        yield return SpawnEnemyUnits();

        if (!ui) ui = BattleUIManager.Instance;
        ui?.InitializeUI(allyArmyData ? allyArmyData.units : null);

        if (loadingText) loadingText.text = "Finish!";
        yield return s_waitShort;
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    public Transform GetAllyCameraPointByState() => isPlayerAttacker ? attackCameraPoint : defenseCameraPoint;
    public Transform GetEnemyCameraPointByState() => isPlayerAttacker ? defenseCameraPoint : attackCameraPoint;

    public void ToggleView()
    {
        IsViewingAllyBase = !IsViewingAllyBase;

        if (IsViewingAllyBase)
        {
            BattleCameraManager.Instance?.MoveToAllyPosition();
            ui?.SwitchToAllyUI();
        }
        else
        {
            BattleCameraManager.Instance?.MoveToEnemyPosition();
            ui?.SwitchToEnemyUI();
        }

        RefreshDragEnabled();
    }

    public void RefreshDragEnabled()
    {
        bool allowDrag = IsViewingAllyBase && !IsBattleRunning;
        for (int i = 0; i < allyUnits.Count; i++)
        {
            var u = allyUnits[i];
            if (!u) continue;
            if (u.TryGetComponent<UnitDragHandler>(out var drag))
                drag.EnableDrag(allowDrag);
        }
    }

    private void EnsureTeams()
    {
        for (int i = 0; i < allyUnits.Count; i++)
            FastAssignTeamAndMask(allyUnits[i], TeamSide.Ally);

        for (int i = 0; i < enemyUnits.Count; i++)
            FastAssignTeamAndMask(enemyUnits[i], TeamSide.Enemy);
    }

    private static void FastAssignTeamAndMask(UnitBase ub, TeamSide team)
    {
        if (!ub) return;
        if (ub.Team != team) ub.AssignTeam(team);
        if (ub.TryGetComponent<UnitTargetingController>(out var tgt))
            tgt.RefreshMask();
    }
    public void StartBattle()
    {
        Debug.Log($"[BattleSystemManager] StartBattle  Allies={allyUnits.Count}  Enemies={enemyUnits.Count}");

        EnsureTeams();

        ui?.HideAllUI(exceptTopBar: true);
        ui?.ShowTopBar();

        // 전투 컨트롤러 초기화(방향 고정: 2D, z=0)
        for (int i = 0; i < allyUnits.Count; i++)
            allyUnits[i]?.GetComponent<UnitCombatController>()?.InitForBattle(Vector3.right);

        for (int i = 0; i < enemyUnits.Count; i++)
            enemyUnits[i]?.GetComponent<UnitCombatController>()?.InitForBattle(Vector3.left);

        RefreshDragEnabled();

        ApplyPanBoundsFromMap();
        if (camPan) camPan.Enable(true);

        // 즉시 종료 케이스 처리
        if (allyUnits.Count == 0 || enemyUnits.Count == 0)
        {
            HandleBattleFinished(new BattleOutcome
            {
                victory = allyUnits.Count > 0,
                allyRemaining = allyUnits.Count,
                enemyRemaining = enemyUnits.Count,
                elapsed = 0.0f
            });
            return;
        }

        // 전투 시작
        if (runtime != null) runtime.OnBattleFinished -= HandleBattleFinished;
        runtime = gameObject.AddComponent<BattleRuntimeManager>();
        runtime.OnBattleFinished += HandleBattleFinished;

        IsBattleRunning = true;
        runtime.Begin(allyUnits, enemyUnits, IsPlayerAttacker);
    }

    private void HandleBattleFinished(BattleOutcome outcome)
    {
        IsBattleRunning = false;
        if (camPan) camPan.Enable(false);
        ShowResult(outcome);
    }

    private void ShowResult(BattleOutcome outcome)
    {
        if (resultUIPrefab && targetCanvas)
        {
            var go = Instantiate(resultUIPrefab, targetCanvas.transform);
            if (go.TryGetComponent<BattleResultUI>(out var uiResult))
            {
                uiResult.Show(outcome);

                uiResult.OnContinue = () =>
                {
                    Destroy(go);
                    Debug.Log("Continue pressed");
                };

                uiResult.OnRetry = () =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                };
            }
            else
            {
                Debug.LogWarning("BattleResultUI 컴포넌트가 프리팹에 없습니다.");
            }
        }
        else
        {
            Debug.Log($"Result => {(outcome.victory ? "VICTORY" : "DEFEAT")} | " +
                      $"Allies:{outcome.allyRemaining} Enemies:{outcome.enemyRemaining} " +
                      $"Time:{outcome.elapsed:0.0}s");
        }
    }

    private IEnumerator SpawnEnemyUnits()
    {
        enemyUnits.Clear();

        if (useManualEnemyPlacement && enemyUnitHolder)
        {
            if (loadingText) loadingText.text = "Applying manual enemy placement...";
            int childCount = enemyUnitHolder.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var t = enemyUnitHolder.GetChild(i);
                if (!t.TryGetComponent<UnitBase>(out var ub)) continue;

                if (ub.UnitStat == null)
                {
                    Debug.LogError($"[BattleSystemManager] 수동 배치 유닛 '{ub.name}'의 UnitStat이 null입니다. Initialize(stat) 필요.");
                    continue;
                }

                FastAssignTeamAndMask(ub, TeamSide.Enemy);
                AddEnemyUnit(ub);
            }
            yield break;
        }

        // 적군 데이터가 없으면 경고 후 종료
        if (!enemyArmyData)
        {
            Debug.LogWarning("[BattleSystemManager] enemyArmyData 미할당");
            yield break;
        }

        if (loadingText) loadingText.text = "Spawning enemies...";

        var enemyArea = GetEnemySpawnArea();
        if (!enemyArea)
        {
            Debug.LogError("[BattleSystemManager] 적 스폰 구역 없음");
            yield break;
        }

        var list = enemyArmyData.units;
        int total = list.Count;
        for (int i = 0; i < total; i++)
        {
            var stat = list[i];
            if (!stat || !stat.prefab) continue;

            Vector3 pos = GetRandomPointInCollider(enemyArea);
            var go = Instantiate(stat.prefab, pos, Quaternion.identity);
            if (go.TryGetComponent<UnitBase>(out var ub))
            {
                ub.Initialize(stat);
                FastAssignTeamAndMask(ub, TeamSide.Enemy);
                AddEnemyUnit(ub);
            }

            if (progressBar) progressBar.fillAmount = (float)(i + 1) / Mathf.Max(1, total);
            yield return null;
        }
    }

    public UnitBase RequestSpawnAlly(UnitStatBase stat)
    {
        if (!stat || !stat.prefab) return null;

        var area = GetAllySpawnArea();
        var go = Instantiate(stat.prefab, GetRandomPointInCollider(area), Quaternion.identity);

        if (go.TryGetComponent<UnitBase>(out var ub))
        {
            ub.Initialize(stat);
            ub.AssignTeam(TeamSide.Ally);
            RegisterUnit(ub);
            return ub;
        }

        Destroy(go);
        return null;
    }

    public bool RecallAlly(UnitBase unit)
    {
        if (!unit) return false;

        allyUnits.Remove(unit);
        Destroy(unit.gameObject);
        UnitsChanged?.Invoke();
        return true;
    }

    public void UnregisterUnit(UnitBase unit)
    {
        if (!unit) return;
        allyUnits.Remove(unit);
        enemyUnits.Remove(unit);
        UnitsChanged?.Invoke();
        CheckWinCondition();
    }

    public void AddEnemyUnit(UnitBase unit)
    {
        if (!unit) return;
        if (!enemyUnits.Contains(unit))
        {
            FastAssignTeamAndMask(unit, TeamSide.Enemy);
            enemyUnits.Add(unit);
            UnitsChanged?.Invoke();
        }
    }

    public void RegisterUnit(UnitBase unit)
    {
        if (!unit) return;

        if (unit.Team == TeamSide.Ally)
        {
            if (!allyUnits.Contains(unit)) allyUnits.Add(unit);
            enemyUnits.Remove(unit);
        }
        else if (unit.Team == TeamSide.Enemy)
        {
            if (!enemyUnits.Contains(unit)) enemyUnits.Add(unit);
            allyUnits.Remove(unit);
        }
        else
        {
            Debug.LogWarning($"[BSM] TeamSide.Neutral 유닛은 등록하지 않음: {unit.name}");
        }

        UnitsChanged?.Invoke();
    }

    private void CheckWinCondition()
    {
        if (allyUnits.Count == 0) Debug.Log("[BattleSystemManager] 패배 - 아군 전멸");
        else if (enemyUnits.Count == 0) Debug.Log("[BattleSystemManager] 승리 - 적군 전멸");
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

    private static void CountUnitsInAreaByList(BoxCollider2D area, Dictionary<UnitTagType, int> counts, List<UnitBase> list)
    {
        if (!area) { Debug.LogError("카운트를 셀 스폰 구역이 null"); return; }
        var b = area.bounds;

        for (int i = 0; i < list.Count; i++)
        {
            var unit = list[i];
            if (!unit || unit.UnitStat == null) continue;
            if (b.Contains(unit.transform.position))
            {
                var tag = unit.UnitStat.unitTagType;
                if (counts.ContainsKey(tag)) counts[tag]++;
            }
        }
    }

    // 스폰 위치 선정
    private Vector3 GetRandomPointInCollider(BoxCollider2D col)
    {
        if (!col) { Debug.LogError("[BattleSystemManager] Collider null"); return Vector3.zero; }
        var b = col.bounds;

        const int MAX_TRY = 20;
        const float RADIUS = 0.3f;

        for (int i = 0; i < MAX_TRY; i++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            var p = new Vector3(x, y, 0f);

            int hit = Physics2D.OverlapCircleNonAlloc(p, RADIUS, s_overlapBuf, unitLayerMask);
            if (hit == 0) return p; 
        }

        return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0.0f);
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
        if (!camPan || !mapBounds) return;

        var b = mapBounds.bounds;
        float cy = mainCamera ? mainCamera.transform.position.y : 0.0f;
        float cz = mainCamera ? mainCamera.transform.position.z : -10.0f;

        leftBoundTmp.position = new Vector3(b.min.x, cy, cz);
        rightBoundTmp.position = new Vector3(b.max.x, cy, cz);
    }

    public BoxCollider2D GetEnemySpawnArea() => isPlayerAttacker ? defenseSpawnArea : attackSpawnArea;
    public BoxCollider2D GetAllySpawnArea() => isPlayerAttacker ? attackSpawnArea : defenseSpawnArea;
}
