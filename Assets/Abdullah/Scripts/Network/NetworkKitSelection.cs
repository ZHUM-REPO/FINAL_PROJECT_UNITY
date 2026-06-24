using Unity.Netcode;
using UnityEngine;

public class NetworkKitSelection : NetworkBehaviour
{
    // synced kit lock state across all clients
    // this is on the player that owns the lock
    public NetworkVariable<Unity.Collections.FixedString64Bytes> lockedKitOne =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<Unity.Collections.FixedString64Bytes> lockedKitTwo =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private KitManager kitManager;

    private void Awake()
    {
        kitManager = GetComponent<KitManager>();
    }

    // called when player locks a kit in lobby
    public void LockKit(string kitName, int slot)
    {
        if (!IsOwner) return;

        if (slot == 0) lockedKitOne.Value = kitName;
        if (slot == 1) lockedKitTwo.Value = kitName;

        // tell server to update global kit lock
        LockKitServerRpc(kitName, (int)OwnerClientId);
    }

    public void UnlockKit(string kitName)
    {
        if (!IsOwner) return;

        if (lockedKitOne.Value == kitName) lockedKitOne.Value = "";
        if (lockedKitTwo.Value == kitName) lockedKitTwo.Value = "";

        UnlockKitServerRpc(kitName, (int)OwnerClientId);
    }

    // ─── Server RPCs ──────────────────────────────────────

    [ServerRpc]
    private void LockKitServerRpc(string kitName, int playerID)
    {
        // tell all clients this kit is now locked
        LockKitClientRpc(kitName, playerID);
    }

    [ServerRpc]
    private void UnlockKitServerRpc(string kitName, int playerID)
    {
        UnlockKitClientRpc(kitName, playerID);
    }

    // ─── Client RPCs ──────────────────────────────────────

    [ClientRpc]
    private void LockKitClientRpc(string kitName, int playerID)
    {
        KitAssignmentManager.Instance?.TryLockKit(kitName, playerID);
    }

    [ClientRpc]
    private void UnlockKitClientRpc(string kitName, int playerID)
    {
        KitAssignmentManager.Instance?.UnlockKit(kitName, playerID);
    }
}