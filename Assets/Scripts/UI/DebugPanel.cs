using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 디버그 패널의 UI를 관리하는 클래스
/// 동적으로 디버그 버튼들을 생성하고 관리
/// </summary>
public class DebugPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField]
    private Transform m_buttonContainer;
    [SerializeField]
    private ScrollRect m_scrollRect;
    [SerializeField]
    private Button m_closeButton;

    [Header("Button Prefab")]
    [SerializeField]
    private GameObject m_debugButtonPrefab;

    private List<GameObject> m_createdButtons = new List<GameObject>();
    private DebugManager m_debugManager;

    /// <summary>
    /// 디버그 패널 초기화
    /// </summary>
    /// <param name="debugManager">디버그 매니저 참조</param>
    public void Initialize(DebugManager debugManager)
    {
        m_debugManager = debugManager;

        // 닫기 버튼 이벤트 설정
        if (m_closeButton != null)
        {
            m_closeButton.onClick.AddListener(ClosePanel);
        }

        // 디버그 버튼들 생성
        CreateDebugButtons();
    }

    /// <summary>
    /// 패널 닫기
    /// </summary>
    public void ClosePanel()
    {
        if (m_debugManager != null)
        {
            m_debugManager.ToggleDebugPanel();
        }
    }

    /// <summary>
    /// 모든 디버그 버튼들을 동적으로 생성
    /// </summary>
    private void CreateDebugButtons()
    {
        if (m_buttonContainer == null || m_debugButtonPrefab == null)
        {
            Debug.LogError("Button container or button prefab is not assigned.");
            return;
        }

        // 기존 버튼들 제거
        ClearExistingButtons();

        // 버튼 데이터 정의
        var buttonData = GetDebugButtonData();

        // 버튼들을 그리드 형태로 배치
        CreateButtonGrid(buttonData);
    }

    /// <summary>
    /// 기존 버튼들 제거
    /// </summary>
    private void ClearExistingButtons()
    {
        foreach (var button in m_createdButtons)
        {
            if (button != null)
            {
                DestroyImmediate(button);
            }
        }
        m_createdButtons.Clear();
    }

    /// <summary>
    /// 디버그 버튼 데이터 정의
    /// </summary>
    /// <returns>버튼 데이터 리스트</returns>
    private List<DebugButtonData> GetDebugButtonData()
    {
        return new List<DebugButtonData>
        {
            // 리소스 디버그 버튼들
            new DebugButtonData("Resource +1000", "Add 1000 to all resources", () => m_debugManager.AddResource1000()),
            new DebugButtonData("Resource +10000", "Add 10000 to all resources", () => m_debugManager.AddResource10000()),
            new DebugButtonData("Wood +1000", "Add 1000 wood", () => m_debugManager.AddSpecificResource(ResourceType.TYPE.Wood, 1000)),
            new DebugButtonData("Iron +1000", "Add 1000 iron", () => m_debugManager.AddSpecificResource(ResourceType.TYPE.Iron, 1000)),
            new DebugButtonData("Food +1000", "Add 1000 food", () => m_debugManager.AddSpecificResource(ResourceType.TYPE.Food, 1000)),
            new DebugButtonData("Tech +1000", "Add 1000 tech", () => m_debugManager.AddSpecificResource(ResourceType.TYPE.Tech, 1000)),

            // 팩션 디버그 버튼들
            new DebugButtonData("Faction Like +5", "Increase all faction likes by 5", () => m_debugManager.AddAllFactionLike5()),
            new DebugButtonData("Max Faction Like", "Set all faction likes to maximum", () => m_debugManager.MaxAllFactionLike()),
            new DebugButtonData("Wolf Like +10", "Increase wolf faction like by 10", () => m_debugManager.AddSpecificFactionLike(FactionType.TYPE.Wolf, 10)),
            new DebugButtonData("Owl Like +10", "Increase owl faction like by 10", () => m_debugManager.AddSpecificFactionLike(FactionType.TYPE.Owl, 10)),
            new DebugButtonData("Cat Like +10", "Increase cat faction like by 10", () => m_debugManager.AddSpecificFactionLike(FactionType.TYPE.Cat, 10)),
            new DebugButtonData("Turtle Like +10", "Increase turtle faction like by 10", () => m_debugManager.AddSpecificFactionLike(FactionType.TYPE.Turtle, 10)),

            // 게임 상태 디버그 버튼들
            new DebugButtonData("Day +1", "Add 1 day", () => m_debugManager.AddDays(1)),
            new DebugButtonData("Day +10", "Add 10 days", () => m_debugManager.AddDays(10)),
            new DebugButtonData("Day +30", "Add 30 days", () => m_debugManager.AddDays(30)),
            new DebugButtonData("Log Game Status", "Print current game status to console", () => m_debugManager.LogGameStatus()),
        };
    }

    /// <summary>
    /// 버튼들을 그리드 형태로 배치
    /// </summary>
    /// <param name="buttonData">버튼 데이터 리스트</param>
    private void CreateButtonGrid(List<DebugButtonData> buttonData)
    {
        for (int i = 0; i < buttonData.Count; i++)
        {
            // 버튼 생성
            GameObject buttonObj = Instantiate(m_debugButtonPrefab, m_buttonContainer);
            m_createdButtons.Add(buttonObj);

            // 버튼 설정
            SetupButton(buttonObj, buttonData[i]);
        }
    }

    /// <summary>
    /// 개별 버튼 설정
    /// </summary>
    /// <param name="buttonObj">버튼 게임오브젝트</param>
    /// <param name="data">버튼 데이터</param>
    private void SetupButton(GameObject buttonObj, DebugButtonData data)
    {
        // 버튼 컴포넌트 설정
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => data.action?.Invoke());
        }

        // 텍스트 설정
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = data.buttonText;
        }
        else
        {
            // Text 컴포넌트로 fallback
            Text legacyText = buttonObj.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.text = data.buttonText;
            }
        }
    }

    /// <summary>
    /// 버튼 데이터 구조체
    /// </summary>
    [System.Serializable]
    public class DebugButtonData
    {
        public string buttonText;
        public string tooltipText;
        public System.Action action;

        public DebugButtonData(string text, string tooltip, System.Action buttonAction)
        {
            buttonText = text;
            tooltipText = tooltip;
            action = buttonAction;
        }
    }
}

/// <summary>
/// 간단한 툴팁 컴포넌트 (선택사항)
/// </summary>
public class Tooltip : MonoBehaviour
{
    public string tooltipText;

    // 필요에 따라 툴팁 표시 로직 구현
}