using Unity.Netcode;
using UnityEngine;

// IDs must match the order of the Spell Visual Prefabs array below
public enum SpellVisualType
{
    Fireball = 0,
    Freeze = 1,
    IceWall = 2,
    GravityPush = 3
}

public class SpellSyncer : NetworkBehaviour
{
    [Header("Visual prefabs — fill in the SAME order as SpellVisualType")]
    public GameObject[] spellVisualPrefabs;

    // Called by the local owner right after they spawn a spell locally.
    public void BroadcastSpellVisual(int spellId, Vector3 position, Quaternion rotation)
    {
        if (!IsOwner) return;
        SpawnVisualServerRpc(spellId, position, rotation);
    }

    [ServerRpc]
    private void SpawnVisualServerRpc(int spellId, Vector3 position, Quaternion rotation)
    {
        SpawnVisualClientRpc(spellId, position, rotation, OwnerClientId);
    }

    [ClientRpc]
    private void SpawnVisualClientRpc(int spellId, Vector3 position,
                                      Quaternion rotation, ulong casterId)
    {
        // the caster already has their own local copy — don't duplicate it
        if (NetworkManager.Singleton.LocalClientId == casterId) return;

        if (spellVisualPrefabs == null) return;
        if (spellId < 0 || spellId >= spellVisualPrefabs.Length) return;

        GameObject prefab = spellVisualPrefabs[spellId];
        if (prefab == null) return;

        GameObject visual = Instantiate(prefab, position, rotation);

        // mark as visual-only so remote copies don't also apply damage
        ProjectileBase proj = visual.GetComponent<ProjectileBase>();
        if (proj != null) proj.isVisualOnly = true;
    }
}