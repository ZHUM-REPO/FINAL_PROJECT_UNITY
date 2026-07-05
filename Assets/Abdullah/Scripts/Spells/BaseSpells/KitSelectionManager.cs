using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class KitSelectionManager : NetworkBehaviour
{
    public static KitSelectionManager Instance;

    [Header("All available kits (order matters — must match UI)")]
    public KitDefinition[] allKits;   // size 4: Fire, Ice, Heal, Gravity

    // networked ownership: index = kit slot, value = owning clientId (ulong.MaxValue = free)
    private NetworkList<ulong> kitOwners;

    // fired on clients whenever ownership changes, so the UI can refresh
    public event System.Action OnKitOwnershipChanged;

    private const ulong FREE = ulong.MaxValue;

    private void Awake()
    {
        Instance = this;
        kitOwners = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        // initialise the list on the server (one slot per kit, all free)
        if (IsServer)
        {
            kitOwners.Clear();
            for (int i = 0; i < allKits.Length; i++)
                kitOwners.Add(FREE);
        }

        kitOwners.OnListChanged += HandleListChanged;
    }

    public override void OnNetworkDespawn()
    {
        kitOwners.OnListChanged -= HandleListChanged;
    }

    private void HandleListChanged(NetworkListEvent<ulong> change)
    {
        OnKitOwnershipChanged?.Invoke();
    }

    // ─── Queries (used by the UI) ───────────────────────────

    public ulong GetOwner(int kitIndex)
    {
        if (kitIndex < 0 || kitIndex >= kitOwners.Count) return FREE;
        return kitOwners[kitIndex];
    }

    public bool IsFree(int kitIndex) => GetOwner(kitIndex) == FREE;

    public bool IsOwnedByLocalPlayer(int kitIndex)
    {
        return GetOwner(kitIndex) == NetworkManager.Singleton.LocalClientId;
    }

    // how many kits a single player is allowed, based on player count
    public int GetPerPlayerCap()
    {
        int players = Mathf.Max(1, NetworkManager.Singleton.ConnectedClientsList.Count);
        return Mathf.CeilToInt((float)allKits.Length / players);
    }

    public int CountKitsOwnedBy(ulong clientId)
    {
        int count = 0;
        for (int i = 0; i < kitOwners.Count; i++)
            if (kitOwners[i] == clientId) count++;
        return count;
    }

    // ─── Actions (called from the UI, run on server) ────────

    // player clicked a kit — decide claim vs release
    public void RequestToggleKit(int kitIndex)
    {
        ToggleKitServerRpc(kitIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleKitServerRpc(int kitIndex, ServerRpcParams rpcParams = default)
    {
        ulong requester = rpcParams.Receive.SenderClientId;

        if (kitIndex < 0 || kitIndex >= kitOwners.Count) return;

        ulong currentOwner = kitOwners[kitIndex];

        // release: the requester already owns this kit
        if (currentOwner == requester)
        {
            kitOwners[kitIndex] = FREE;
            EquipKitClientRpc(kitIndex, requester, false);
            return;
        }

        // claim: only if free AND requester is under their cap
        if (currentOwner == FREE)
        {
            if (CountKitsOwnedBy(requester) >= GetPerPlayerCap())
            {
                Debug.Log($"Client {requester} is at their kit cap.");
                return;
            }

            kitOwners[kitIndex] = requester;
            EquipKitClientRpc(kitIndex, requester, true);
        }

        // if owned by someone else → do nothing (locked)
    }

    // tell the owning client to equip/unequip the kit on their KitManager
    [ClientRpc]
    private void EquipKitClientRpc(int kitIndex, ulong targetClient, bool equip)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClient) return;

        KitManager km = FindLocalKitManager();
        if (km == null) return;

        if (equip)
            km.AddKit(allKits[kitIndex]);
        else
            km.RemoveKit(allKits[kitIndex]);
    }

    private KitManager FindLocalKitManager()
    {
        foreach (var km in FindObjectsByType<KitManager>(FindObjectsSortMode.None))
        {
            NetworkObject netObj = km.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
                return km;
        }
        return null;
    }
}