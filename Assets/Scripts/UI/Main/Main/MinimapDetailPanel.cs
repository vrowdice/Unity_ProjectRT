using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinimapDetailPanel : MonoBehaviour
{
	[Header("MinimapDetailPanel")]
	[SerializeField]
	TextMeshProUGUI m_titleText = null;
	[SerializeField]
	Transform m_resourceContentTrans = null;
	private IUIManager m_mainUIManager = null;

	public void Open(TileMapData argTileMapData, TileMapState argTileMapState, IUIManager argUIManager)
    {
		m_mainUIManager = argUIManager;

		m_titleText.text = argTileMapData.m_terrainType.ToString();

		GameObjectUtils.ClearChildren(m_resourceContentTrans);

		var resourceList = argTileMapState?.m_resources?.ToList();
		if (resourceList != null)
		{
			foreach (var argResourceAmount in resourceList)
			{
				GameObject resourceObj = Instantiate(argUIManager.ResourceIconTextPrefeb, m_resourceContentTrans);
				resourceObj.GetComponent<ResourceIconText>().InitializeMainText(argResourceAmount.m_type, argResourceAmount.m_amount);
			}
		}

		this.gameObject.SetActive(true);
    }
}
