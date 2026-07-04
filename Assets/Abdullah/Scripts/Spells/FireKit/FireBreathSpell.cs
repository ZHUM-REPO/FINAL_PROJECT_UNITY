using UnityEngine;

public class FireBreathSpell : SpellBase
{
    [Header("Fire Breath Settings")]
    public float range = 8f;
    public float coneAngle = 30f;
    public float damagePerSecond = 20f;
    public float manaPerSecond = 10f;
    public Transform spawnPoint;

    // upgrade flags
    [HideInInspector] public bool upgradeLongerRange = false;       // level 1
    [HideInInspector] public bool upgradeWiderCone = false;         // level 2
    [HideInInspector] public bool upgradeIncreasedDamage = false;   // level 3

    private bool isBrething = false;

    // FireBreath is held — TryCast() starts it, TryStop() ends it
    // override TryCast so it doesn't consume all mana upfront
    public new bool TryCast()
    {
        if (isOnCooldown || isBrething) return false;
        if (playerStats.myMana <= 0f) return false;

        isBrething = true;
        playerStats.SetCasting(true);
        PlayCastParticles();
        if (castParticles != null) castParticles.Play();
        return true;
    }

    public void TryStop()
    {
        if (!isBrething) return;

        isBrething = false;
        playerStats.SetCasting(false);
        StartCooldown();
        GrantXP();

        if (castParticles != null) castParticles.Stop();
    }

    protected override void Cast() { } // not used — fire breath uses TryCast/TryStop

    protected override void Update()
    {
        base.Update();

        if (!isBrething) return;

        // drain mana per second
        float drain = manaPerSecond * Time.deltaTime;
        if (!playerStats.ConsumeMana(drain))
        {
            // out of mana — force stop
            TryStop();
            return;
        }

        ApplyBreathDamage();
    }

    private void ApplyBreathDamage()
    {
        float actualRange = upgradeLongerRange ? range * 1.5f : range;
        float actualAngle = upgradeWiderCone ? coneAngle * 1.4f : coneAngle;
        float actualDPS = upgradeIncreasedDamage ? damagePerSecond * 1.5f : damagePerSecond;

        // find all colliders in range
        Collider[] hits = Physics.OverlapSphere(spawnPoint.position, actualRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            // check if within cone angle
            Vector3 dirToTarget = (hit.transform.position - spawnPoint.position).normalized;
            float angle = Vector3.Angle(spawnPoint.forward, dirToTarget);
            if (angle > actualAngle) continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(actualDPS * Time.deltaTime);
        }
    }

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeLongerRange     = upgradesBought >= 1;
        upgradeWiderCone       = upgradesBought >= 2;
        upgradeIncreasedDamage = upgradesBought >= 3;
    }
}