using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestPanel : MonoBehaviour, IUIPanel
{
    [SerializeField]
    Transform m_factionLikeScrollViewContentTrans = null;
    [SerializeField]
    GameObject m_factionLikeImagePrefeb = null;
    [SerializeField]
    RequestDetailPanel m_requestDetailPanel = null;

    private GameDataManager m_gameDataManager = null;
    private List<FactionLikeImage> m_factionLikeImageList = new List<FactionLikeImage>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;
        gameObject.SetActive(true);
        ResetFactionLikeImage();
    }

    public void OnClose()
    {

    }

    public void ResetFactionLikeImage()
    {
        if(m_factionLikeImageList.Count != m_gameDataManager.FactionEntryDict.Count)
        {
            foreach (Transform item in m_factionLikeScrollViewContentTrans)
            {
                Destroy(item.gameObject);
            }

            foreach(KeyValuePair<FactionType, FactionEntry> item in m_gameDataManager.FactionEntryDict)
            {
                FactionEntry _tmpEntry = item.Value;

                if (_tmpEntry.m_data.m_factionType == FactionType.None)
                {
                    Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
                    continue;
                }

                FactionLikeImage _tmp = Instantiate(
                    m_factionLikeImagePrefeb.GetComponent<FactionLikeImage>(), m_factionLikeScrollViewContentTrans);

                m_factionLikeImageList.Add(_tmp);

                _tmp.Setting(_tmpEntry.m_data.m_factionType, _tmpEntry.m_data.m_icon, _tmpEntry.m_data.m_name, _tmpEntry.m_state.m_like);
            }
        }
        else
        {
            foreach(FactionLikeImage item in m_factionLikeImageList)
            {
                if (item.m_factionType == FactionType.None)
                {
                    Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
                    continue;
                }

                item.SetLikeText(m_gameDataManager.FactionEntryDict[item.m_factionType].m_state.m_like);
            }
        }
    }

    /// <summary>
    /// 의뢰 패널 선택
    /// </summary>
    /// <param name="argPanelIndex"></param>
    /// 0 = 수락 가능
    /// 1 = 의뢰중
    public void SelectResearchContent(int argPanelIndex)
    {
        foreach (Transform item in m_factionLikeScrollViewContentTrans)
        {
            Destroy(item.gameObject);
        }

        foreach (KeyValuePair<string, ResearchEntry> item in m_gameDataManager.CommonResearchEntryDict)
        {
            switch (argPanelIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                default:
                    Debug.LogError(ExceptionMessages.ErrorInvalidResearchInfo);
                    break;
            }

        }
    }

    public void OpenRequestDetailPanel(string argRequestCode)
    {
        m_requestDetailPanel.OnOpen(m_gameDataManager.RequestDataDict[argRequestCode]);
    }
}
