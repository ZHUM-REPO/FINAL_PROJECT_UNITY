using Unity.Netcode;
using UnityEngine;

public class NetworkKitSelection : NetworkBehaviour
{
    public NetworkVariable<Unity.Collections.FixedString64Bytes> lockedKitOne =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);   // was Owner

    public NetworkVariable<Unity.Collections.FixedString64Bytes> lockedKitTwo =
        new NetworkVariable<Unity.Collections.FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);   // was Owner

    private KitManager kitManager;

    private void Awake()
    {
        kitManager = GetComponent<KitManager>();
    }

    public void LockKit(string kitName, int slot)
    {
        if (!IsOwner) return;
        // route the write through the server since it's now server-write
        SetLockServerRpc(kitName, slot, (int)OwnerClientId);
    }

    public void UnlockKit(string kitName)
    {
        if (!IsOwner) return;
        ClearLockServerRpc(kitName, (int)OwnerClientId);
    }

    // ─── Server RPCs ──────────────────────────────────────

    [ServerRpc]
    private void SetLockServerRpc(string kitName, int slot, int playerID)
    {
        // server writes the variables (now allowed)
        if (slot == 0) lockedKitOne.Value = kitName;
        if (slot == 1) lockedKitTwo.Value = kitName;

        LockKitClientRpc(kitName, playerID);
    }

    [ServerRpc]
    private void ClearLockServerRpc(string kitName, int playerID)
    {
        if (lockedKitOne.Value == kitName) lockedKitOne.Value = "";
        if (lockedKitTwo.Value == kitName) lockedKitTwo.Value = "";

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