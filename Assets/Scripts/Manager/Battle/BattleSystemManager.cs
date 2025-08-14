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
    [Tooltip("Unit Data에 따라 유닛 생성 칸+랜덤소환되는 enemey 수가 변함")]
    [SerializeField] private AllyArmyData allyArmyData;
    [SerializeField] private EnemyArmyData enemyArmyData;

    [Header("적 수동 배치(옵션)")]
    [Tooltip("디자이너가 씬에 미리 배치한 적 유닛들의 부모 Transform")]
    [SerializeField] private Transform enemyUnitHolder;
    [SerializeField] private bool useManualEnemyPlacement = true;

    [Header("StartBattle 시 재배치 옵션")]
    [SerializeField] private bool repositionAlliesOnStart = false;
    [SerializeField] private bool repositionEnemiesOnStart = false;

    [Header("결과 UI")]
    [SerializeField] private GameObject resultUIPrefab;
    [SerializeField] private Canvas targetCanvas;

    [Header("전투 중 카메라 드래그")]
    [SerializeField] private CameraDragPan camPan;
    [SerializeField] private BoxCollider2D mapBounds;

    // 런타임 상태
    public List<UnitBase> allyUnits = new();
    public List<UnitBase> enemyUnits = new();

    // 프로퍼티 노출 시켜주는 코드/ 이거 빠지면 타켓팅 등에서 오류 발생
    public List<UnitBase> AllyUnits => allyUnits;
    public List<UnitBase> EnemyUnits => enemyUnits;

    private bool isPlayerAttacker = true;
    public bool IsPlayerAttacker => isPlayerAttacker;
    public bool IsBattleRunning { get; private set; } = false;
    public bool IsViewingAllyBase { get; private set; } = true;

    public Transform AttackCameraPoint => attackCameraPoint;
    public Transform DefenseCameraPoint => defenseCameraPoint;

    public System.Action UnitsChanged;
    private void NotifyUnitsChanged() => UnitsChanged?.Invoke();

    private BattleRuntimeManager runtime;
    private BattleUIManager ui;

    private Transform leftBoundTmp;
    private Transform rightBoundTmp;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!mainCamera) mainCamera = Camera.main;
        ui = BattleUIManager.Instance;

        // 카메라 팬 경계 1차 적용 /camPan에 mapBounds 전달은 CameraDragPan에서 처리
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

        yield return null; // 한 프레임 양보

        // 적 스폰
        yield return StartCoroutine(SpawnEnemyUnits());

        // 배치 UI 초기화
        if (ui == null) ui = BattleUIManager.Instance;
        ui?.InitializeUI(allyArmyData?.units);

        if (loadingText) loadingText.text = "Finish!";
        yield return new WaitForSeconds(0.2f);
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    // 진영 상태에 따른 카메라 포인트
    public Transform GetAllyCameraPointByState() => isPlayerAttacker ? attackCameraPoint : defenseCameraPoint;
    public Transform GetEnemyCameraPointByState() => isPlayerAttacker ? defenseCameraPoint : attackCameraPoint;

    // 아군/적 시점 토글
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

    // 배치 중에만 아군 드래그 허용
    public void RefreshDragEnabled()
    {
        bool allowDrag = IsViewingAllyBase && !IsBattleRunning;
        foreach (var u in allyUnits)
            u?.GetComponent<UnitDragHandler>()?.EnableDrag(allowDrag);
    }

    // 전투 시작
    public void StartBattle()
    {
        Debug.Log("[BattleSystemManager] StartBattle");

        ui?.HideAllUI();

        if (repositionAlliesOnStart) SpawnUnits(allyUnits, GetAllySpawnArea());
        if (repositionEnemiesOnStart) SpawnUnits(enemyUnits, GetEnemySpawnArea());

        IsBattleRunning = true;
        RefreshDragEnabled();

        // 중복 구독 방지
        if (runtime != null) runtime.OnBattleFinished -= HandleBattleFinished;

        runtime = gameObject.AddComponent<BattleRuntimeManager>();
        runtime.OnBattleFinished += HandleBattleFinished;
        runtime.Begin(allyUnits, enemyUnits, IsPlayerAttacker);

        if (camPan)
        {
            ApplyPanBoundsFromMap();
            camPan.Enable(true);
        }
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
            var uiResult = go.GetComponent<BattleResultUI>();
            if (uiResult != null)
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
            Debug.Log(
                $"Result => {(outcome.victory ? "VICTORY" : "DEFEAT")} | " +
                $"Allies:{outcome.allyRemaining} Enemies:{outcome.enemyRemaining} " +
                $"Time:{outcome.elapsed:0.0}s"
            );
        }
    }

    // 스폰
    private IEnumerator SpawnEnemyUnits()
    {
        enemyUnits.Clear();

        if (useManualEnemyPlacement && enemyUnitHolder)
        {
            if (loadingText) loadingText.text = "Applying manual enemy placement...";
            foreach (Transform child in enemyUnitHolder)
            {
                var ub = child.GetComponent<UnitBase>();
                if (!ub) continue;
                if (ub.UnitStat == null)
                {
                    Debug.LogError($"[BattleSystemManager] 수동 배치 유닛 '{ub.name}'의 UnitStat이 null입니다. 스탯 할당 또는 Initialize(stat) 호출 필요.");
                    continue;
                }
                AddEnemyUnit(ub);
            }
            yield break;
        }

        if (!enemyArmyData)
        {
            Debug.LogWarning("[BattleSystemManager] enemyArmyData 미할당");
            yield break;
        }

        if (loadingText) loadingText.text = "Spawning enemies...";
        int total = enemyArmyData.units.Count;
        int i = 0;

        var enemyArea = GetEnemySpawnArea();
        if (!enemyArea)
        {
            Debug.LogError("[BattleSystemManager] 적 스폰 구역 없음");
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
            Debug.LogWarning("[BattleSystemManager] 아군 스폰 구역 null");
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
        if (allyUnits.Count == 0) Debug.Log("[BattleSystemManager] 패배 - 아군 전멸");
        else if (enemyUnits.Count == 0) Debug.Log("[BattleSystemManager] 승리 - 적군 전멸");
    }

    // 카테고리 카운트 샘플
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
        if (area == null) { Debug.LogError("카운트를 셀 스폰 구역이 null"); return; }

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

    private void SpawnUnits(List<UnitBase> list, BoxCollider2D area)
    {
        if (!area) return;
        foreach (var u in list)
        {
            if (!u) continue;
            u.transform.position = GetRandomPointInCollider(area);
        }
    }

    // 겹침 최소화 랜덤 스폰
    private Vector3 GetRandomPointInCollider(BoxCollider2D col)
    {
        if (!col) { Debug.LogError("[BattleSystemManager] Collider null"); return Vector3.zero; }
        var b = col.bounds;

        const int MAX_TRY = 20;
        const float RADIUS = 0.3f; 
        for (int i = 0; i < MAX_TRY; i++)
        {
            Vector3 p = new Vector3(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y),
                0f
            );
            if (!Physics2D.OverlapCircle(p, RADIUS))
                return p;
        }
        return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0f);
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
        }
    }

    public BoxCollider2D GetEnemySpawnArea() => isPlayerAttacker ? defenseSpawnArea : attackSpawnArea;
    public BoxCollider2D GetAllySpawnArea() => isPlayerAttacker ? attackSpawnArea : defenseSpawnArea;
}
