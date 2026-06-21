using UnityEngine;

// attached temporarily to pushed enemies to deal damage when they hit something
public class GravityLandingDamage : MonoBehaviour
{
    public float landingDamage = 20f;
    private bool isActive = false;

    public void Activate()
    {
        isActive = true;
        // deactivate after 3 seconds regardless
        Invoke(nameof(Deactivate), 3f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        EnemyHealth enemy = GetComponent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(landingDamage);

        Deactivate();
    }

    private void Deactivate()
    {
        isActive = false;
        Destroy(this);
    }
}