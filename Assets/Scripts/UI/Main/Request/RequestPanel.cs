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
    private List<FactionLikeImage> m_factionLikeImageList = new();

    private List<RequestPanel> m_acceptableRequestPanelList = new();
    private List<RequestPanel> m_inProgressRequestPanelList = new();

    private bool m_isAcceptableBtnView = true;

    public GameDataManager GameDataManager => m_gameDataManager;
    public MainUIManager MainUIManager => m_mainUIManager;

    private void UpdateFactionLikeImage()
    {
        if (m_factionLikeImageList.Count != m_gameDataManager.FactionEntryDict.Count)
        {
            GameObjectUtils.ClearChildren(m_factionLikeScrollViewContentTrans);

            foreach (KeyValuePair<FactionType.TYPE, FactionEntry> item in m_gameDataManager.FactionEntryDict)
            {
                FactionEntry _tmpEntry = item.Value;

                if (_tmpEntry.m_data.m_factionType == FactionType.TYPE.None || !_tmpEntry.m_state.m_have)
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
                if (item.m_factionType == FactionType.TYPE.None)
                {
                    Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
                    continue;
                }

                item.SetLikeText(m_gameDataManager.FactionEntryDict[item.m_factionType].m_state.m_like);
            }
        }
    }

    private void UpdateRequestBtns(bool argIsAcceptable)
    {
        m_isAcceptableBtnView = argIsAcceptable;

        List<RequestState> _requestList;

        if (argIsAcceptable == true)
        {
            _requestList = m_gameDataManager.AcceptableRequestList;
        }
        else
        {
            _requestList = m_gameDataManager.AcceptedRequestList;
        }

        GameObjectUtils.ClearChildren(m_requestBtnScrollViewContentTrans);

        for (int i = 0; i < _requestList.Count; i++)
        {
            RequestState _state = _requestList[i];

            GameObject _btnObj = Instantiate(m_requestBtnPrefeb, m_requestBtnScrollViewContentTrans);
            RequestBtn _requestBtn = _btnObj.GetComponent<RequestBtn>();
            _requestBtn.Initialize(
                true,
                this,
                m_gameDataManager.GetFactionEntry(_state.m_factionType).m_data.m_icon,
                m_gameDataManager.GetRequestIcon(_state.m_requestType),
                _state);
        }
    }

    public void OnOpen(GameDataManager argDataManager, MainUIManager argUIManager)
    {
        m_gameDataManager = argDataManager;
        m_mainUIManager = argUIManager;

        gameObject.SetActive(true);
        UpdateFactionLikeImage();
        UpdateRequestBtns();
    }

    public void OnClose()
    {

    }

    public void UpdateRequestBtns()
    {
        UpdateRequestBtns(m_isAcceptableBtnView);
    }

    /// <summary>
    /// 의뢰 패널 선택
    /// </summary>
    /// <param name="argPanelIndex"></param>
    /// 0 = 수락 가능
    /// 1 = 의뢰중
    public void SelectRequestContent(int argPanelIndex)
    {
        GameObjectUtils.ClearChildren(m_factionLikeScrollViewContentTrans);

        switch (argPanelIndex)
        {
            case 0:
                UpdateRequestBtns(true);
                break;
            case 1:
                UpdateRequestBtns(false);
                break;
            default:
                Debug.LogError(ExceptionMessages.ErrorInvalidResearchInfo);
                break;
        }
    }

    public void AcceptRequest(RequestState argState)
    {
        m_gameDataManager.AcceptRequest(argState);
        UpdateRequestBtns();

        m_mainUIManager.UpdateAllMainText();
    }

    public void OpenRequestDetailPanel(bool argIsAcceptable, RequestState argRequestIndex)
    {
        m_requestDetailPanel.OnOpen(this, argIsAcceptable, argRequestIndex);
    }
}
