using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestCompleteCondition
{
    public int m_completeTargetInfo;     // �� �ڵ�, �ڿ� Ÿ�� �ε��� ��
    public int m_nowCompleteValue; // �Ƿ� �Ϸ� ���� �޼� ��ġ
    public int m_completeValue;   // �Ƿ� �Ϸ� ����
    public int m_nowTime; // ���� ���� �ð�
    public int m_limTime; // ���� �ð�

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
                m_completeValue = 3; //�Ÿ� ����
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
