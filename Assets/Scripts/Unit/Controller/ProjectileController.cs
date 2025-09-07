using UnityEngine;

public class ProjectileContoller : MonoBehaviour
{
    private UnitBase owner;
    private GameObject target;
    private float damage;
    public float speed = 5.0f;

    public void Initialize(UnitBase attacker, GameObject targetEnemy, float attackDamage)
    {
        owner = attacker;
        target = targetEnemy;
        damage = attackDamage;
    }

    private void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
            {
                UnitBase targetUnit = target.GetComponent<UnitBase>();
                if (targetUnit != null)
                {
                    targetUnit.TakeDamage(damage);
                    owner.AddMana(owner.ManaRecoveryOnBasicAttack);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
