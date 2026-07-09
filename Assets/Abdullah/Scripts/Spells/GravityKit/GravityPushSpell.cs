using UnityEngine;

public class GravityPushSpell : SpellBase
{
    [Header("Gravity Push Settings")]
    public float pushRadius = 5f;
    public float pushForce = 15f;
    public float damage = 10f;
    public ParticleSystem pushParticles;

    [HideInInspector] public bool upgradeLargerRadius = false;
    [HideInInspector] public bool upgradeIncreasedForce = false;
    [HideInInspector] public bool upgradeDamageOnLanding = false;

    protected override void Cast()
    {
        PlayCastParticles();

        float actualRadius = upgradeLargerRadius ? pushRadius * 1.75f : pushRadius;
        float actualForce = upgradeIncreasedForce ? pushForce * 1.5f : pushForce;

        if (pushParticles != null)
            Instantiate(pushParticles, transform.position, Quaternion.identity);

        SpellSyncer syncer = GetComponentInParent<SpellSyncer>();
        if (syncer != null)
            syncer.BroadcastSpellVisual((int)SpellVisualType.GravityPush,
                                        transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, actualRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            Vector3 direction = (hit.transform.position - transform.position).normalized;
            direction.y = 0.3f;   // add a slight upward arc so they get knocked up a bit
            Vector3 force = direction.normalized * actualForce;

            // MINION: route the push through the server (handles NavMeshAgent + networking)
            BossMinionAI minion = hit.GetComponentInParent<BossMinionAI>();
            if (minion != null)
            {
                minion.ApplyGravityPush(force);

                // minions take damage too
                minion.TakeDamage(damage);
                continue;
            }

            // REGULAR physics object (crates, etc.) — push directly
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(force, ForceMode.Impulse);

            // boss or other enemy health
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            if (upgradeDamageOnLanding)
            {
                GravityLandingDamage landing = hit.gameObject
                    .GetComponent<GravityLandingDamage>()
                    ?? hit.gameObject.AddComponent<GravityLandingDamage>();
                landing.Activate();
            }
        }
    }

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeLargerRadius    = upgradesBought >= 1;
        upgradeIncreasedForce  = upgradesBought >= 2;
        upgradeDamageOnLanding = upgradesBought >= 3;
    }
}