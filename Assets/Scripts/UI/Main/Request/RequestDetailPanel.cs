using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequestDetailPanel : MonoBehaviour
{
    [SerializeField]
    Transform m_conditionContentTrans = null;
    [SerializeField]
    Transform m_resourceContentTrans = null;
    [SerializeField]
    Transform m_tokenContentTrans = null;
    [SerializeField]
    TextMeshProUGUI m_titleText = null;
    [SerializeField]
    Button m_acceptBtn = null;
    [SerializeField]
    Button m_refuseBtn = null;
    [SerializeField]
    TextMeshProUGUI m_acceptBtnText = null;
    [SerializeField]
    TextMeshProUGUI m_refuseBtnText = null;
    // [SerializeField]
    // TextMeshProUGUI m_descriptionText = null;

    private MainUIManager m_mainUIManager;
    private RequestPanel m_requestPanel;
    private RequestState m_nowRequestState;
    private bool m_isAcceptable;

    public void OnOpen(MainUIManager argMainUIManager, RequestPanel argRequestPanel, bool argIsAcceptable, RequestState argState)
    {
        m_mainUIManager = argMainUIManager;
        m_requestPanel = argRequestPanel;
        m_nowRequestState = argState;
        m_isAcceptable = argIsAcceptable;

        // UI 버튼 텍스트 설정
        if (m_isAcceptable)
        {
            m_acceptBtnText.text = "Accept";
            m_refuseBtnText.text = "Refuse";
            m_acceptBtn.interactable = true;
        }
        else
        {
            m_acceptBtnText.text = "Complete";
            m_refuseBtnText.text = "Cancel";
            
            // 조건이 달성되었는지 확인하여 완료 버튼 활성화/비활성화
            bool canComplete = IsRequestConditionMet(argState);
            m_acceptBtn.interactable = canComplete;
        }

        GameObjectUtils.ClearChildren(m_conditionContentTrans);
        
        // RequestCompleteCondition은 단일 객체이므로 foreach 대신 직접 처리
        if (argState.m_requestCompleteCondition != null)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ConditionPanelTextPrefeb, m_conditionContentTrans);
            _obj.GetComponent<ConditionPanelText>().Init(argState.m_requestCompleteCondition, argState.m_requestType, m_isAcceptable);
        }

        GameObjectUtils.ClearChildren(m_resourceContentTrans);
        foreach (ResourceAmount item in argState.m_resourceRewardList)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ResourceIconTextPrefeb, m_resourceContentTrans);

            _obj.GetComponent<ResourceIconText>().InitializeMainText(item.m_type, item.m_amount);
        }

        GameObjectUtils.ClearChildren(m_tokenContentTrans);
        foreach (TokenAmount item in argState.m_tokenRewardList)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ResourceIconTextPrefeb, m_tokenContentTrans);

            _obj.GetComponent<ResourceIconText>().InitializeMainText(item.m_type, item.m_amount);
        }

        m_titleText.text = argState.m_title;
        // m_descriptionText.text = argState.m_description;

        gameObject.SetActive(true);
    }

    public void ClickAcceptBtn()
    {
        if (m_isAcceptable)
        {
            m_requestPanel.AcceptRequest(m_nowRequestState);
        }
        else
        {
            // 완료 버튼 클릭 시 처리
            m_requestPanel.CompleteRequest(m_nowRequestState);
        }

        gameObject.SetActive(false);
    }

    public void ClickRefuseBtn()
    {
        if (m_isAcceptable)
        {
            // 거절 버튼 클릭 시 처리 (아직 구현되지 않음)
            Debug.Log("Refuse button clicked - functionality not implemented yet");
        }
        else
        {
            // 취소 버튼 클릭 시 처리
            m_requestPanel.CancelRequest(m_nowRequestState);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 요청 조건이 달성되었는지 확인
    /// </summary>
    /// <param name="argState">확인할 요청 상태</param>
    /// <returns>조건 달성 여부</returns>
    private bool IsRequestConditionMet(RequestState argState)
    {
        if (argState.m_requestCompleteCondition == null)
        {
            return false;
        }

        var condition = argState.m_requestCompleteCondition;
        
        switch (argState.m_requestType)
        {
            case RequestType.TYPE.Battle:
                // 전투 승리 조건 - 현재는 단순히 목표값 확인
                return condition.m_nowCompleteValue >= condition.m_completeValue;
                
            case RequestType.TYPE.Conquest:
                // 영토 정복 조건 - 현재는 단순히 목표값 확인
                return condition.m_nowCompleteValue >= condition.m_completeValue;
                
            case RequestType.TYPE.Production:
                // 특정 자원 생산 조건 - 현재 보유량 확인
                if (GameManager.Instance != null)
                {
                    ResourceType.TYPE resourceType = (ResourceType.TYPE)condition.m_completeTargetInfo;
                    long currentAmount = GameManager.Instance.GetResource(resourceType);
                    return currentAmount >= condition.m_completeValue;
                }
                return false;
                
            case RequestType.TYPE.Stockpile:
                // 특정 자원 보유 조건 - 현재 보유량 확인
                if (GameManager.Instance != null)
                {
                    ResourceType.TYPE resourceType = (ResourceType.TYPE)condition.m_completeTargetInfo;
                    long currentAmount = GameManager.Instance.GetResource(resourceType);
                    return currentAmount >= condition.m_completeValue;
                }
                return false;
                
            default:
                return false;
        }
    }
}
