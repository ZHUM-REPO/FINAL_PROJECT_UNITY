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

    // set true on remote copies so they show but don't deal damage
    [HideInInspector] public bool isVisualOnly = false;

    private float lifetimeTimer;

    protected virtual void Start()
    {
        lifetimeTimer = lifetime;
        if (trailParticles != null)
            trailParticles.Play();
    }

    protected virtual void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
            DestroyProjectile();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;

        // remote visual copies skip damage but still show impact + despawn
        if (!isVisualOnly)
            OnHit(other);

        SpawnImpact();
        DestroyProjectile();
    }

    protected virtual void OnHit(Collider other)
    {
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
            trailParticles.transform.SetParent(null);
            trailParticles.Stop();
            Destroy(trailParticles.gameObject, 2f);
        }
        Destroy(gameObject);
    }
}