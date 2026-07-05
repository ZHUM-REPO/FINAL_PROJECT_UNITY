using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private bool isDead = false;
    private PlayerStats playerStats;

    private void Awake() => playerStats = GetComponent<PlayerStats>();

    private void Update()
    {
        if (!isDead && playerStats.IsDead())
            Die();
    }

    public bool IsDead() => isDead;

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died. Waiting for resurrection.");
        // we will add spectator camera switch here in a later phase
    }

    public void Resurrect(float healthPercent)
    {
        isDead = false;
        playerStats.myHealth = playerStats.maxHealth * healthPercent;
        Debug.Log($"{gameObject.name} resurrected with {playerStats.myHealth} HP");
        // we will re-enable player controller here in a later phase
    }
}