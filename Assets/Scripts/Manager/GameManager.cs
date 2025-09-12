using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임의 핵심 로직을 관리하는 매니저 클래스
/// 싱글톤 패턴으로 구현되어 전역에서 접근 가능
/// 날짜, 리소스, 토큰, UI 매니저 등을 관리
/// </summary>
public class GameManager : MonoBehaviour
{

    
    [Header("UI Prefabs")]
    [SerializeField]
    private GameObject m_warningPanelPrefeb = null;
    [SerializeField]
    private GameObject m_confirmDialogPrefab = null;

    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }
    public IUIManager MainUiManager => m_nowUIManager;

    // 관리자 컴포넌트들
    private SceneLoadManager m_sceneLoadManager = null;

    // 게임 상태 데이터
    public int Date { get; private set; }

    // 현재 활성화된 UI 매니저
    private IUIManager m_nowUIManager = null;

    // 자원 관리는 GameDataManager.ResourceManager로 위임
    public long WealthToken => GameDataManager.Instance.ResourceManager.WealthToken;
    public long ExchangeToken => GameDataManager.Instance.ResourceManager.ExchangeToken;

    /// <summary>
    /// 싱글톤 패턴 초기화
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 필요한 매니저 컴포넌트들 자동 추가
            InitializeManagerComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 매니저 컴포넌트들을 자동으로 추가
    /// </summary>
    private void InitializeManagerComponents()
    {
        // SceneLoadManager 컴포넌트 추가
        m_sceneLoadManager = gameObject.AddComponent<SceneLoadManager>();
    }

    /// <summary>
    /// 게임 시작 시 초기 설정
    /// </summary>
    void Start()
    {
        FirstSetting();
    }

    /// <summary>
    /// 게임의 첫 번째 설정
    /// UI 매니저 설정, 건물 생산량 계산
    /// </summary>
    void FirstSetting()
    {
        SetupUIManager();
        GetBuildingDateResource();
    }

    /// <summary>
    /// UI 매니저 찾기 및 설정
    /// </summary>
    private void SetupUIManager()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return;
        }

        m_nowUIManager = canvas.GetComponent<IUIManager>();
        if (m_nowUIManager == null)
        {
            m_nowUIManager = canvas.GetComponentInChildren<IUIManager>();
        }

        if (m_nowUIManager != null)
        {
            m_nowUIManager.Initialize(this, GameDataManager.Instance);
        }
        else
        {
            Debug.LogError("No IUIManager found on the Canvas or its children!");
        }
    }

    /// <summary>
    /// 날짜를 추가하고 관련 로직 실행
    /// 리소스 생산, 요청 생성, 이벤트 체크, 연구 진행 등을 수행
    /// </summary>
    /// <param name="argAddDate">추가할 날짜 수</param>
    public void AddDate(int argAddDate)
    {
        if (argAddDate < 0)
        {
            Date = 0;
            Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
            return;
        }

        if (GameDataManager.Instance.GameBalanceEntry.m_data.m_maxDate <= Date)
        {
            Date = GameDataManager.Instance.GameBalanceEntry.m_data.m_maxDate;
        }

        GameDataManager.Instance.ResourceManager.GetBuildingDateResource();
        GameDataManager.Instance.ResourceManager.GetDayResource(argAddDate);

        Date += argAddDate;

        UpdateGameBalance();
        UpdateResearchProgress(argAddDate);
        CheckAndProcessRequests();
        CheckAndProcessEvents();

        GameDataManager.Instance.EffectManager.GetAllActiveEffectInfo();
    }

    /// <summary>
    /// 게임 밸런스 업데이트
    /// </summary>
    private void UpdateGameBalance()
    {
        GameBalanceEntry balanceEntry = GameDataManager.Instance.GameBalanceEntry;
        balanceEntry.m_state.m_dateMul = 1.0f + Mathf.Pow(balanceEntry.m_data.m_dateBalanceMul, Date);
    }

    /// <summary>
    /// 요청 생성 및 강제 연락 요청 체크
    /// </summary>
    private void CheckAndProcessRequests()
    {
        GameBalanceEntry balanceEntry = GameDataManager.Instance.GameBalanceEntry;
        
        // 요청 생성 조건 체크
        if (Date % balanceEntry.m_data.m_makeRequestDate == 0)
        {
            GameDataManager.Instance.MakeRandomRequest();
        }

        // 강제 연락 요청 체크
        if (balanceEntry.m_data.m_forcedContactRequestDateList.Contains(Date))
        {
            GameDataManager.Instance.ForceContactRequest();
        }
    }

    /// <summary>
    /// 이벤트 발생 체크
    /// </summary>
    private void CheckAndProcessEvents()
    {
        if (GameDataManager.Instance.EventManager.ProcessEventDate())
        {
            Warning(InfoMessages.EventOccurs);
        }
    }

    /// <summary>
    /// 연구 진행도 업데이트
    /// </summary>
    /// <param name="daysPassed">경과한 일수</param>
    private void UpdateResearchProgress(int daysPassed)
    {
        if (GameDataManager.Instance.FactionResearchEntryDict == null) return;

        // 모든 팩션의 연구 진행도 업데이트
        foreach (var factionKvp in GameDataManager.Instance.FactionResearchEntryDict)
        {
            var researchEntry = factionKvp.Value;
            if (researchEntry == null) continue;

            // 팩션의 모든 연구를 확인
            foreach (var researchKvp in researchEntry.ResearchByKey)
            {
                var researchData = researchKvp.Value;
                var researchState = researchEntry.GetResearchStateByKey(researchKvp.Key);
                
                if (researchData == null || researchState == null) continue;

                // 연구가 진행 중인 경우에만 진행도 증가
                if (researchState.IsInProgress)
                {
                    // 진행도 증가
                    researchState.m_progress += daysPassed;

                    // 연구 완료 체크
                    if (researchState.m_progress >= researchData.m_duration)
                    {
                        // 연구 완료
                        researchState.m_progress = (int)researchData.m_duration;
                        researchState.m_isResearched = true;
                        
                        // 연구 완료 시 이펙트 활성화
                        researchEntry.ActivateResearchEffects(researchKvp.Key, GameDataManager.Instance);
                        
                        Debug.Log($"Research completed: {researchData.m_name} (Faction: {factionKvp.Key})");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 여러 리소스를 한 번에 변경
    /// 부족한 리소스가 있으면 변경하지 않고 false 반환
    /// </summary>
    /// <param name="argResourceChanges">변경할 리소스와 양</param>
    /// <returns>성공 여부</returns>
    public bool TryChangeAllResources(Dictionary<ResourceType.TYPE, long> argResourceChanges)
    {
        bool result = GameDataManager.Instance.ResourceManager.TryChangeAllResources(argResourceChanges);
        if (result)
        {
            UpdateUI();
        }
        return result;
    }

    /// <summary>
    /// 단일 리소스를 변경
    /// 부족한 리소스면 변경하지 않고 false 반환
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <param name="argAmount">변경할 양 (음수면 소비)</param>
    /// <returns>성공 여부</returns>
    public bool TryChangeResource(ResourceType.TYPE argType, long argAmount)
    {
        bool result = GameDataManager.Instance.ResourceManager.TryChangeResource(argType, argAmount);
        if (result)
        {
            UpdateUI();
        }
        return result;
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (m_nowUIManager != null)
        {
            m_nowUIManager.UpdateAllMainText();
        }
    }

    /// <summary>
    /// 모든 건물의 생산량을 계산하여 일일 생산량 업데이트
    /// 이벤트 효과도 적용
    /// </summary>
    public void GetBuildingDateResource()
    {
        GameDataManager.Instance.ResourceManager.GetBuildingDateResource();
    }



    /// <summary>
    /// 씬을 로드
    /// </summary>
    /// <param name="argSceneName">로드할 씬 이름</param>
    public void LoadScene(string argSceneName)
    {
        if (m_sceneLoadManager != null)
        {
            m_sceneLoadManager.LoadScene(argSceneName);
        }
        else
        {
            Debug.LogError("SceneLoadManager is not assigned!");
        }
    }

    /// <summary>
    /// 특정 리소스의 현재 보유량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>보유량</returns>
    public long GetResource(ResourceType.TYPE argType)
    {
        return GameDataManager.Instance.ResourceManager.GetResource(argType);
    }

    /// <summary>
    /// 특정 리소스의 일일 생산량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>일일 생산량</returns>
    public long GetDayAddResource(ResourceType.TYPE argType)
    {
        return GameDataManager.Instance.ResourceManager.GetDayAddResource(argType);
    }

    /// <summary>
    /// 특정 리소스의 건물 생산량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>건물 생산량</returns>
    public long GetBuildingProduction(ResourceType.TYPE argType)
    {
        return GameDataManager.Instance.ResourceManager.GetBuildingProduction(argType);
    }

    /// <summary>
    /// 특정 리소스의 영토 생산량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>영토 생산량</returns>
    public long GetTerritoryProduction(ResourceType.TYPE argType)
    {
        return GameDataManager.Instance.ResourceManager.GetTerritoryProduction(argType);
    }

    /// <summary>
    /// 경고 메시지를 표시
    /// </summary>
    /// <param name="argWarnStr">경고 메시지</param>
    public void Warning(string argWarnStr)
    {
        if (m_warningPanelPrefeb == null || m_nowUIManager?.CanvasTrans == null)
        {
            Debug.LogError("Warning panel prefab or canvas transform is null!");
            return;
        }

        GameObject warningObj = Instantiate(m_warningPanelPrefeb, m_nowUIManager.CanvasTrans);
        Transform textTransform = warningObj.transform.Find("Text");
        
        if (textTransform != null)
        {
            Text textComponent = textTransform.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = argWarnStr;
            }
            else
            {
                Debug.LogError("Text component not found on warning panel!");
            }
        }
        else
        {
            Debug.LogError("Text transform not found on warning panel!");
        }
    }

    /// <summary>
    /// 확인 다이얼로그를 표시
    /// </summary>
    /// <param name="message">다이얼로그 메시지</param>
    /// <param name="onYes">확인 버튼 클릭 시 실행할 액션</param>
    public void ShowConfirmDialog(string message, Action onYes)
    {
        if (m_confirmDialogPrefab == null || m_nowUIManager?.CanvasTrans == null)
        {
            Debug.LogError("Confirm dialog prefab or canvas transform is null!");
            return;
        }

        GameObject dialogObj = Instantiate(m_confirmDialogPrefab, m_nowUIManager.CanvasTrans);
        ConfirmDialogUI dialog = dialogObj.GetComponent<ConfirmDialogUI>();
        
        if (dialog != null)
        {
            dialog.Setup(message, onYes);
        }
        else
        {
            Debug.LogError("ConfirmDialogUI component not found on dialog prefab!");
        }
    }
}