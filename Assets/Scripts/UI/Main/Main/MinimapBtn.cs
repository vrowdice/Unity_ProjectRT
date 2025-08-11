using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_text = null;

    private TileMapData m_tileMapData = null;
    private TileMapState m_tileMapState = null;

    public void Init(TileMapData argTileMapData, TileMapState argTileMapState)
    {
        m_tileMapData = argTileMapData;
        m_tileMapState = argTileMapState;

        gameObject.GetComponent<Image>().color = m_tileMapData.m_color;
        if (argTileMapState.m_isRoad == true)
        {
            m_text.gameObject.SetActive(true);
            m_text.text = "Road";
        }
    }

    public void Click()
    {

    }
}
