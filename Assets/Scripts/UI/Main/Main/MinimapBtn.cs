using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapBtn : MonoBehaviour
{
    TileMapData m_tileMapData = null;
    TileMapState m_tileMapState = null;

    public void Init(TileMapData argTileMapData, TileMapState argTileMapState)
    {
        m_tileMapData = argTileMapData;
        m_tileMapState = argTileMapState;

        gameObject.GetComponent<Image>().color = m_tileMapData.m_color;
    }

    public void Click()
    {

    }
}
