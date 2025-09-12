using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

/// <summary>
/// 메인 UI를 관리하는 매니저 클래스
/// 리소스 표시, 패널 전환, UI 업데이트 등을 담당
/// IUIManager 인터페이스를 구현
/// </summary>
public class MainUIManager : MonoBehaviour, IUIManager
{
    [SerializeField]
    List<GameObject> m_panelList = new List<GameObject>();
    [SerializeField]
    GameObject m_quickMovePanel = null;
    [SerializeField]
    GameObject m_BackMainBtn = null;

    [Header("Resource Panel Text")]
    [SerializeField]
    TextMeshProUGUI m_woodText = null;
    [SerializeField]
    TextMeshProUGUI m_addWoodText = null;
    [SerializeField]
    TextMeshProUGUI m_ironText = null;
    [SerializeField]
    TextMeshProUGUI m_addIronText = null;
    [SerializeField]
    TextMeshProUGUI m_foodText = null;
    [SerializeField]
    TextMeshProUGUI m_addFoodText = null;
    [SerializeField]
    TextMeshProUGUI m_techText = null;
    [SerializeField]
    TextMeshProUGUI m_addTechText = null;

    [Header("Resource Add Info Panel")]
    [SerializeField]
    GameObject m_resourceAddInfoPanelPrefab = null;

    [Header("Game Info Text")]
    [SerializeField]
    TextMeshProUGUI m_dateText = null;
    [SerializeField]
    TextMeshProUGUI m_requestText = null;

    [Header("Common UI")]
    [SerializeField]
    GameObject m_resourceIconTextPrefeb = null;
    [SerializeField]
    GameObject m_conditionPanelTextPrefeb = null;
    [SerializeField]
    GameObject m_resourceIconImagePrefeb = null;

    [Header("Debug Settings")]
    [SerializeField]
    private bool m_enableDebugMode = false;
    [SerializeField]
    private GameObject m_debugManagerPrefab = null;

    // 참조 변수들
    private GameManager m_gameManager = null;
    private List<IUIPanel> m_iPanelList = new List<IUIPanel>();
    private Transform m_canvasTrans = null;
    private int m_nowPanelIndex = 0;
    
    // 동적 패널 관리
    private ResourceAddInfoPanel m_currentInfoPanel = null;
    private GameObject m_backgroundBlocker = null;

    // 디버그 매니저 관리
    private GameObject m_debugManagerInstance = null;
    private DebugManager m_debugManager = null;

    // 프로퍼티들
    public GameObject ResourceIconTextPrefeb { get => m_resourceIconTextPrefeb; }
    public GameObject ConditionPanelTextPrefeb { get => m_conditionPanelTextPrefeb; }
    public GameObject ResourceIconImagePrefeb { get => m_resourceIconImagePrefeb; }
    public Transform CanvasTrans => m_canvasTrans;

    /// <summary>
    /// 특정 리소스의 현재 보유량을 UI 텍스트에 표시
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    void SetResourceText(ResourceType.TYPE argType)
    {
        long resourceAmount = m_gameManager.GetResource(argType);
        string formattedAmount = ReplaceUtils.FormatNumber(resourceAmount);

        switch (argType)
        {
            case ResourceType.TYPE.Wood:
                m_woodText.text = formattedAmount;
                break;
            case ResourceType.TYPE.Iron:
                m_ironText.text = formattedAmount;
                break;
            case ResourceType.TYPE.Food:
                m_foodText.text = formattedAmount;
                break;
            case ResourceType.TYPE.Tech:
                m_techText.text = formattedAmount;
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                break;
        }
    }

