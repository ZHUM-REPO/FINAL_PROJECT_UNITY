using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerData : NetworkBehaviour
{
    // synced values — all clients can read, only owner can write
    public NetworkVariable<float> networkHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> networkMana = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> networkEndurance = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> networkIsDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> networkScore = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // kit name synced so other players can see your kit
    public NetworkVariable<Unity.Collections.FixedString64Bytes> networkKitName =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private PlayerStats playerStats;
    private PlayerProgression playerProgression;
    private KitManager kitManager;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        playerProgression = GetComponent<PlayerProgression>();
        kitManager = GetComponent<KitManager>();
    }

    public override void OnNetworkSpawn()
    {
        // only the owner updates their own values
        if (!IsOwner) return;

        // listen for kit changes to sync kit name
        InvokeRepeating(nameof(SyncValues), 0f, 0.1f);
    }

    private void SyncValues()
    {
        if (!IsOwner) return;

        networkHealth.Value = playerStats.myHealth;
        networkMana.Value = playerStats.myMana;
        networkEndurance.Value = playerStats.myEndurance;
        networkIsDead.Value = playerStats.IsDead();
        networkScore.Value = playerProgression.score;

        if (kitManager.equippedKit != null)
            networkKitName.Value = kitManager.equippedKit.kitName;
    }
}