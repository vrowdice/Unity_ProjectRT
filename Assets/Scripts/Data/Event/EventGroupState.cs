/// <summary>
/// �׷츶�� �̺�Ʈ �߻� Ȯ���� ����
/// �ʱ�ȭ �� �� �׷츶�� �ش� Ŭ���� ���� ��
/// ��¥�� ���� ������ Ȯ�� ����
/// </summary>
public class EventGroupState
{
    public int m_eventGroupKey;
    public float m_percent;

    public EventGroupState(int argEventGroupKey, float argPercent)
    {
        m_eventGroupKey = argEventGroupKey;
        m_percent = argPercent;
    }
}
