using UnityEngine;
using Unity.Netcode;

public class PlayerStats : NetworkBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float myHealth;

    [Header("Mana")]
    public float maxMana = 100f;
    public float myMana;
    public float manaRegen = 5f;
    private bool isCasting = false;

    [Header("Endurance")]
    public float maxEndurance = 100f;
    public float myEndurance;
    public float enduranceDrain = 15f;
    public float enduranceRegain = 10f;
    [Range(0f, 1f)]
    public float sprintResumeThreshold = 0.25f;
    public bool canSprint = true;

    private void Start()
    {
        myHealth = maxHealth;
        myMana = maxMana;
        myEndurance = maxEndurance;
    }

    private void Update()
    {
        // only the owner updates their own stats
        if (!IsOwner) return;

        HandleManaRegen();
    }

    // ─── Mana ────────────────────────────────────────────

    private void HandleManaRegen()
    {
        if (isCasting) return;

        myMana += manaRegen * Time.deltaTime;
        myMana = Mathf.Clamp(myMana, 0f, maxMana);
    }

    public void SetCasting(bool casting)
    {
        isCasting = casting;
    }

    /// <summary>Returns true if mana was consumed, false if not enough.</summary>
    public bool ConsumeMana(float amount)
    {
        if (myMana < amount) return false;

        myMana -= amount;
        myMana = Mathf.Clamp(myMana, 0f, maxMana);
        return true;
    }

    // ─── Health ───────────────────────────────────────────

    // IDamageable: the boss and minions call this when they hit the player.
    // The boss runs on the server, so damage is applied server-side and
    // synced down to the owner so it can't be overwritten by client updates.
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        if (IsServer)
            ApplyDamage(amount);
        else
            TakeDamageServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float amount)
    {
        ApplyDamage(amount);
    }

    private void ApplyDamage(float amount)
    {
        myHealth -= amount;
        myHealth = Mathf.Clamp(myHealth, 0f, maxHealth);

        // tell the owner their new health so their local value matches
        SyncHealthClientRpc(myHealth);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        if (IsServer)
        {
            myHealth += amount;
            myHealth = Mathf.Clamp(myHealth, 0f, maxHealth);
            SyncHealthClientRpc(myHealth);
        }
        else
        {
            HealServerRpc(amount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealServerRpc(float amount)
    {
        myHealth += amount;
        myHealth = Mathf.Clamp(myHealth, 0f, maxHealth);
        SyncHealthClientRpc(myHealth);
    }

    [ClientRpc]
    private void SyncHealthClientRpc(float newHealth)
    {
        myHealth = newHealth;
    }

    public bool IsDead() => myHealth <= 0f;

    // ─── Endurance ────────────────────────────────────────

    public void EnduranceDrain()
    {
        myEndurance -= enduranceDrain * Time.deltaTime;
        myEndurance = Mathf.Clamp(myEndurance, 0f, maxEndurance);

        if (myEndurance <= 0f)
            canSprint = false;
    }

    public void EnduranceRegain()
    {
        myEndurance += enduranceRegain * Time.deltaTime;
        myEndurance = Mathf.Clamp(myEndurance, 0f, maxEndurance);

        if (!canSprint && myEndurance >= maxEndurance * sprintResumeThreshold)
            canSprint = true;
    }

    // ─── Stat Upgrades (called by PlayerProgression) ──────

    public void UpgradeMaxHealth(float amount)
    {
        maxHealth += amount;
        myHealth += amount; // current health scales up with the upgrade
    }

    public void UpgradeMaxMana(float amount)
    {
        maxMana += amount;
        myMana += amount;
    }

    public void UpgradeMaxEndurance(float amount)
    {
        maxEndurance += amount;
        myEndurance += amount;
    }
}