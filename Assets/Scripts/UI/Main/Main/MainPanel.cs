using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanel : BasePanel
{
    [Header("MainPanel")]
    [SerializeField]
    GameObject m_minimapTileBtnPrefeb = null;
    [SerializeField]
    GameObject m_minimapScrollView = null;
    [SerializeField]
    Transform m_minimapScrollViewContentTrans = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPanelOpen()
    {
        SetPanelName("");
        SetBuildingLevel("");
    }

    public void ClickSkipBtn()
    {

    }

    public void ClickMinimapBtn()
    {
        GameDataManager _manager = m_gameDataManager;
        m_minimapScrollView.SetActive(true);

        // 기존 UI 정리
        GameObjectUtils.ClearChildren(m_minimapScrollViewContentTrans);

        // 타일맵 크기 가져오기
        Vector2Int mapSize = _manager.TileMapManager.GetTileMapSize();
        
        if (mapSize == Vector2Int.zero)
        {
            Debug.LogError("타일맵이 생성되지 않았습니다.");
            return;
        }

        // 모든 타일을 생성
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                TileMapState tileState = _manager.TileMapManager.GetTileMapState(x, y);
                
                if (tileState != null)
                {
                    // 타일의 지형 타입에 해당하는 TileMapData 가져오기
                    TileMapData tileData = _manager.TileMapManager.GetTileMapData(tileState.m_terrainType);
                    
                    if (tileData != null)
                    {
                        // UI 버튼 생성
                        GameObject tileBtn = Instantiate(m_minimapTileBtnPrefeb, m_minimapScrollViewContentTrans);
                        MinimapBtn minimapBtn = tileBtn.GetComponent<MinimapBtn>();
                        
                        if (minimapBtn != null)
                        {
                            minimapBtn.Init(tileData, tileState);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"타일 데이터를 찾을 수 없습니다: {tileState.m_terrainType}");
                    }
                }
                else
                {
                    Debug.LogWarning($"타일 상태를 가져올 수 없습니다: ({x}, {y})");
                }
            }
        }
    }
}
