using UnityEngine;

public class FreezeSpell : SpellBase
{
    [Header("Freeze Settings")]
    public GameObject freezeProjectilePrefab;
    public Transform spawnPoint;
    public float freezeDuration = 3f;
    public float damage = 20f; // HP damage dealt on hit (passed to the projectile)

    [HideInInspector] public bool upgradeLongerFreeze = false;
    [HideInInspector] public bool upgradeAreaFreeze = false;
    [HideInInspector] public bool upgradeInstantKillFrozen = false;

    protected override void Cast()
    {
        PlayCastParticles();
        if (freezeProjectilePrefab == null || spawnPoint == null) return;

        Vector3 cameraForward = Camera.main.transform.forward;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);

        GameObject proj = Instantiate(freezeProjectilePrefab,
                                      spawnPoint.position, cameraRotation);

        FreezeProjectile fp = proj.GetComponent<FreezeProjectile>();
        if (fp != null)
        {
            fp.freezeDuration = upgradeLongerFreeze ? freezeDuration * 2f : freezeDuration;
            fp.hasAreaFreeze = upgradeAreaFreeze;
            fp.instantKillFrozen = upgradeInstantKillFrozen;
            fp.damage = damage; // deal HP damage in addition to freezing
            fp.impactParticles = impactParticles;
        }

        SpellSyncer syncer = GetComponentInParent<SpellSyncer>();
        if (syncer != null)
            syncer.BroadcastSpellVisual((int)SpellVisualType.Freeze,
                                        spawnPoint.position, cameraRotation);
    }

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeLongerFreeze = upgradesBought >= 1;
        upgradeAreaFreeze = upgradesBought >= 2;
        upgradeInstantKillFrozen = upgradesBought >= 3;
    }
}