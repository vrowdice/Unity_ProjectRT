using UnityEngine;

public enum StatusType { None = 0, MarkHunted = 1 /* 확장 */ }

public interface IStatusController
{
    bool Has(StatusType type);
    void Apply(StatusType type, float duration, UnitBase applier);
    void Remove(StatusType type);
}

// 스킬/버프/연계 효과를 중앙에서 연결하는 브로커
public class BattleEventBroker : MonoBehaviour
{
    public void Raise(string eventName, UnitBase caster, UnitBase victim)
    {
        if (eventName == "MarkHunt")
        {
            Debug.Log($"[BattleEventBroker] MarkHunt 효과 적용: {caster.UnitName}");
        }
    }
}
