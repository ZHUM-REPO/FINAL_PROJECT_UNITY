using UnityEngine;

public abstract class SpellBase : MonoBehaviour
{
    [Header("Spell Info")]
    public string spellName;
    public float manaCost;
    public float cooldown;
    public float xpReward = 10f;

    [Header("Visuals")]
    public ParticleSystem castParticles;
    public ParticleSystem impactParticles;

    protected float cooldownTimer = 0f;
    protected bool isOnCooldown = false;
    protected PlayerStats playerStats;
    protected PlayerProgression playerProgression;
    protected string kitName;

    protected virtual void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        playerProgression = GetComponentInParent<PlayerProgression>();
    }

    protected virtual void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                isOnCooldown = false;
            }
        }
    }

    // called by KitManager when player presses cast button
    public bool TryCast()
    {
        if (isOnCooldown)
        {
            Debug.Log($"{spellName} is on cooldown.");
            return false;
        }

        if (!playerStats.ConsumeMana(manaCost))
        {
            Debug.Log($"Not enough mana for {spellName}.");
            return false;
        }

        StartCooldown();
        Cast();
        GrantXP();
        return true;
    }

    protected void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldown;
    }

    protected void GrantXP()
    {
        if (playerProgression != null && !string.IsNullOrEmpty(kitName))
            playerProgression.AddKitXP(kitName, xpReward);
    }

    protected void PlayCastParticles()
    {
        if (castParticles != null)
            castParticles.Play();
    }

    protected void SpawnImpactParticles(Vector3 position, Quaternion rotation)
    {
        if (impactParticles != null)
            Instantiate(impactParticles, position, rotation);
    }

    public void SetKitName(string name) => kitName = name;

    public float GetCooldownProgress() => cooldown > 0 ? cooldownTimer / cooldown : 0f;

    public bool IsOnCooldown() => isOnCooldown;

    // each spell implements its own cast logic
    protected abstract void Cast();
}