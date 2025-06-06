using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestPanel : MonoBehaviour, IUIPanel
{
    [SerializeField]
    Transform m_factionLikeScrollViewContentTrans = null;
    [SerializeField]
    Transform m_requestBtnScrollViewContentTrans = null;
    [SerializeField]
    GameObject m_factionLikeImagePrefeb = null;
    [SerializeField]
    GameObject m_requestBtnPrefeb = null;
    [SerializeField]
    RequestDetailPanel m_requestDetailPanel = null;

    private GameDataManager m_gameDataManager = null;
    private MainUIManager m_mainUIManager = null;
    private List<FactionLikeImage> m_factionLikeImageList = new List<FactionLikeImage>();

    private List<RequestPanel> m_acceptableRequestPanelList = new List<RequestPanel>();
    private List<RequestPanel> m_inProgressRequestPanelList = new List<RequestPanel>();

    public MainUIManager MainUIManager => m_mainUIManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(GameDataManager argDataManager, MainUIManager argUIManager)
    {
        m_gameDataManager = argDataManager;
        m_mainUIManager = argUIManager;
        gameObject.SetActive(true);
        ResetFactionLikeImage();
        SetRequestBtns(true);
    }

    public void OnClose()
    {

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

    public void OpenRequestDetailPanel(int argRequestIndex)
    {
        m_requestDetailPanel.OnOpen();
    }

    void ResetFactionLikeImage()
    {
        if (m_factionLikeImageList.Count != m_gameDataManager.FactionEntryDict.Count)
        {
            UIUtils.ClearChildren(m_factionLikeScrollViewContentTrans);

            foreach (KeyValuePair<FactionType, FactionEntry> item in m_gameDataManager.FactionEntryDict)
            {
                FactionEntry _tmpEntry = item.Value;

                if (_tmpEntry.m_data.m_factionType == FactionType.None || !_tmpEntry.m_state.m_have)
                {
                    continue;
                }

                FactionLikeImage _tmp = Instantiate(
                    m_factionLikeImagePrefeb.GetComponent<FactionLikeImage>(), m_factionLikeScrollViewContentTrans);

                m_factionLikeImageList.Add(_tmp);

                _tmp.Initialize(_tmpEntry.m_data.m_factionType, _tmpEntry.m_data.m_icon, _tmpEntry.m_data.m_name, _tmpEntry.m_state.m_like);
            }
        }
        else
        {
            foreach (FactionLikeImage item in m_factionLikeImageList)
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

    void SetRequestBtns(bool argIsAcceptable)
    {
        List<RequestState> _requestList = new List<RequestState>();

        if (argIsAcceptable == true)
        {
            _requestList = m_gameDataManager.AcceptableRequestList;
        }
        else
        {
            _requestList = m_gameDataManager.InProgressRequestList;
        }

        UIUtils.ClearChildren(m_requestBtnScrollViewContentTrans);

        for (int i = 0; i < _requestList.Count; i++)
        {
            RequestState _state = _requestList[i];

            GameObject _btnObj = Instantiate(m_requestBtnPrefeb, m_requestBtnScrollViewContentTrans);
            RequestBtn _requestBtn = _btnObj.GetComponent<RequestBtn>();
            _requestBtn.Initialize(
                _state.m_resourceReward,
                this,
                m_gameDataManager.FactionEntryDict[_state.m_factionType].m_data.m_icon,
                i,
                _state.m_factionAddLike,
                _state.m_factionType.ToString(),
                _state.m_requestType.ToString());
        }
    }
}
