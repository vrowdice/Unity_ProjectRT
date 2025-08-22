using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
[DisallowMultipleComponent]
public class BattleSystemManager : MonoBehaviour
{
    #region Singleton & Refs

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

    [Header("Unit 거리 조절")]
    [SerializeField] private LayerMask unitLayerMask = 0;

    #endregion

    #region Runtime State & Properties & Events

    private readonly List<UnitBase> allyUnits = new();
    private readonly List<UnitBase> enemyUnits = new();

    public IReadOnlyList<UnitBase> AllyUnits => allyUnits;
    public IReadOnlyList<UnitBase> EnemyUnits => enemyUnits;

    private bool isPlayerAttacker = true;
    public bool IsPlayerAttacker => isPlayerAttacker;

    public bool IsBattleRunning { get; private set; } = false;
    public bool IsViewingAllyBase { get; private set; } = true;

    public Transform AttackCameraPoint => attackCameraPoint;
    public Transform DefenseCameraPoint => defenseCameraPoint;




    public event UnityAction UnitsChanged;
    private BattleRuntimeManager runtime;
    private BattleUIManager ui;

    private Transform leftBoundTmp;
    private Transform rightBoundTmp;

    private static readonly Collider2D[] s_overlapBuf = new Collider2D[1];
    private static readonly WaitForSeconds s_waitShort = new WaitForSeconds(0.2f);

    #endregion

