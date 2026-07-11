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
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(damage);

        // explosion upgrade — damages everything in radius
        if (hasExplosion)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player")) continue;
                EnemyHealth e = hit.GetComponent<EnemyHealth>();
                if (e != null)
                    e.TakeDamage(explosionDamage);
            }

            if (explosionParticles != null)
                Instantiate(explosionParticles, transform.position, Quaternion.identity);
        }
    }
}