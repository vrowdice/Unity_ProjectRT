using UnityEngine;
using TMPro;

/// <summary>
/// 모든 패널의 기본 클래스
/// 공통 기능들을 관리하여 코드 중복을 줄임
/// </summary>
public abstract class BasePanel : MonoBehaviour, IUIPanel
{
    [Header("Panel Settings")]
    [SerializeField] protected TextMeshProUGUI m_showInfoPanelText = null;
    [SerializeField] protected string m_panelName = "";
    [SerializeField] protected string m_buildingCodeForLevel = ""; // 빌딩 코드 (문자열로 설정)
    [SerializeField] protected string m_levelPrefix = "LV. "; // 레벨 표시 접두사

    protected GameDataManager m_gameDataManager = null;
    protected MainUIManager m_mainUIManager = null;

    // 프로퍼티들
    public GameDataManager GameDataManager => m_gameDataManager;
    public MainUIManager MainUIManager => m_mainUIManager;

    /// <summary>
    /// 패널이 열릴 때 호출되는 공통 로직
    /// </summary>
    /// <param name="argDataManager">게임 데이터 매니저</param>
    /// <param name="argUIManager">UI 매니저</param>
    public virtual void OnOpen(GameDataManager argDataManager, MainUIManager argUIManager)
    {
        // null 체크
        if (argDataManager == null || argUIManager == null)
        {
            Debug.LogError("GameDataManager or MainUIManager is null in BasePanel.OnOpen");
            return;
        }

        m_gameDataManager = argDataManager;
        m_mainUIManager = argUIManager;

        // 패널 활성화
        gameObject.SetActive(true);

        // 하위 클래스에서 구현할 초기화 로직 (패널 설정 포함)
        OnPanelOpen();

        // 패널 정보 설정 (OnPanelOpen 이후에 호출하여 설정된 값들 사용)
        string infoText = GetPanelInfoText();

        if(m_showInfoPanelText != null)
        {
            m_showInfoPanelText.text = infoText;
        }
    }

    /// <summary>
    /// 패널이 닫힐 때 호출되는 공통 로직
    /// </summary>
    public virtual void OnClose()
    {
        OnPanelClose();
    }

    /// <summary>
    /// 패널 정보 텍스트를 생성
    /// </summary>
    /// <returns>패널 정보 텍스트</returns>
    protected virtual string GetPanelInfoText()
    {
        if (string.IsNullOrEmpty(m_panelName))
        {
            return "";
        }

        // 빌딩 레벨이 설정되어 있으면 레벨 정보 추가
        if (!string.IsNullOrEmpty(m_buildingCodeForLevel) && m_gameDataManager != null)
        {
            try
            {
                var buildingEntry = m_gameDataManager.GetBuildingEntry(m_buildingCodeForLevel);
                if (buildingEntry != null)
                {
                    return $"{m_panelName} {m_levelPrefix}{buildingEntry.m_state.m_amount}";
                }
                else
                {
                    Debug.LogWarning($"Building entry not found for code: {m_buildingCodeForLevel}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get building level for {m_panelName}: {e.Message}");
            }
        }

        return m_panelName;
    }

    /// <summary>
    /// 하위 클래스에서 구현할 패널 열기 로직
    /// </summary>
    protected virtual void OnPanelOpen()
    {
        // 하위 클래스에서 오버라이드
    }

    /// <summary>
    /// 하위 클래스에서 구현할 패널 닫기 로직
    /// </summary>
    protected virtual void OnPanelClose()
    {
        // 하위 클래스에서 오버라이드
    }

    /// <summary>
    /// 빌딩 레벨을 동적으로 설정
    /// </summary>
    /// <param name="buildingCode">빌딩 코드</param>
    protected void SetBuildingLevel(string buildingCode)
    {
        m_buildingCodeForLevel = buildingCode;
    }

    /// <summary>
    /// 빌딩 레벨을 동적으로 설정 (정수)
    /// </summary>
    /// <param name="buildingCode">빌딩 코드</param>
    protected void SetBuildingLevel(int buildingCode)
    {
        m_buildingCodeForLevel = buildingCode.ToString();
    }

    /// <summary>
    /// 패널 이름을 동적으로 설정
    /// </summary>
    /// <param name="panelName">패널 이름</param>
    protected void SetPanelName(string panelName)
    {
        m_panelName = panelName;
    }

    /// <summary>
    /// 안전한 빌딩 엔트리 가져오기
    /// </summary>
    /// <param name="buildingCode">빌딩 코드</param>
    /// <returns>빌딩 엔트리, 없으면 null</returns>
    protected BuildingEntry GetBuildingEntrySafe(string buildingCode)
    {
        if (m_gameDataManager == null || string.IsNullOrEmpty(buildingCode))
        {
            return null;
        }

        return m_gameDataManager.GetBuildingEntry(buildingCode);
    }

    /// <summary>
    /// 안전한 빌딩 엔트리 가져오기 (정수)
    /// </summary>
    /// <param name="buildingCode">빌딩 코드</param>
    /// <returns>빌딩 엔트리, 없으면 null</returns>
    protected BuildingEntry GetBuildingEntrySafe(int buildingCode)
    {
        return GetBuildingEntrySafe(buildingCode.ToString());
    }
} 