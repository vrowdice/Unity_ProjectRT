using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_text = null;
    [SerializeField]
    Transform m_simpleIconContent = null;
    [SerializeField]
    Transform m_detailIconContent = null;

    private TileMapData m_tileMapData = null;
    private TileMapState m_tileMapState = null;
    private MinimapPanel m_minimapPanel = null;
    private bool m_showDetailResources = false;

    public void Init(TileMapData argTileMapData, TileMapState argTileMapState, MinimapPanel argMinimapPanel)
    {
        IUIManager _uiManager = argMinimapPanel.MainUIManager;

        m_tileMapData = argTileMapData;
        m_tileMapState = argTileMapState;
        m_minimapPanel = argMinimapPanel;

        gameObject.GetComponent<Image>().color = m_tileMapData.m_color;

        // 도로 여부 텍스트 표시
        UpdateRoadText();

        // 기존 자원 아이콘들 정리
        ClearResourceIcons();

        // 자원 아이콘들 생성
        CreateResourceIcons(_uiManager);
    }

    /// <summary>
    /// 도로 여부에 따라 텍스트 업데이트
    /// </summary>
    private void UpdateRoadText()
    {
        if (m_text != null && m_tileMapState != null)
        {
            if (m_tileMapState.m_isRoad)
            {
                m_text.text = "R";
                m_text.color = Color.white; // 도로는 흰색으로 표시
            }
            else
            {
                m_text.text = ""; // 도로가 아니면 텍스트 숨김
            }
        }
    }

    /// <summary>
    /// 리소스 표시 모드를 설정하고 UI 업데이트
    /// </summary>
    /// <param name="showDetail">true: 상세 모드, false: 간단 모드</param>
    public void SetResourceDisplayMode(bool showDetail)
    {
        m_showDetailResources = showDetail;
        UpdateResourceDisplay();
    }

    /// <summary>
    /// 현재 설정된 표시 모드에 따라 리소스 표시 업데이트
    /// </summary>
    private void UpdateResourceDisplay()
    {
        if (m_simpleIconContent != null)
            m_simpleIconContent.gameObject.SetActive(!m_showDetailResources);
            
        if (m_detailIconContent != null)
            m_detailIconContent.gameObject.SetActive(m_showDetailResources);
    }

    private void ClearResourceIcons()
    {
        // m_simpleIconContent의 기존 자식들 제거
        if (m_simpleIconContent != null)
        {
            foreach (Transform child in m_simpleIconContent)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        // m_detailIconContent의 기존 자식들 제거
        if (m_detailIconContent != null)
        {
            foreach (Transform child in m_detailIconContent)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }

    private void CreateResourceIcons(IUIManager uiManager)
    {
        if (m_tileMapState?.m_resources == null) return;

        // 각 자원 타입별로 아이콘 생성
        foreach (ResourceType.TYPE resourceType in System.Enum.GetValues(typeof(ResourceType.TYPE)))
        {
            long amount = m_tileMapState.m_resources.GetAmount(resourceType);
            
            // 자원이 0보다 클 때만 아이콘 생성
            if (amount > 0)
            {
                // m_simpleIconContent에 ResourceIconImagePrefeb 생성
                if (m_simpleIconContent != null && uiManager.ResourceIconImagePrefeb != null)
                {
                    GameObject simpleIcon = Instantiate(uiManager.ResourceIconImagePrefeb, m_simpleIconContent);
                    Image iconImage = simpleIcon.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = GameDataManager.Instance.GetResourceIcon(resourceType);
                    }
                }

                // m_detailIconContent에 ResourceIconTextPrefeb 생성
                if (m_detailIconContent != null && uiManager.ResourceIconTextPrefeb != null)
                {
                    GameObject detailIcon = Instantiate(uiManager.ResourceIconTextPrefeb, m_detailIconContent);
                    ResourceIconText resourceIconText = detailIcon.GetComponent<ResourceIconText>();
                    if (resourceIconText != null)
                    {
                        resourceIconText.InitializeMainText(resourceType, amount);
                    }
                }
            }
        }
        
        // 초기 표시 모드 설정
        UpdateResourceDisplay();
    }

    public void Click()
    {
        m_minimapPanel.OpenMinimapDetailPanel(m_tileMapData, m_tileMapState);
    }
}
