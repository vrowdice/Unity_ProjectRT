using UnityEngine;

// ���ط� ���
public static class DamageCalculator
{
    // �� ���ڰ� �������� ���� ȿ���� ����
    private const float DefenseFactor = 20.0f;

    public static float CalculateDamage(float rawDamage, float defensePower)
    {
        float reduction = defensePower / (defensePower + DefenseFactor);
        float finalDamage = rawDamage * (1.0f - reduction);

        // �ּ� ���ط��� ���ְ� �ʹٸ� Mathf.Max(0.0f, finalDamage); �� ����
        return Mathf.Max(1.0f, finalDamage);
    }
}