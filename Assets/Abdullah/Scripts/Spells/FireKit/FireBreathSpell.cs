using UnityEngine;

public class FireBreathSpell : SpellBase
{
    [Header("Fire Breath Settings")]
    public float range = 8f;
    public float coneAngle = 30f;
    public float damagePerSecond = 20f;
    public float manaPerSecond = 10f;
    public Transform spawnPoint;

    [Header("Flame Prefab")]
    public GameObject flamePrefab;   // drag the vfx_Flamethrower_01 PREFAB here

    [HideInInspector] public bool upgradeLongerRange = false;
    [HideInInspector] public bool upgradeWiderCone = false;
    [HideInInspector] public bool upgradeIncreasedDamage = false;

    private bool isBrething = false;
    private GameObject flameInstance;   // spawned copy that lives during the breath

    public new bool TryCast()
    {
        if (isOnCooldown || isBrething) return false;
        if (playerStats.myMana <= 0f) return false;

        isBrething = true;
        playerStats.SetCasting(true);

        SpawnFlame();
        return true;
    }

    public void TryStop()
    {
        if (!isBrething) return;

        isBrething = false;
        playerStats.SetCasting(false);
        StartCooldown();
        GrantXP();

        DestroyFlame();
    }

    protected override void Cast() { }

    protected override void Update()
    {
        base.Update();

        if (!isBrething) return;

        AimFlame();

        float drain = manaPerSecond * Time.deltaTime;
        if (!playerStats.ConsumeMana(drain))
        {
            TryStop();
            return;
        }

        ApplyBreathDamage();
    }

    private void SpawnFlame()
    {
        if (flamePrefab == null)
        {
            Debug.LogWarning("FireBreath: flamePrefab not assigned!");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 forward = Camera.main != null ? Camera.main.transform.forward : transform.forward;

        flameInstance = Instantiate(flamePrefab, pos, Quaternion.LookRotation(forward));
    }

    private void AimFlame()
    {
        if (flameInstance == null) return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 forward = Camera.main != null ? Camera.main.transform.forward : transform.forward;

        flameInstance.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(forward));
    }

    private void DestroyFlame()
    {
        if (flameInstance != null)
            Destroy(flameInstance);
        flameInstance = null;
    }

    private void ApplyBreathDamage()
    {
        float actualRange = upgradeLongerRange ? range * 1.5f : range;
        float actualAngle = upgradeWiderCone ? coneAngle * 1.4f : coneAngle;
        float actualDPS = upgradeIncreasedDamage ? damagePerSecond * 1.5f : damagePerSecond;

        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 forward = Camera.main != null ? Camera.main.transform.forward : transform.forward;

        Collider[] hits = Physics.OverlapSphere(origin, actualRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            Vector3 dirToTarget = (hit.transform.position - origin).normalized;
            float angle = Vector3.Angle(forward, dirToTarget);
            if (angle > actualAngle) continue;

            ApplyDamage(hit, actualDPS * Time.deltaTime);
        }
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

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeLongerRange = upgradesBought >= 1;
        upgradeWiderCone = upgradesBought >= 2;
        upgradeIncreasedDamage = upgradesBought >= 3;
    }
}