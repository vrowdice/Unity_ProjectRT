using UnityEngine;

/// ���� ���� ���
public enum HitDeliveryStrategy
{
    Instant,    
    Projectile  
}

public interface IProjectileController
{
    void Initialize(UnitBase attacker, GameObject target, float damage);
}

public static class HitDeliverer
{
    public static void Deliver(
        UnitBase attacker,
        GameObject targetGO,
        float damage,
        HitDeliveryStrategy strategy,
        GameObject projectilePrefab = null,
        Transform origin = null,
        string contextName = "HitDeliverer")
    {
        if (attacker == null || targetGO == null || !targetGO.activeSelf) return;

        switch (strategy)
        {
            case HitDeliveryStrategy.Instant:
                ApplyDamage(attacker, targetGO, damage);
                break;

            case HitDeliveryStrategy.Projectile:
                if (projectilePrefab == null || origin == null)
                {
                    // ������: �߻�ü ���� ���� �� ���Ÿ������ ��ü
                    Debug.LogWarning($"[{contextName}] Projectile/Origin ���� �� Instant�� ��ü");
                    ApplyDamage(attacker, targetGO, damage);
                    return;
                }

                var proj = Object.Instantiate(projectilePrefab, origin.position, origin.rotation);

                // ǥ�� �������̽� �켱
                var std = proj.GetComponent<IProjectileController>();
                if (std != null) { std.Initialize(attacker, targetGO, damage); return; }

                var c1 = proj.GetComponent<ProjectileContoller>();
                var c2 = proj.GetComponent<ProjectileContoller>(); 
                if (c1 != null) c1.Initialize(attacker, targetGO, damage);
                else if (c2 != null) c2.Initialize(attacker, targetGO, damage);
                else
                {
                    Debug.LogError($"[{contextName}] ProjectileController ���� �� Instant�� ��ü");
                    ApplyDamage(attacker, targetGO, damage);
                    Object.Destroy(proj);
                }
                break;
        }
    }

    private static void ApplyDamage(UnitBase attacker, GameObject targetGO, float damage)
    {
        var victim = targetGO.GetComponent<UnitBase>();
        if (victim == null || victim.IsDead) return;

        victim.TakeDamage(damage);
        UnitImpactEmitter.Emit(attacker.gameObject, ImpactEventType.BasicAttackHit,
                               attacker, targetGO, damage, "Basic/Skill");
    }
}