    #region Unity Lifetime

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!mainCamera) mainCamera = Camera.main;

        ui = BattleUIManager.Instance;

        EnsurePanBoundAnchors();
        ApplyPanBoundsFromMap();
    }

    private void Start()
    {
        StartCoroutine(InitializeBattleScene());
    }

    private void OnDestroy()
    {
        if (runtime != null) runtime.OnBattleFinished -= HandleBattleFinished;
    }

    #endregion

    #region Scene Initialization & Flow

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

    #endregion

    #region Camera View Toggle

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

    private void RefreshDragEnabled()
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

    #endregion

    #region Battle Start / Finish

    public void StartBattle()
    {
        Debug.Log($"[BSM] StartBattle  Allies={allyUnits.Count}  Enemies={enemyUnits.Count}");

        EnsureTeams();

        ui?.HideAllUI(exceptTopBar: true);
        ui?.ShowTopBar();

        // 전투 컨트롤러 초기화
        for (int i = 0; i < allyUnits.Count; i++)
            allyUnits[i]?.GetComponent<UnitCombatController>()?.InitForBattle(Vector3.right);

        for (int i = 0; i < enemyUnits.Count; i++)
            enemyUnits[i]?.GetComponent<UnitCombatController>()?.InitForBattle(Vector3.left);

        RefreshDragEnabled();

        ApplyPanBoundsFromMap();
        if (camPan) camPan.Enable(true);

        // 즉시 종료 케이스
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

        // 런타임 구동
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
                    Debug.Log("[BSM] Continue pressed");
                };

                uiResult.OnRetry = () =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                };
            }
            else
            {
                Debug.LogWarning("[BSM] BattleResultUI 컴포넌트를 찾지 못했습니다.");
            }
        }
        else
        {
            Debug.Log($"[BSM] Result => {(outcome.victory ? "VICTORY" : "DEFEAT")} | " +
                      $"Allies:{outcome.allyRemaining} Enemies:{outcome.enemyRemaining} " +
                      $"Time:{outcome.elapsed:0.0}s");
        }
    }

    #endregion

    #region Spawn / Placement

    private IEnumerator SpawnEnemyUnits()
    {
        enemyUnits.Clear();

        // 수동 배치 우선
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
                    Debug.LogError($"[BSM] 수동 배치 '{ub.name}'의 UnitStat이 null입니다. Initialize(stat) 필요.");
                    continue;
                }

                FastAssignTeamAndMask(ub, TeamSide.Enemy);
                AddEnemyUnit(ub);


            }
            yield break;
        }

        // 데이터 스폰
        if (!enemyArmyData)
        {
            Debug.LogWarning("[BSM] enemyArmyData 미할당");
            yield break;
        }

        if (loadingText) loadingText.text = "Spawning enemies...";

        var enemyArea = GetEnemySpawnArea();
        if (!enemyArea)
        {
            Debug.LogError("[BSM] 적 스폰 구역이 없습니다.");
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
        if (!area) { Debug.LogError("[BSM] 아군 스폰 구역이 없습니다."); return null; }

        var go = Instantiate(stat.prefab, GetRandomPointInCollider(area), Quaternion.identity);

        if (go.TryGetComponent<UnitBase>(out var ub))
        {
            ub.Initialize(stat);
            
            var mover = ub.GetComponent<UnitMovementController>();
            if (mover) mover.SetMapBounds(mapBounds);

            ub.AssignTeam(TeamSide.Ally);
            RegisterUnit(ub);              
                                       
            return ub;
        }
        Debug.Log($"[Spawn] {ub.name} team={ub.Team} stat={ub.UnitStat.name} id={ub.UnitStat.GetInstanceID()}");

        Destroy(go);
        return null;
    }

    public bool RecallAlly(UnitBase unit)
    {
        if (!unit) return false;

        bool removed = allyUnits.Remove(unit);
        if (!removed) return false;

        unit.OnDied -= HandleUnitDied;  
        Destroy(unit.gameObject);

        UnitsChanged?.Invoke();
        return true;
    }


    // 팀 부여
    private static void FastAssignTeamAndMask(UnitBase ub, TeamSide team)
    {
        if (!ub) return;
        if (ub.Team != team) ub.AssignTeam(team);
        if (ub.TryGetComponent<UnitTargetingController>(out var tgt))
            tgt.RefreshMask();
    }

    private void EnsureTeams()
    {
        for (int i = 0; i < allyUnits.Count; i++) FastAssignTeamAndMask(allyUnits[i], TeamSide.Ally);
        for (int i = 0; i < enemyUnits.Count; i++) FastAssignTeamAndMask(enemyUnits[i], TeamSide.Enemy);
    }

    // 등록/해제
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

        unit.OnDied -= HandleUnitDied;
        unit.OnDied += HandleUnitDied;

        UnitsChanged?.Invoke();
    }

    public void UnregisterUnit(UnitBase unit)
    {
        if (!unit) return;

        unit.OnDied -= HandleUnitDied; 

        allyUnits.Remove(unit);
        enemyUnits.Remove(unit);

        UnitsChanged?.Invoke();
        CheckWinCondition();
    }

    private void HandleUnitDied(UnitBase dead)
    {
        UnregisterUnit(dead);
    }

    public void AddEnemyUnit(UnitBase unit)
    {
        if (!unit) return;
        if (enemyUnits.Contains(unit)) return;

        FastAssignTeamAndMask(unit, TeamSide.Enemy);
        enemyUnits.Add(unit);

        var mover = unit.GetComponent<UnitMovementController>();
        if (mover) mover.SetMapBounds(mapBounds);

        UnitsChanged?.Invoke();
    }

    private void CheckWinCondition()
    {
        if (allyUnits.Count == 0) Debug.Log("[BSM] 패배 - 아군 전멸");
        else if (enemyUnits.Count == 0) Debug.Log("[BSM] 승리 - 적군 전멸");
    }

    #endregion

    #region UI Counter APIs (배치 UI에서 사용)

    public Dictionary<UnitTagType, int> GetCountsInSpawnAreas()
    {
        var counts = NewTagCounter();
        CountUnitsInAreaByList(GetAllySpawnArea(), counts, allyUnits);
        return counts;
    }

    public Dictionary<UnitTagType, int> GetEnemyCountsInSpawnAreas()
    {
        var counts = NewTagCounter();
        CountUnitsInAreaByList(GetEnemySpawnArea(), counts, enemyUnits);
        return counts;
    }

    private static Dictionary<UnitTagType, int> NewTagCounter()
    {
        return new Dictionary<UnitTagType, int>
        {
            { UnitTagType.Melee,   0 },
            { UnitTagType.Range,   0 },
            { UnitTagType.Defense, 0 }
        };
    }

    private static void CountUnitsInAreaByList(BoxCollider2D area, Dictionary<UnitTagType, int> counts, List<UnitBase> list)
    {
        if (!area) { Debug.LogError("[BSM] 카운트 영역이 null"); return; }
        if (list == null || list.Count == 0) return;

        var b = area.bounds;
        for (int i = 0; i < list.Count; i++)
        {
            var unit = list[i];
            if (!unit) continue;

            var stat = unit.UnitStat;
            if (stat == null) continue;

            if (!b.Contains(unit.transform.position)) continue;

            var tag = stat.unitTagType;
            if (counts.ContainsKey(tag)) counts[tag]++;
        }
    }

    #endregion

    #region Spawn Utilities

    private Vector3 GetRandomPointInCollider(BoxCollider2D col)
    {
        if (!col) { Debug.LogError("[BSM] Collider null"); return Vector3.zero; }

        var b = col.bounds;
        const int MAX_TRY = 20;
        const float RADIUS = 0.3f;

        for (int i = 0; i < MAX_TRY; i++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            var p = new Vector3(x, y, 0.0f);

            int hit = Physics2D.OverlapCircleNonAlloc(p, RADIUS, s_overlapBuf, unitLayerMask);
            if (hit == 0) return p;
        }
        return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0.0f);
    }

    #endregion

    #region Unit Event Hooks for UI refresh
    private void HookUnit(UnitBase u)
    {
        if (!u) return;
        u.OnDied -= OnUnitDied_UIRefresh;
        u.OnDied += OnUnitDied_UIRefresh;
    }

    private void UnhookUnit(UnitBase u)
    {
        if (!u) return;
        u.OnDied -= OnUnitDied_UIRefresh;
    }

    private void OnUnitDied_UIRefresh(UnitBase dead)
    {
        UnitsChanged?.Invoke();
    }
    #endregion

    #region Camera Pan Bounds

    private void EnsurePanBoundAnchors()
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

    #endregion

    #region Ally/Enemy Spawn Area Accessors

    public BoxCollider2D GetEnemySpawnArea() => isPlayerAttacker ? defenseSpawnArea : attackSpawnArea;
    public BoxCollider2D GetAllySpawnArea() => isPlayerAttacker ? attackSpawnArea : defenseSpawnArea;

    #endregion
}
