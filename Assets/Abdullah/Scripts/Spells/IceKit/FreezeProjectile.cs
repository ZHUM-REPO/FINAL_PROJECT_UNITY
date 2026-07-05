using UnityEngine;

public class FreezeProjectile : ProjectileBase
{
    [Header("Freeze")]
    public float freezeDuration = 3f;
    public bool hasAreaFreeze = false;
    public float areaRadius = 4f;
    public bool instantKillFrozen = false;

    protected override void OnHit(Collider other)
    {
        if (other.CompareTag("Player")) return;

        if (hasAreaFreeze)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, areaRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player")) continue;
                ApplyFreeze(hit);
            }
        }
        else
        {
            ApplyFreeze(other);
        }
    }

    private void ApplyFreeze(Collider target)
    {
        IFreezable freezable = target.GetComponent<IFreezable>();
        if (freezable != null)
            freezable.Freeze(freezeDuration, instantKillFrozen);
    }
}