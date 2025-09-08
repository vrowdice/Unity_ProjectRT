using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectDifficultyPanel : MonoBehaviour, IUIPanel
{

    [SerializeField]
    GameObject m_difficultyBtnPrefab;
    
    [SerializeField]
    Transform m_selectDifficultyScrollViewContent;
    // Start is called before the first frame update

    public void OnOpen(GameDataManager gameDataManager, MainUIManager mainUIManager)
    {
        if (m_difficultyBtnPrefab == null || m_selectDifficultyScrollViewContent == null)
        {
            Debug.LogError("SelectDifficultyPanel: References are not assigned.");
            return;
        }

        // 필요 시 중복 생성 방지
        foreach (Transform child in m_selectDifficultyScrollViewContent)
            Destroy(child.gameObject);

        foreach (var item in EnumUtils.GetAllEnumValues<BalanceType.TYPE>())
        {
            var btnGO = Instantiate(m_difficultyBtnPrefab, m_selectDifficultyScrollViewContent);
            if (btnGO.TryGetComponent<DifficultyBtn>(out var btn))
                btn.Init(item);
            else
                Debug.LogError("SelectDifficultyPanel: DifficultyBtn component missing on prefab.");
        }
    }

    public void OnClose()
    {
        foreach (Transform child in m_selectDifficultyScrollViewContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void NextBtn()
    {

    }
}
