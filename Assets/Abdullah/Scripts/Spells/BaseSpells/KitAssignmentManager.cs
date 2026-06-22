using System.Collections.Generic;
using UnityEngine;
using System;

public class KitAssignmentManager : MonoBehaviour
{
    public static KitAssignmentManager Instance;

    [Header("All Available Kits")]
    public KitDefinition fireKit;
    public KitDefinition iceKit;
    public KitDefinition healKit;
    public KitDefinition gravityKit;

    // tracks which player locked which kit
    // key: kit name, value: player ID who locked it
    private Dictionary<string, int> lockedKits = new Dictionary<string, int>();

    // tracks which kits each player has chosen
    // key: player ID, value: list of kit names
    private Dictionary<int, List<string>> playerKits = new Dictionary<int, List<string>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── How Many Kits Per Player ─────────────────────────

    public int GetKitsAllowedForPlayer(int totalPlayers)
    {
        // this handles the uneven 3-player case separately
        return totalPlayers switch
        {
            1 => 4,
            2 => 2,
            4 => 1,
            _ => 1 // default fallback
        };
    }

    // for 3 players — first player to pick gets 2 kits,
    // the other two get 1 kit each
    public int GetKitsAllowedForPlayer(int totalPlayers, int playerIndex)
    {
        if (totalPlayers == 3)
            return playerIndex == 0 ? 2 : 1;

        return GetKitsAllowedForPlayer(totalPlayers);
    }

    // ─── Kit Locking ──────────────────────────────────────

    public bool TryLockKit(string kitName, int playerID)
    {
        // already locked by someone else
        if (lockedKits.ContainsKey(kitName) && lockedKits[kitName] != playerID)
        {
            Debug.Log($"{kitName} is already taken.");
            return false;
        }

        lockedKits[kitName] = playerID;

        if (!playerKits.ContainsKey(playerID))
            playerKits[playerID] = new List<string>();

        if (!playerKits[playerID].Contains(kitName))
            playerKits[playerID].Add(kitName);

        Debug.Log($"Player {playerID} locked {kitName}");
        return true;
    }

    public void UnlockKit(string kitName, int playerID)
    {
        if (lockedKits.ContainsKey(kitName) && lockedKits[kitName] == playerID)
        {
            lockedKits.Remove(kitName);

            if (playerKits.ContainsKey(playerID))
                playerKits[playerID].Remove(kitName);

            Debug.Log($"Player {playerID} unlocked {kitName}");
        }
    }

    public bool IsKitLocked(string kitName) => lockedKits.ContainsKey(kitName);

    public int GetKitOwner(string kitName) =>
        lockedKits.ContainsKey(kitName) ? lockedKits[kitName] : -1;

    // ─── Kit Switching (for solo / multi-kit players) ─────

    public List<KitDefinition> GetPlayerKits(int playerID)
    {
        List<KitDefinition> kits = new List<KitDefinition>();
        if (!playerKits.ContainsKey(playerID)) return kits;

        foreach (string kitName in playerKits[playerID])
        {
            KitDefinition kit = GetKitByName(kitName);
            if (kit != null) kits.Add(kit);
        }
        return kits;
    }

    private KitDefinition GetKitByName(string name)
    {
        if (fireKit.kitName == name) return fireKit;
        if (iceKit.kitName == name) return iceKit;
        if (healKit.kitName == name) return healKit;
        if (gravityKit.kitName == name) return gravityKit;
        return null;
    }

    // ─── Validation ───────────────────────────────────────

    public bool PlayerHasMaxKits(int playerID, int totalPlayers, int playerIndex)
    {
        int allowed = GetKitsAllowedForPlayer(totalPlayers, playerIndex);
        int current = playerKits.ContainsKey(playerID) ? playerKits[playerID].Count : 0;
        return current >= allowed;
    }
}