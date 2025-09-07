using UnityEngine;

public class UnitImpactEmitter : MonoBehaviour
{

    public void Emit(ImpactEventType type, UnitBase source, GameObject target = null, float value = 0f, string extra = null)
    {
        var receivers = GetComponents<IImpactReceiver>();
        foreach (var r in receivers)
            r.OnImpact(type, source, target, value, extra);
    }

    public static void Emit(GameObject go, ImpactEventType type, UnitBase source, GameObject target = null, float value = 0f, string extra = null)
    {
        if (!go) return;
        var emitter = go.GetComponentInParent<UnitImpactEmitter>();
        if (emitter != null) emitter.Emit(type, source, target, value, extra);
    }
}
