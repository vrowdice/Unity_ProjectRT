/// <summary>
/// 그룹마다 이벤트 발생 확률을 관리
/// 초기화 시 각 그룹마다 해당 클래스 생성 후
/// 날짜가 지날 때마다 확률 갱신
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
