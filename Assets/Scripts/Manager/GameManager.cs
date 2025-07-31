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
    [SerializeField]
    private SceneLoadManager m_scenLoadManager = null;
    [SerializeField]
    private GameDataManager m_gameDataManager = null;
    [SerializeField]
    private GameObject m_warningPanelPrefeb = null;
    [SerializeField]
    private GameObject m_confirmDialogPrefab = null;

    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }
    public GameDataManager GameDataManager => m_gameDataManager;

    // 게임 상태 데이터
    public int Date { get; private set; }
    public long WealthToken { get; private set; }
    public long ExchangeToken { get; private set; }

    // 현재 활성화된 UI 매니저
    private IUIManager m_nowUIManager = null;

    // 리소스 관리 딕셔너리
    private Dictionary<ResourceType.TYPE, long> m_resourcesDict = new();
    private Dictionary<ResourceType.TYPE, long> m_producedResourcesDict = new();

    /// <summary>
    /// 싱글톤 패턴 초기화
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
    /// 리소스 초기화, UI 매니저 설정, 건물 생산량 계산
    /// </summary>
    void FirstSetting()
    {
        // 모든 리소스를 초기값으로 설정
        foreach (ResourceType.TYPE argType in EnumUtils.GetAllEnumValues<ResourceType.TYPE>())
        {
            //리소스를 모두 0으로 초기화 하고
            m_resourcesDict[argType] = 100000;
            m_producedResourcesDict[argType] = 0;
        }

        // UI 매니저 찾기 및 설정
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            m_nowUIManager = canvas.GetComponent<IUIManager>();

            if (m_nowUIManager == null)
            {
                m_nowUIManager = canvas.GetComponentInChildren<IUIManager>();
            }

            if (m_nowUIManager != null)
            {
                m_nowUIManager.Initialize(this);
            }
            else
            {
                Debug.LogError("No IUIManager found on the Canvas or its children!");
            }
        }
        else
        {
            Debug.LogError("No Canvas found in the scene!");
        }

        GetBuildingDateResource();
    }

    /// <summary>
    /// 날짜를 추가하고 관련 로직 실행
    /// 리소스 생산, 요청 생성, 이벤트 체크 등을 수행
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

        if(GameDataManager.GameBalanceEntry.m_data.m_maxDate <= Date)
        {
            Date = GameDataManager.GameBalanceEntry.m_data.m_maxDate;
        }

        GetBuildingDateResource();
        GetDayResource(argAddDate);

        Date += argAddDate;

        // 게임 밸런스 업데이트
        GameBalanceEntry _balanceEntry = m_gameDataManager.GameBalanceEntry;

        _balanceEntry.m_state.m_dateMul = 1.0f + Mathf.Pow(_balanceEntry.m_data.m_dateBalanceMul, Date);
        
        // 요청 생성 조건 체크
        if (Date % _balanceEntry.m_data.m_makeRequestDate == 0)
        {
            m_gameDataManager.MakeRandomRequest();
        }

        // 강제 연락 요청 체크
        if(_balanceEntry.m_data.m_forcedContactRequestList.Contains(Date) == true)
        {
            m_gameDataManager.ForceContactRequest();
        }

        // 이벤트 발생 체크
        if (m_gameDataManager.EventEntry.AddDate() == true)
        {
            Warning(InfoMessages.EventOccurs);
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
        // 먼저 모든 리소스가 충분한지 확인
        foreach (KeyValuePair<ResourceType.TYPE, long> item in argResourceChanges)
        {
            if (!m_resourcesDict.ContainsKey(item.Key))
            {
                Debug.LogError($"Resource type {item.Key} not found in resource dictionary.");
                return false;
            }

            if (item.Value < 0)
            {
                if (m_resourcesDict[item.Key] + item.Value < 0)
                {
                    Warning($"Not enough {item.Key} to perform this action. Required: {Mathf.Abs(item.Value)}, Have: {m_resourcesDict[item.Key]}");
                    return false;
                }
            }
        }

        // 모든 리소스 변경 실행
        foreach (KeyValuePair<ResourceType.TYPE, long> item in argResourceChanges)
        {
            m_resourcesDict[item.Key] += item.Value;
        }

        m_nowUIManager.UpdateAllMainText();
        return true;
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
        if (!m_resourcesDict.ContainsKey(argType))
        {
            Debug.LogError($"Resource type {argType} not found in resource dictionary.");
            return false;
        }

        if (argAmount < 0)
        {
            if (m_resourcesDict[argType] + argAmount < 0)
            {
                Warning($"Not enough {argType} to perform this action. Required: {Mathf.Abs(argAmount)}, Have: {m_resourcesDict[argType]}");
                return false;
            }
        }

        m_resourcesDict[argType] += argAmount;

        m_nowUIManager.UpdateAllMainText();
        return true;
    }

    /// <summary>
    /// 지정된 일수만큼 리소스를 생산
    /// </summary>
    /// <param name="argDay">생산할 일수</param>
    public void GetDayResource(int argDay)
    {
        for (int i = 0; i < argDay; i++)
        {
            foreach (var kvp in m_producedResourcesDict.ToList())
            {
                TryChangeResource(kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// 모든 건물의 생산량을 계산하여 일일 생산량 업데이트
    /// 이벤트 효과도 적용
    /// </summary>
    public void GetBuildingDateResource()
    {
        //생산량 딕셔너리를 모두 0으로
        foreach (var key in m_producedResourcesDict.Keys.ToList())
        {
            m_producedResourcesDict[key] = 0;
        }

        // 모든 건물의 생산량 계산
        foreach (var buildingPair in GameDataManager.BuildingEntryDict)
        {
            BuildingEntry building = buildingPair.Value;
            building.ApplyProduction();

            foreach (var production in building.m_state.m_calculatedProductionList)
            {
                float modifier = 1f;
                if (m_gameDataManager.EventEntry.m_state.m_buildingResourceModDic.TryGetValue(production.m_type, out float value))
                {
                    modifier = (value == 0f) ? 1f : value;
                }

                if (m_producedResourcesDict.ContainsKey(production.m_type))
                {
                    m_producedResourcesDict[production.m_type] += (long)(production.m_amount * modifier);
                }
                else
                {
                    Debug.LogWarning($"[GetBuildingDateResource] Unknown resource type: {production.m_type}");
                }
            }
        }
    }

    /// <summary>
    /// 씬을 로드
    /// </summary>
    /// <param name="argSceneName">로드할 씬 이름</param>
    public void LoadScene(string argSceneName)
    {
        m_scenLoadManager.LoadScene(argSceneName);
    }

    /// <summary>
    /// 특정 리소스의 현재 보유량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>보유량</returns>
    public long GetResource(ResourceType.TYPE argType)
    {
        return m_resourcesDict.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 특정 리소스의 일일 생산량을 가져옴
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <returns>일일 생산량</returns>
    public long GetDayAddResource(ResourceType.TYPE argType)
    {
        return m_producedResourcesDict.TryGetValue(argType, out long value) ? value : 0;
    }

    /// <summary>
    /// 경고 메시지를 표시
    /// </summary>
    /// <param name="argWarnStr">경고 메시지</param>
    public void Warning(string argWarnStr)
    {
        GameObject _obj = Instantiate(m_warningPanelPrefeb, m_nowUIManager.CanvasTrans);
        _obj.transform.Find("Text").gameObject.GetComponent<Text>().text = argWarnStr;
    }

    /// <summary>
    /// 확인 다이얼로그를 표시
    /// </summary>
    /// <param name="message">다이얼로그 메시지</param>
    /// <param name="onYes">확인 버튼 클릭 시 실행할 액션</param>
    public void ShowConfirmDialog(string message, Action onYes)
    {
        GameObject dialogObj = Instantiate(m_confirmDialogPrefab, m_nowUIManager.CanvasTrans);
        ConfirmDialogUI dialog = dialogObj.GetComponent<ConfirmDialogUI>();
        dialog.Setup(message, onYes);
    }
}