using UnityEngine;

public class GravityPushSpell : SpellBase
{
    [Header("Gravity Push Settings")]
    public float pushRadius = 5f;
    public float pushForce = 15f;
    public float damage = 10f;
    public ParticleSystem pushParticles;

    [HideInInspector] public bool upgradeLargerRadius = false;      // level 1
    [HideInInspector] public bool upgradeIncreasedForce = false;    // level 2
    [HideInInspector] public bool upgradeDamageOnLanding = false;   // level 3

    protected override void Cast()
    {
        PlayCastParticles();

        float actualRadius = upgradeLargerRadius ? pushRadius * 1.75f : pushRadius;
        float actualForce = upgradeIncreasedForce ? pushForce * 1.5f : pushForce;

        if (pushParticles != null)
            Instantiate(pushParticles, transform.position, Quaternion.identity);

        // tell the other players to spawn a visual copy of the push effect
        SpellSyncer syncer = GetComponentInParent<SpellSyncer>();
        if (syncer != null)
            syncer.BroadcastSpellVisual((int)SpellVisualType.GravityPush,
                                        transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, actualRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            // push rigidbody if it has one (objects & ragdolls)
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                rb.AddForce(direction * actualForce, ForceMode.Impulse);
            }

            // damage enemy
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            // level 3 upgrade — add landing damage component to track collision
            if (upgradeDamageOnLanding)
            {
                GravityLandingDamage landing = hit.gameObject
                    .GetComponent<GravityLandingDamage>()
                    ?? hit.gameObject.AddComponent<GravityLandingDamage>();
                landing.Activate();
            }
        }
    }
}