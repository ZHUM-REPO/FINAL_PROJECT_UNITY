using UnityEngine;

public class FireballProjectile : ProjectileBase
{
    [Header("Explosion")]
    public bool hasExplosion = false;
    public float explosionRadius = 3f;
    public float explosionDamage = 15f;
    public ParticleSystem explosionParticles;

    protected override void OnHit(Collider other)
    {
        if (other.CompareTag("Player")) return;

        // direct hit damage
        ApplyDamage(other, damage);

        // explosion upgrade — damages everything in radius
        if (hasExplosion)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player")) continue;
                ApplyDamage(hit, explosionDamage);
            }

            if (explosionParticles != null)
                Instantiate(explosionParticles, transform.position, Quaternion.identity);
        }
    }

    // Boss & minions implement IDamageable (via the Damage component);
    // legacy enemies use EnemyHealth. Check the collider and its parents so it
    // works whether the health component sits on the collider or the root.
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