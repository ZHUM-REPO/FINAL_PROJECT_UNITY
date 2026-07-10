using Unity.Netcode;
using UnityEngine;

public class NetworkWand : NetworkBehaviour
{
    [Header("All kits in fixed order (must match KitManager.allKits)")]
    public KitDefinition[] allKits;   // 0=Fire, 1=Ice, 2=Gravity, 3=Heal

    [Header("Where the wand attaches (the old MagicStick spot)")]
    public Transform wandHolder;

    // which wand each player holds — synced to everyone. -1 = none.
    private NetworkVariable<int> equippedWandIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private GameObject currentWand;

    public override void OnNetworkSpawn()
    {
        equippedWandIndex.OnValueChanged += OnWandChanged;

        // build the initial wand if one is already set (late joiners)
        if (equippedWandIndex.Value >= 0)
            SwapWandModel(equippedWandIndex.Value);
    }

    public override void OnNetworkDespawn()
    {
        equippedWandIndex.OnValueChanged -= OnWandChanged;
    }

    // called by KitManager (on the owner) when a kit is equipped
    public void SetWandForKit(KitDefinition kit)
    {
        if (!IsOwner) return;

        int index = GetKitIndex(kit);
        equippedWandIndex.Value = index;   // owner-write; syncs to all clients
    }

    private int GetKitIndex(KitDefinition kit)
    {
        if (allKits == null || kit == null) return -1;
        for (int i = 0; i < allKits.Length; i++)
            if (allKits[i] == kit) return i;
        return -1;
    }

    private void OnWandChanged(int oldIndex, int newIndex)
    {
        SwapWandModel(newIndex);
    }

    private void SwapWandModel(int index)
    {
        if (currentWand != null)
        {
            Destroy(currentWand);
            currentWand = null;
        }

        if (index < 0 || allKits == null || index >= allKits.Length) return;

        KitDefinition kit = allKits[index];
        if (kit == null || kit.wandPrefab == null || wandHolder == null) return;

        // parent it but KEEP the prefab's own local transform (false = keep local values)
        currentWand = Instantiate(kit.wandPrefab);
        currentWand.transform.SetParent(wandHolder, false);
    }
}