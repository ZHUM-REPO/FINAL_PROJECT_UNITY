using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f;
    public float damage = 25f;
    public float lifetime = 5f;

    [Header("Visuals")]
    public ParticleSystem impactParticles;
    public ParticleSystem trailParticles;

    private float lifetimeTimer;

    protected virtual void Start()
    {
        lifetimeTimer = lifetime;
        if (trailParticles != null)
            trailParticles.Play();
    }

    protected virtual void Update()
    {
        // move forward in straight line
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
            DestroyProjectile();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // ignore the caster
        if (other.CompareTag("Player")) return;

        OnHit(other);
        SpawnImpact();
        DestroyProjectile();
    }

    // override in each spell projectile to add unique hit effects
    protected virtual void OnHit(Collider other)
    {
        // apply damage if enemy
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }

    protected void SpawnImpact()
    {
        if (impactParticles != null)
            Instantiate(impactParticles, transform.position, Quaternion.identity);
    }

    protected void DestroyProjectile()
    {
        if (trailParticles != null)
        {
            // detach trail so it finishes playing before destroying
            trailParticles.transform.SetParent(null);
            trailParticles.Stop();
            Destroy(trailParticles.gameObject, 2f);
        }
        Destroy(gameObject);
    }
}