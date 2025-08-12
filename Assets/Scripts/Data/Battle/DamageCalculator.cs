using UnityEngine;

// 피해량 계산
public static class DamageCalculator
{
    // 이 숫자가 높을수록 방어력 효율이 증가
    private const float DefenseFactor = 20.0f;

    public static float CalculateDamage(float rawDamage, float defensePower)
    {
        float reduction = defensePower / (defensePower + DefenseFactor);
        float finalDamage = rawDamage * (1.0f - reduction);

        // 최소 피해량을 없애고 싶다면 Mathf.Max(0.0f, finalDamage); 로 변경
        return Mathf.Max(1.0f, finalDamage);
    }
}