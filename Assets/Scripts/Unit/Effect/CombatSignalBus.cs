using System;

public static class CombatSignalBus
{
    public static event Action<CombatSignal> OnSignal;

    public static void Publish(CombatSignal s) => OnSignal?.Invoke(s);
}
