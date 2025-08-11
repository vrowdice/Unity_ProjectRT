public class CompletionConditionType
{
    public enum TYPE
    {
        None,
        DefeatEnemyCount,       // 적 N마리 처치 (전투)
        OccupyTerritory,        // 특정 영역 점령 (영역 점령)
        ProduceSpecificResource, // 특정 자원 생산 (생산 증강)
        DeliverSpecificResource, // 특정 자원 전달 (자원 전달)
        MaintainProduction,     // 생산량 유지 성공 (생산 증강)
    }
}
