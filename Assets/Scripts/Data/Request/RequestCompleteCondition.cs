using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestCompleteCondition
{
    public int m_completeTargetInfo;     // 적 코드, 자원 타입 인덱스 등
    public int m_nowCompleteValue; // 의뢰 완료 조건 달성 수치
    public int m_completeValue;   // 의뢰 완료 조건
    public int m_nowTime; // 현재 지난 시간
    public int m_limTime; // 제한 시간

    public RequestCompleteCondition(RequestType.TYPE argType, float argMainMul, double argDateMul)
    {
        m_nowCompleteValue = 0;
        m_nowTime = 0;

        switch (argType)
        {
            case RequestType.TYPE.Hunt:
                m_completeTargetInfo = 0;
                m_completeValue = (int)((15 * argDateMul) * argMainMul);
                m_limTime = Mathf.Max((int)((5 + argDateMul) - argMainMul), 5);
                break;
            case RequestType.TYPE.Conquest:
                m_completeTargetInfo = 0;
                m_completeValue = 3; //거리 계산식
                m_limTime = Mathf.Max((int)(((3 + m_completeValue) + argDateMul) - argMainMul), 5);
                break;
            case RequestType.TYPE.Production:
                m_completeTargetInfo = EnumUtils.GetRandomEnumValueInt<ResourceType.TYPE>();
                m_completeValue = (int)((30 * argDateMul) * argMainMul);
                m_limTime = Mathf.Max((int)((6 * argDateMul) - argMainMul / 2), 5);
                break;
            case RequestType.TYPE.Stockpile:
                m_completeTargetInfo = EnumUtils.GetRandomEnumValueInt<ResourceType.TYPE>();
                m_completeValue = (int)((10 * argDateMul) * argMainMul);
                m_limTime = Mathf.Max((int)((5 * argDateMul) - argMainMul / 2), 5);
                break;
            default:
                break;
        }
    }

    public void MaintainProduction(float argMainMul, double argDateMul)
    {
        m_nowTime = 0;
        m_limTime = Mathf.Max((int)((3 * argDateMul) - argMainMul), 2);
    }
}
