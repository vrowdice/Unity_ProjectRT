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

    public void Init(TileMapData argTileMapData, TileMapState argTileMapState, MinimapPanel argMinimapPanel)
    {


        m_tileMapData = argTileMapData;
        m_tileMapState = argTileMapState;
        m_minimapPanel = argMinimapPanel;

        gameObject.GetComponent<Image>().color = m_tileMapData.m_color;

        
    }

    public void Click()
    {
        m_minimapPanel.OpenMinimapDetailPanel(m_tileMapData, m_tileMapState);
    }
}
