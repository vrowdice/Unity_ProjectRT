using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestPanel : BasePanel
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

    private List<FactionLikeImage> m_factionLikeImageList = new();

    private List<RequestPanel> m_acceptableRequestPanelList = new();
    private List<RequestPanel> m_inProgressRequestPanelList = new();

    private bool m_isAcceptableBtnView = true;

    private const int m_requestBuildingCode = 10002;

    protected override void OnPanelOpen()
    {
        // 패널 설정
        SetPanelName("Request");
        SetBuildingLevel(m_requestBuildingCode);

        InitializeRequestPanel();
    }

    /// <summary>
    /// 요청 패널 초기화
    /// </summary>
    private void InitializeRequestPanel()
    {
        UpdateFactionLikeImage();
        UpdateRequestBtns();
    }

    private void UpdateFactionLikeImage()
    {
        if (m_factionLikeScrollViewContentTrans == null)
        {
            Debug.LogError("Faction like scroll view content transform is null!");
            return;
        }

        if (m_factionLikeImagePrefeb == null)
        {
            Debug.LogError("Faction like image prefab is null!");
            return;
        }

        if (m_factionLikeImageList.Count != m_gameDataManager.FactionEntryDict.Count)
        {
            CreateFactionLikeImages();
        }
        else
        {
            UpdateExistingFactionLikeImages();
        }
    }

    /// <summary>
    /// 팩션 호감도 이미지들 생성
    /// </summary>
    private void CreateFactionLikeImages()
    {
        GameObjectUtils.ClearChildren(m_factionLikeScrollViewContentTrans);
        m_factionLikeImageList.Clear();

        foreach (KeyValuePair<FactionType.TYPE, FactionEntry> item in m_gameDataManager.FactionEntryDict)
        {
            FactionEntry _tmpEntry = item.Value;

            if (_tmpEntry.m_data.m_factionType == FactionType.TYPE.None || _tmpEntry.m_state.m_like <= 0)
            {
                continue;
            }

            FactionLikeImage _tmp = Instantiate(
                m_factionLikeImagePrefeb.GetComponent<FactionLikeImage>(), m_factionLikeScrollViewContentTrans);

            if (_tmp != null)
            {
                m_factionLikeImageList.Add(_tmp);
                _tmp.Initialize(_tmpEntry.m_data.m_factionType, _tmpEntry.m_data.m_icon, _tmpEntry.m_data.m_name, _tmpEntry.m_state.m_like);
            }
        }
    }

    /// <summary>
    /// 기존 팩션 호감도 이미지들 업데이트
    /// </summary>
    private void UpdateExistingFactionLikeImages()
    {
        foreach (FactionLikeImage item in m_factionLikeImageList)
        {
            if (item == null) continue;

            if (item.m_factionType == FactionType.TYPE.None)
            {
                Debug.LogError(ExceptionMessages.ErrorValueNotAllowed);
                continue;
            }

            item.SetLikeText(m_gameDataManager.FactionEntryDict[item.m_factionType].m_state.m_like);
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
                argIsAcceptable,  // 올바른 값 전달
                this,
                m_gameDataManager.GetFactionEntry(_state.m_factionType).m_data.m_icon,
                m_gameDataManager.GetRequestIcon(_state.m_requestType),
                _state);
        }
    }

    public void UpdateRequestBtns()
    {
        UpdateRequestBtns(m_isAcceptableBtnView);
    }

    /// <summary>
    /// 요청 패널 선택
    /// </summary>
    /// <param name="argPanelIndex"></param>
    /// 0 = 수락 가능
    /// 1 = 요청중
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

    public void CompleteRequest(RequestState argState)
    {
        Debug.Log($"Completing request: {argState.m_title}");
        
        // 요청 완료 처리
        // 보상 지급
        if (GameManager.Instance != null)
        {
            // 리소스 보상 지급
            foreach (ResourceAmount resourceReward in argState.m_resourceRewardList)
            {
                bool success = GameManager.Instance.TryChangeResource(resourceReward.m_type, resourceReward.m_amount);
            }

            // 토큰 보상 지급 (토큰 시스템이 구현되면 추가)
            // foreach (TokenAmount tokenReward in argState.m_tokenRewardList)
            // {
            //     // 토큰 지급 로직
            // }
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }

        // 팩션 호감도 증가
        if (argState.m_factionType != FactionType.TYPE.None)
        {
            var factionEntry = m_gameDataManager.GetFactionEntry(argState.m_factionType);
            if (factionEntry != null)
            {
                int oldLike = factionEntry.m_state.m_like;
                factionEntry.m_state.m_like += argState.m_factionAddLike;
                Debug.Log($"Faction {argState.m_factionType} like increased: {oldLike} -> {factionEntry.m_state.m_like} (+{argState.m_factionAddLike})");
            }
            else
            {
                Debug.LogError($"Faction entry not found for {argState.m_factionType}");
            }
        }

        // 요청 목록에서 제거
        bool removed = m_gameDataManager.AcceptedRequestList.Remove(argState);

        InitializeRequestPanel();
        m_mainUIManager.UpdateAllMainText();
    }

    public void CancelRequest(RequestState argState)
    {
        // 요청 취소 처리
        // 요청 목록에서 제거
        m_gameDataManager.AcceptedRequestList.Remove(argState);
        UpdateRequestBtns();
        m_mainUIManager.UpdateAllMainText();
    }

    public void OpenRequestDetailPanel(bool argIsAcceptable, RequestState argRequestIndex)
    {
        m_requestDetailPanel.OnOpen(m_mainUIManager ,this, argIsAcceptable, argRequestIndex);
    }
}
