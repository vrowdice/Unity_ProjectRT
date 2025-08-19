using UnityEngine;

public enum StatusType { None = 0, MarkHunted = 1 /* Ȯ�� */ }

public interface IStatusController
{
    bool Has(StatusType type);
    void Apply(StatusType type, float duration, UnitBase applier);
    void Remove(StatusType type);
}

// ��ų/����/���� ȿ���� �߾ӿ��� �����ϴ� ���Ŀ
public class BattleEventBroker : MonoBehaviour
{
    public void Raise(string eventName, UnitBase caster, UnitBase victim)
    {
        if (eventName == "MarkHunt")
        {
            Debug.Log($"[BattleEventBroker] MarkHunt ȿ�� ����: {caster.UnitName}");
        }
    }
}
