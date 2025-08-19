using UnityEngine;

/// 공격 전달 방식
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
                    // 안전망: 발사체 설정 누락 시 즉시타격으로 대체
                    Debug.LogWarning($"[{contextName}] Projectile/Origin 누락 → Instant로 대체");
                    ApplyDamage(attacker, targetGO, damage);
                    return;
                }

                var proj = Object.Instantiate(projectilePrefab, origin.position, origin.rotation);

                // 표준 인터페이스 우선
                var std = proj.GetComponent<IProjectileController>();
                if (std != null) { std.Initialize(attacker, targetGO, damage); return; }

                var c1 = proj.GetComponent<ProjectileContoller>();
                var c2 = proj.GetComponent<ProjectileContoller>(); 
                if (c1 != null) c1.Initialize(attacker, targetGO, damage);
                else if (c2 != null) c2.Initialize(attacker, targetGO, damage);
                else
                {
                    Debug.LogError($"[{contextName}] ProjectileController 없음 → Instant로 대체");
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
