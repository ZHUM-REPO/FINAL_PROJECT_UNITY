using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerData : NetworkBehaviour
{
    public NetworkVariable<float> networkHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);   // was Owner

    public NetworkVariable<float> networkMana = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<float> networkEndurance = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> networkIsDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> networkScore = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<Unity.Collections.FixedString64Bytes> networkKitName =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<Unity.Collections.FixedString64Bytes> networkSpellName =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

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
        // the SERVER now drives the synced values for every player,
        // and the owner sends its local values up via an RPC
        if (IsServer)
            InvokeRepeating(nameof(ServerSync), 0f, 0.1f);

        if (IsOwner)
            InvokeRepeating(nameof(SendLocalValues), 0f, 0.1f);
    }

    // runs on the SERVER — writes the NetworkVariables (allowed, server-write)
    private void ServerSync()
    {
        if (playerStats == null) return;

        networkHealth.Value = playerStats.myHealth;
        networkMana.Value = playerStats.myMana;
        networkEndurance.Value = playerStats.myEndurance;
        networkIsDead.Value = playerStats.IsDead();

        if (playerProgression != null)
            networkScore.Value = playerProgression.score;

        if (kitManager != null && kitManager.equippedKit != null)
            networkKitName.Value = kitManager.equippedKit.kitName;

        if (kitManager != null)
            networkSpellName.Value = kitManager.GetActiveSpellName();
    }

    // runs on the OWNER — sends its local stat values to the server
    private void SendLocalValues()
    {
        if (playerStats == null) return;

        string kitName = (kitManager != null && kitManager.equippedKit != null)
            ? kitManager.equippedKit.kitName : "";
        string spellName = kitManager != null ? kitManager.GetActiveSpellName() : "";

        SubmitValuesServerRpc(
            playerStats.myHealth,
            playerStats.myMana,
            playerStats.myEndurance,
            playerStats.IsDead(),
            playerProgression != null ? playerProgression.score : 0,
            kitName,
            spellName);
    }

    [ServerRpc]
    private void SubmitValuesServerRpc(float health, float mana, float endurance,
        bool isDead, int score,
        Unity.Collections.FixedString64Bytes kitName,
        Unity.Collections.FixedString64Bytes spellName)
    {
        // apply the owner's reported values to the local PlayerStats on the server,
        // so ServerSync writes accurate numbers into the NetworkVariables
        if (playerStats != null)
        {
            playerStats.myHealth = health;
            playerStats.myMana = mana;
            playerStats.myEndurance = endurance;
        }

        networkScore.Value = score;
        networkKitName.Value = kitName;
        networkSpellName.Value = spellName;
    }
}