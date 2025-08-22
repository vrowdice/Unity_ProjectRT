using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConditionPanelText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_text = null;
    [SerializeField]
    Button m_toObjBtn = null;

    private RequestCompleteCondition m_condition;
    private RequestType.TYPE m_requestType;
    private bool m_isAcceptable;

    public void Init(RequestCompleteCondition condition, RequestType.TYPE requestType, bool isAcceptable)
    {
        m_condition = condition;
        m_requestType = requestType;
        m_isAcceptable = isAcceptable;
        
        // 조건 텍스트 생성
        string conditionText = GetConditionText(condition, requestType);
        
        // 진행 상황 표시 (N/N) 형태
        string progressText = GetProgressText(condition, requestType);
        
        // 전체 텍스트 설정
        m_text.text = $"{conditionText} {progressText}";
        
        // 이동 버튼은 일단 비활성화 (보류)
        if (m_toObjBtn != null)
        {
            m_toObjBtn.gameObject.SetActive(false);
        }
    }

    private string GetConditionText(RequestCompleteCondition condition, RequestType.TYPE requestType)
    {
        // RequestType에 따른 조건 텍스트 반환
        switch (requestType)
        {
            case RequestType.TYPE.Battle:
                return "Win Battle";
            case RequestType.TYPE.Conquest:
                return "Conquer Territory";
            case RequestType.TYPE.Production:
                ResourceType.TYPE resourceType = (ResourceType.TYPE)condition.m_completeTargetInfo;
                return $"Produce {resourceType}";
            case RequestType.TYPE.Stockpile:
                ResourceType.TYPE stockpileResourceType = (ResourceType.TYPE)condition.m_completeTargetInfo;
                return $"Stockpile {stockpileResourceType}";
            default:
                return "Complete Condition";
        }
    }

    private string GetProgressText(RequestCompleteCondition condition, RequestType.TYPE requestType)
    {
        int currentValue = condition.m_nowCompleteValue;
        int targetValue = condition.m_completeValue;

        // 자원 관련 조건의 경우 GameManager에서 현재 자원량을 참조
        if (requestType == RequestType.TYPE.Production || requestType == RequestType.TYPE.Stockpile)
        {
            if (GameManager.Instance != null)
            {
                ResourceType.TYPE resourceType = (ResourceType.TYPE)condition.m_completeTargetInfo;
                currentValue = (int)GameManager.Instance.GetResource(resourceType);
            }
        }

        return $"({currentValue}/{targetValue})";
    }

    // 이동 버튼 클릭 시 호출될 메서드 (보류)
    public void OnToObjBtnClick()
    {
        // 해당 조건에 관련된 요소로 이동하는 로직
        // 현재는 보류 상태
        Debug.Log("이동 버튼 클릭 - 기능 보류");
    }
}
