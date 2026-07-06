using UnityEngine;

public class FreezeProjectile : ProjectileBase
{
    [Header("Freeze")]
    public float freezeDuration = 3f;
    public bool hasAreaFreeze = false;
    public float areaRadius = 4f;
    public bool instantKillFrozen = false;

    protected override void OnHit(Collider other)
    {
        if (other.CompareTag("Player")) return;

        if (hasAreaFreeze)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, areaRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player")) continue;
                ApplyDamage(hit, damage); // 'damage' comes from ProjectileBase
                ApplyFreeze(hit);
            }
        }
        else
        {
            ApplyDamage(other, damage);
            ApplyFreeze(other);
        }
    }

    private void ApplyFreeze(Collider target)
    {
        IFreezable freezable = target.GetComponent<IFreezable>();
        if (freezable != null)
            freezable.Freeze(freezeDuration, instantKillFrozen);
    }

    // Boss & minions implement IDamageable (via the Damage component);
    // legacy enemies use EnemyHealth. Check the collider and its parents.
    private void ApplyDamage(Collider target, float amount)
    {
        if (amount <= 0f) return;

        IDamageable damageable = target.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(amount);
            return;
        }

        EnemyHealth enemy = target.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(amount);
    }
}