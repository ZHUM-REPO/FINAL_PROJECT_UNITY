using UnityEngine;

public class FireballSpell : SpellBase
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public Transform spawnPoint; // tip of the magic stick
    public float damage = 30f;

    // upgrade flags — set by KitManager when kit levels up
    [HideInInspector] public bool upgradeIncreasedDamage = false;   // level 1
    [HideInInspector] public bool upgradeExplosionRadius = false;    // level 2
    [HideInInspector] public bool upgradeDoubleCast = false;         // level 3

    protected override void Cast()
    {
        PlayCastParticles();
        SpawnFireball();

        if (upgradeDoubleCast)
        {
            // slight delay for second fireball handled via coroutine
            StartCoroutine(DoubleCastDelay());
        }
    }

    private void SpawnFireball()
    {
        if (fireballPrefab == null || spawnPoint == null) return;

        // use camera direction instead of spawnPoint rotation
        Vector3 cameraForward = Camera.main.transform.forward;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);

        GameObject fb = Instantiate(fireballPrefab, spawnPoint.position, cameraRotation);
        FireballProjectile projectile = fb.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.damage = upgradeIncreasedDamage ? damage * 1.5f : damage;
            projectile.hasExplosion = upgradeExplosionRadius;
            projectile.impactParticles = impactParticles;
        }
    }

    private System.Collections.IEnumerator DoubleCastDelay()
    {
        yield return new WaitForSeconds(0.2f);
        SpawnFireball();
    }

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeIncreasedDamage = upgradesBought >= 1;
        upgradeExplosionRadius = upgradesBought >= 2;
        upgradeDoubleCast      = upgradesBought >= 3;
    }
}