using UnityEngine;

public struct UnitStatSnapshot
{
    public string UnitName;
    public float AttackPower, DefensePower, MaxHealth, MoveSpeed, AttackRange, EnemySearchRange;
    public float AttackSpeed, AttackCycleSec, AttackActiveSec, AttackRecoverySec;
    public int AttackCount;
    public float DamageCoefficient;
    public float MaxMana, CurrentMana, ManaRecoveryOnBasicAttack, ManaRecoveryPerSecond;

    public void RecalcTempoByDelayRatio(float delayRatio)
    {
        if (AttackSpeed > 0.0f)
        {
            AttackCycleSec = 1.0f / AttackSpeed;
            AttackRecoverySec = AttackCycleSec * Mathf.Clamp01(delayRatio);
            AttackActiveSec = Mathf.Max(0f, AttackCycleSec - AttackRecoverySec);
        }
        else AttackCycleSec = AttackActiveSec = AttackRecoverySec = 0.0f;
    }

    public void Clamp()
    {
        MaxHealth = Mathf.Max(1.0f, MaxHealth);
        MaxMana = Mathf.Max(0.0f, MaxMana);
        CurrentMana = Mathf.Clamp(CurrentMana, 0.0f, MaxMana);
    }
}
