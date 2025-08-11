using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    GameObject m_BackMainBtn = null;

    [Header("Resource Panel Text")]
    [SerializeField]
    TextMeshProUGUI m_woodText = null;
    [SerializeField]
    TextMeshProUGUI m_addWoodText = null;
    [SerializeField]
    TextMeshProUGUI m_metalText = null;
    [SerializeField]
    TextMeshProUGUI m_addMetalText = null;
    [SerializeField]
    TextMeshProUGUI m_foodText = null;
    [SerializeField]
    TextMeshProUGUI m_addFoodText = null;
    [SerializeField]
    TextMeshProUGUI m_techText = null;
    [SerializeField]
    TextMeshProUGUI m_addTechText = null;

    [Header("Resource Icon Button")]
    [SerializeField]
    Button m_woodIconBtn = null;
    [SerializeField]
    Button m_metalIconBtn = null;
    [SerializeField]
    Button m_foodIconBtn = null;
    [SerializeField]
    Button m_techIconBtn = null;

    [Header("Resource Add Info Panel")]
    [SerializeField]
    GameObject m_resourceAddInfoPanel = null;
    [SerializeField]
    TextMeshProUGUI m_resourceAddInfoTextPrefab = null;
    [SerializeField]
    Transform m_resourceAddInfoContent = null;

    [Header("Game Info Text")]
    [SerializeField]
    TextMeshProUGUI m_dateText = null;
    [SerializeField]
    TextMeshProUGUI m_requestText = null;

    [Header("Panel Info Panel")]
    [SerializeField]
    GameObject m_panelInfoPanel = null;
    [SerializeField]
    TextMeshProUGUI m_panelInfoText = null;

    [Header("Common UI")]
    [SerializeField]
    GameObject m_resourceIconTextPrefeb = null;

    // 참조 변수들
    private GameManager m_gameManager = null;
    private List<IUIPanel> m_iPanelList = new List<IUIPanel>();
    private Transform m_canvasTrans = null;
    private int m_nowPanelIndex = 0;

    // 프로퍼티들
    public GameObject ResourceIconTextPrefeb { get => m_resourceIconTextPrefeb; }
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
                m_metalText.text = formattedAmount;
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
                m_addMetalText.text = "+ " + formattedAmount;
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
    /// <param name="gameManager">게임 매니저 참조</param>
    public void Initialize(GameManager gameManager)
    {
        m_gameManager = gameManager;
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
    }

    public void SetResourceIconButton()
    {
        m_woodIconBtn.onClick.AddListener(() => SetResourceText(ResourceType.TYPE.Wood));
        m_metalIconBtn.onClick.AddListener(() => SetResourceText(ResourceType.TYPE.Iron));
        m_foodIconBtn.onClick.AddListener(() => SetResourceText(ResourceType.TYPE.Food));
        m_techIconBtn.onClick.AddListener(() => SetResourceText(ResourceType.TYPE.Tech));
    }

    public void SetResourceAddInfoPanel()
    {
        m_resourceAddInfoPanel.SetActive(false);
    }

    public void OpenResourceAddInfoPanel(ResourceType.TYPE argType)
    {
        m_resourceAddInfoPanel.SetActive(true);
        TextMeshProUGUI text = Instantiate(m_resourceAddInfoTextPrefab, m_resourceAddInfoContent);
        text.text = argType.ToString();
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
            m_iPanelList[m_nowPanelIndex].OnOpen(GameManager.Instance.GameDataManager, this);
        }
        else
        {
            Debug.LogWarning($"IUIPanel at index {m_nowPanelIndex} is null!");
        }
    }

    /// <summary>
    /// 패널 설명 패널 설정
    /// </summary>
    /// <param name="argIsOn">활성화 여부</param>
    /// <param name="argTextContent">텍스트 내용</param>
    public void ManagePanelInfoPanel(bool argIsOn, string argTextContent)
    {
        if (m_panelInfoPanel == null)
        {
            Debug.LogError("m_panelInfoPanel is null!");
            return;
        }
        
        if (m_panelInfoText == null)
        {
            Debug.LogError("m_panelInfoText is null!");
            return;
        }
        
        m_panelInfoPanel.SetActive(argIsOn);
        m_panelInfoText.text = argTextContent;
    }

    /// <summary>
    /// 요청 텍스트 업데이트
    /// 수락 가능한 요청 수 / 수락된 요청 수 형식으로 표시
    /// </summary>
    public void UpdateRequestText()
    {
        m_requestText.text = m_gameManager.GameDataManager.AcceptableRequestList.Count + " / " + m_gameManager.GameDataManager.AcceptedRequestList.Count;
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

    //--디버그용--
    //리소스를 모두 추가하거나 리소스를 모두
    /// <summary>
    /// 디버그용: 모든 리소스를 1000씩 추가
    /// </summary>
    public void AddResource1000()
    {
        TryAdd(ResourceType.TYPE.Wood, 1000);
        TryAdd(ResourceType.TYPE.Iron, 1000);
        TryAdd(ResourceType.TYPE.Food, 1000);
        TryAdd(ResourceType.TYPE.Tech, 1000);
    }
}
