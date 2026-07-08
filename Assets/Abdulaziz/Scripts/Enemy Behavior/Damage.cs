using System;
using Unity.Netcode;
using UnityEngine;

public class Damage : NetworkBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;

    private NetworkVariable<float> networkHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public event Action<float> HealthChanged;
    public event Action Died;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => networkHealth.Value;
    public bool IsDead => networkHealth.Value <= 0f;
    public bool Invulnerable { get; set; }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            networkHealth.Value = maxHealth;

        networkHealth.OnValueChanged += OnHealthValueChanged;
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

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        // safety: if this object isn't network-spawned, we can't send RPCs.
        // this happens if the boss has no NetworkObject, or the scene was
        // opened directly without starting the network session.
        if (!IsSpawned)
        {
            Debug.LogWarning($"[Damage] '{gameObject.name}' took damage but is NOT network-spawned. " +
                             $"Check it has a NetworkObject and the scene was loaded via the network.", this);
            return;
        }

        TakeDamageServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float amount)
    {
        if (IsDead || Invulnerable) return;
        networkHealth.Value = Mathf.Max(0f, networkHealth.Value - amount);
    }
}