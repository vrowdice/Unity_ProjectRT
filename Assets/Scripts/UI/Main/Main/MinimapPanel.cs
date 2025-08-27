using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapPanel : MonoBehaviour
{
	[SerializeField]
	MinimapDetailPanel m_minimapDetailPanel = null;
	[SerializeField]
	GameObject m_minimapTileBtnPrefeb = null;
	[SerializeField]
	Transform m_minimapScrollViewContentTrans = null;

	[SerializeField]
	float m_zoomSpeed = 0.1f;
	[SerializeField]
	float m_minZoom = 0.5f;
	[SerializeField]
	float m_maxZoom = 2.0f;

	private ScrollRect scrollRect;
	private RectTransform contentRect;
	private GameDataManager m_gameDataManager = null;
	private MainUIManager m_mainUIManager = null;

	void Start()
	{
		if (scrollRect == null)
		{
			scrollRect = GetComponent<ScrollRect>();
		}

		contentRect = scrollRect.content;
		if (contentRect == null)
		{
			Debug.LogError("ScrollRect content is not assigned.");
		}
	}

	void Update()
	{
		if (contentRect == null) return;

		float scroll = Input.GetAxis("Mouse ScrollWheel");

		if (scroll != 0.0f)
		{
			float currentScale = contentRect.localScale.x;
			float newScale = currentScale + scroll * m_zoomSpeed;
			newScale = Mathf.Clamp(newScale, m_minZoom, m_maxZoom);
			contentRect.localScale = new Vector3(newScale, newScale, 1f);
		}
	}

	public void OpenMinimap(GameDataManager argDataManager, MainUIManager argUIManager)
	{
		m_gameDataManager = argDataManager;
		m_mainUIManager = argUIManager;

		this.gameObject.SetActive(true);
		GameObjectUtils.ClearChildren(m_minimapScrollViewContentTrans);
		GenerateMinimapTiles();
	}

	public void CloseMinimap()
	{
		this.gameObject.SetActive(false);
		GameObjectUtils.ClearChildren(m_minimapScrollViewContentTrans);
	}

	public void OpenMinimapDetailPanel(TileMapData argTileMapData, TileMapState argTileMapState)
    {
		m_minimapDetailPanel.Open(argTileMapData, argTileMapState, m_mainUIManager);
    }

	private void GenerateMinimapTiles()
	{
		if (m_gameDataManager == null)
		{
			Debug.LogError("GameDataManager is null.");
			return;
		}

		GameObjectUtils.ClearChildren(m_minimapScrollViewContentTrans);

		Vector2Int mapSize = m_gameDataManager.TileMapManager.GetTileMapSize();

		if (mapSize == Vector2Int.zero)
		{
			Debug.LogError("Tilemap has not been generated.");
			return;
		}

		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				TileMapState tileState = m_gameDataManager.TileMapManager.GetTileMapState(x, y);

				if (tileState != null)
				{
					TileMapData tileData = m_gameDataManager.TileMapManager.GetTileMapData(tileState.m_terrainType);

					if (tileData != null)
					{
						GameObject tileBtn = Instantiate(m_minimapTileBtnPrefeb, m_minimapScrollViewContentTrans);
						MinimapBtn minimapBtn = tileBtn.GetComponent<MinimapBtn>();

						if (minimapBtn != null)
						{
							minimapBtn.Init(tileData, tileState, this);
						}
					}
					else
					{
						Debug.LogWarning($"Tile data not found: {tileState.m_terrainType}");
					}
				}
				else
				{
					Debug.LogWarning($"Could not get tile state: ({x}, {y})");
				}
			}
		}
	}
}
