using System;
using Unity.Netcode;
using UnityEngine;

public class Damage : NetworkBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;

    // shared health — server writes, everyone reads
    private NetworkVariable<float> networkHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public event Action<float> HealthChanged; // passes current health
    public event Action Died;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => networkHealth.Value;
    public bool IsDead => networkHealth.Value <= 0f;
    public bool Invulnerable { get; set; }

    public override void OnNetworkSpawn()
    {
        // set starting health on the server
        if (IsServer)
            networkHealth.Value = maxHealth;

        // everyone reacts when health changes (drives UI + boss phase logic)
        networkHealth.OnValueChanged += OnHealthValueChanged;

        // fire once so UI initializes to the current value
        HealthChanged?.Invoke(networkHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        networkHealth.OnValueChanged -= OnHealthValueChanged;
    }

    private void OnHealthValueChanged(float oldValue, float newValue)
    {
        HealthChanged?.Invoke(newValue);

        if (newValue <= 0f && oldValue > 0f)
            Died?.Invoke();
    }

    // players call this when their spell hits the boss.
    // it routes to the server, which is the only one allowed to change health.
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        TakeDamageServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float amount)
    {
        if (IsDead || Invulnerable) return;

        networkHealth.Value = Mathf.Max(0f, networkHealth.Value - amount);
        // OnValueChanged fires on all clients automatically, driving HealthChanged/Died
    }
}

public interface IDamageable
{
    void TakeDamage(float amount);
}