    /// <summary>
    /// 특정 리소스의 일일 생산량을 UI 텍스트에 표시
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    void SetAddResourceText(ResourceType.TYPE argType)
    {
        m_gameManager.GetBuildingDateResource();

        long resourceAmount = m_gameManager.GetDayAddResource(argType);
        string formattedAmount = ReplaceUtils.FormatNumber(resourceAmount);

        switch (argType)
        {
            case ResourceType.TYPE.Wood:
                m_addWoodText.text = "+ " + formattedAmount;
                break;
            case ResourceType.TYPE.Iron:
                m_addIronText.text = "+ " + formattedAmount;
                break;
            case ResourceType.TYPE.Food:
                m_addFoodText.text = "+ " + formattedAmount;
                break;
            case ResourceType.TYPE.Tech:
                m_addTechText.text = "+ " + formattedAmount;
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorNoSuchType);
                break;
        }
    }

    /// <summary>
    /// UI 매니저 초기화
    /// 패널 리스트 설정 및 첫 번째 패널 활성화
    /// </summary>
    /// <param name="argGameManager">게임 매니저 참조</param>
    public void Initialize(GameManager argGameManager, GameDataManager argGameDataManager)
    {
        m_gameManager = argGameManager;
        m_canvasTrans = transform;

        //패널 초기화
        foreach (GameObject item in m_panelList)
        {
            item.SetActive(false);

            IUIPanel panel = item.GetComponent<IUIPanel>();
            if (panel != null)
            {
                m_iPanelList.Add(panel);
            }
            else
            {
                Debug.LogWarning($"Panel object {item.name} does not implement IUIPanel.");
                m_iPanelList.Add(null);
            }
        }
        m_panelList[0].SetActive(true);

        m_BackMainBtn.SetActive(false);

        MovePanel(0);

        UpdateAllMainText();
        SetResourceIconButton();

        // 디버그 모드가 활성화되어 있다면 디버그 매니저 초기화
        InitializeDebugManager();
    }

    /// <summary>
    /// 디버그 매니저 초기화
    /// </summary>
    private void InitializeDebugManager()
    {
        if (!m_enableDebugMode)
        {
            Debug.Log("Debug mode is disabled.");
            return;
        }

        if (m_debugManagerPrefab == null)
        {
            Debug.LogWarning("Debug Manager prefab is not assigned. Debug features will not be available.");
            return;
        }

        try
        {
            // 기존 디버그 매니저가 있다면 제거
            if (m_debugManagerInstance != null)
            {
                DestroyImmediate(m_debugManagerInstance);
            }

            // 디버그 매니저 프리팹 생성
            m_debugManagerInstance = Instantiate(m_debugManagerPrefab);
            m_debugManager = m_debugManagerInstance.GetComponent<DebugManager>();

            if (m_debugManager != null)
            {
                // 디버그 매니저 초기화
                m_debugManager.Initialize(m_gameManager, this);
                Debug.Log("Debug Manager successfully initialized. Press F1 to open debug panel.");
            }
            else
            {
                Debug.LogError("DebugManager component not found!");
                if (m_debugManagerInstance != null)
                {
                    DestroyImmediate(m_debugManagerInstance);
                    m_debugManagerInstance = null;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error occurred while initializing Debug Manager: {ex.Message}");
        }
    }

    /// <summary>
    /// 디버그 모드 토글 (런타임에서 활성화/비활성화)
    /// </summary>
    [ContextMenu("Toggle Debug Mode")]
    public void ToggleDebugMode()
    {
        m_enableDebugMode = !m_enableDebugMode;

        if (m_enableDebugMode)
        {
            InitializeDebugManager();
        }
        else
        {
            DestroyDebugManager();
        }

        Debug.Log($"Debug mode {(m_enableDebugMode ? "enabled" : "disabled")}.");
    }

    /// <summary>
    /// 디버그 매니저 제거
    /// </summary>
    private void DestroyDebugManager()
    {
        if (m_debugManagerInstance != null)
        {
            DestroyImmediate(m_debugManagerInstance);
            m_debugManagerInstance = null;
            m_debugManager = null;
            Debug.Log("Debug Manager destroyed.");
        }
    }

    public void SetResourceIconButton()
    {

    }

    public void OpenResourceAddInfoPanel(int argTypeInt)
    {
        // 인트 값을 ResourceType.TYPE으로 직접 캐스팅
        if (!System.Enum.IsDefined(typeof(ResourceType.TYPE), argTypeInt))
        {
            Debug.LogWarning($"Invalid resource type index: {argTypeInt}");
            return;
        }
        
        ResourceType.TYPE resourceType = (ResourceType.TYPE)argTypeInt;
        
        // 이미 패널이 열려있으면 닫기
        if (m_currentInfoPanel != null)
        {
            CloseCurrentInfoPanel();
            return;
        }
        
        // 프리팹이 없으면 에러
        if (m_resourceAddInfoPanelPrefab == null)
        {
            Debug.LogError("Resource add info panel prefab is not assigned!");
            return;
        }
        
        // 배경 블로커 생성 (아무 곳이나 누르면 패널이 닫히도록)
        CreateBackgroundBlocker();
        
        // 패널 생성
        GameObject panelObject = Instantiate(m_resourceAddInfoPanelPrefab, m_canvasTrans);
        m_currentInfoPanel = panelObject.GetComponent<ResourceAddInfoPanel>();
        
        if (m_currentInfoPanel == null)
        {
            Debug.LogError("ResourceAddInfoPanel component not found on prefab!");
            Destroy(panelObject);
            return;
        }
        
        // 활성화된 모든 이펙트 가져오기
        List<EffectBase> activeEffects = GameDataManager.Instance.EffectManager.GetAllActiveEffects();
        
        // 패널 초기화
        m_currentInfoPanel.Init(activeEffects, resourceType);
        
        // 패널 위치 설정 (버튼 아래에 배치)
        SetPanelPosition(argTypeInt);
    }
    
    /// <summary>
    /// 현재 열린 정보 패널 닫기
    /// </summary>
    private void CloseCurrentInfoPanel()
    {
        if (m_currentInfoPanel != null)
        {
            Destroy(m_currentInfoPanel.gameObject);
            m_currentInfoPanel = null;
        }
        
        if (m_backgroundBlocker != null)
        {
            Destroy(m_backgroundBlocker);
            m_backgroundBlocker = null;
        }
    }
    
    /// <summary>
    /// 배경 블로커 생성 (아무 곳이나 누르면 패널이 닫히도록)
    /// </summary>
    private void CreateBackgroundBlocker()
    {
        m_backgroundBlocker = new GameObject("BackgroundBlocker");
        m_backgroundBlocker.transform.SetParent(m_canvasTrans, false);
        
        // RectTransform 설정
        RectTransform rectTransform = m_backgroundBlocker.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.SetAsLastSibling(); // 맨 앞으로 보내기
        
        // Image 컴포넌트 추가 (투명하게)
        UnityEngine.UI.Image image = m_backgroundBlocker.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, 0);
        
        // Button 컴포넌트 추가
        UnityEngine.UI.Button button = m_backgroundBlocker.AddComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(CloseCurrentInfoPanel);
        
        // Button 설정 개선
        button.transition = UnityEngine.UI.Selectable.Transition.None;
        button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        
        // RaycastTarget 활성화
        image.raycastTarget = true;
    }
    
    /// <summary>
    /// 패널 위치 설정 (마우스 위치에 배치)
    /// </summary>
    /// <param name="resourceTypeIndex">리소스 타입 인덱스 (사용하지 않음)</param>
    private void SetPanelPosition(int resourceTypeIndex)
    {
        if (m_currentInfoPanel == null) return;
        
        RectTransform panelRect = m_currentInfoPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // 마우스 위치를 Canvas 좌표로 변환
            Vector2 mousePosition;
            Canvas canvas = m_canvasTrans.GetComponent<Canvas>();
            
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // ScreenSpaceCamera 모드일 때
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_canvasTrans as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out mousePosition
                );
            }
            else
            {
                // ScreenSpaceOverlay 모드일 때
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_canvasTrans as RectTransform,
                    Input.mousePosition,
                    null,
                    out mousePosition
                );
            }
            
            // 패널을 마우스 위치에 배치
            panelRect.anchoredPosition = mousePosition;
            
            // 패널의 앵커를 중앙으로 설정
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0f, 1f); // 왼쪽 상단을 피벗으로 설정
        }
    }

    /// <summary>
    /// 모든 메인 UI 텍스트를 업데이트
    /// 리소스, 날짜, 요청 정보 등을 모두 갱신
    /// </summary>
    public void UpdateAllMainText()
    {
        SetResourceText(ResourceType.TYPE.Wood);
        SetResourceText(ResourceType.TYPE.Iron);
        SetResourceText(ResourceType.TYPE.Food);
        SetResourceText(ResourceType.TYPE.Tech);

        SetAddResourceText(ResourceType.TYPE.Wood);
        SetAddResourceText(ResourceType.TYPE.Iron);
        SetAddResourceText(ResourceType.TYPE.Food);
        SetAddResourceText(ResourceType.TYPE.Tech);

        UpdateDateText();
        UpdateRequestText();
    }

    /// <summary>
    /// 리소스를 추가하고 UI 업데이트
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <param name="argAmount">추가할 양</param>
    /// <returns>성공 여부</returns>
    public bool TryAdd(ResourceType.TYPE argType, int argAmount)
    {
        if (m_gameManager.TryChangeResource(argType, argAmount))
        {
            SetResourceText(argType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 리소스를 소비하고 UI 업데이트
    /// </summary>
    /// <param name="argType">리소스 타입</param>
    /// <param name="argAmount">소비할 양</param>
    /// <returns>성공 여부</returns>
    public bool TryConsume(ResourceType.TYPE argType, int argAmount)
    {
        if (m_gameManager.TryChangeResource(argType, argAmount))
        {
            SetResourceText(argType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 패널 인덱스에 따라 패널 이동
    /// </summary>
    /// <param name="argPanelIndex">패널 인덱스</param>
    public void MovePanel(int argPanelIndex)
    {
        if (!IsValidPanelIndex(argPanelIndex))
        {
            Debug.LogWarning($"Invalid panel index: {argPanelIndex}. Valid range: 0-{m_panelList.Count - 1}");
            return;
        }

        m_nowPanelIndex = argPanelIndex;

        DeactivateAllPanels();
        ActivateCurrentPanel();
        UpdateBackButton();
        OpenCurrentPanel();

        if (m_quickMovePanel != null)
        {
            m_quickMovePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 패널 인덱스가 유효한지 확인
    /// </summary>
    /// <param name="panelIndex">확인할 패널 인덱스</param>
    /// <returns>유효하면 true</returns>
    private bool IsValidPanelIndex(int panelIndex)
    {
        return panelIndex >= 0 && panelIndex < m_panelList.Count;
    }

    /// <summary>
    /// 모든 패널 비활성화
    /// </summary>
    private void DeactivateAllPanels()
    {
        foreach (GameObject item in m_panelList)
        {
            if (item != null)
            {
                item.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 현재 패널 활성화
    /// </summary>
    private void ActivateCurrentPanel()
    {
        if (m_panelList[m_nowPanelIndex] != null)
        {
            m_panelList[m_nowPanelIndex].SetActive(true);
        }
        else
        {
            Debug.LogError($"Panel at index {m_nowPanelIndex} is null!");
        }
    }

    /// <summary>
    /// 뒤로가기 버튼 업데이트
    /// </summary>
    private void UpdateBackButton()
    {
        if (m_BackMainBtn != null)
        {
            m_BackMainBtn.SetActive(m_nowPanelIndex != 0);
        }
    }

    /// <summary>
    /// 현재 패널 열기
    /// </summary>
    private void OpenCurrentPanel()
    {
        if (m_iPanelList[m_nowPanelIndex] != null)
        {
            m_iPanelList[m_nowPanelIndex].OnOpen(GameDataManager.Instance, this);
        }
        else
        {
            Debug.LogWarning($"IUIPanel at index {m_nowPanelIndex} is null!");
        }
    }

    /// <summary>
    /// 요청 텍스트 업데이트
    /// 수락 가능한 요청 수 / 수락된 요청 수 형식으로 표시
    /// </summary>
    public void UpdateRequestText()
    {
        m_requestText.text = GameDataManager.Instance.AcceptableRequestList.Count + " / " + GameDataManager.Instance.AcceptedRequestList.Count;
    }

    /// <summary>
    /// 날짜 텍스트 업데이트
    /// </summary>
    public void UpdateDateText()
    {
        m_dateText.text = GameManager.Instance.Date + " Days";
    }

    /// <summary>
    /// 하루를 추가하고 모든 UI 업데이트
    /// </summary>
    public void AddOneDate()
    {
        GameManager.Instance.AddDate(1);

        UpdateAllMainText();
    }

    /// <summary>
    /// 애플리케이션 종료 시 디버그 매니저 정리
    /// </summary>
    private void OnDestroy()
    {
        DestroyDebugManager();
    }
}
