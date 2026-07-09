using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : NetworkBehaviour
{
    public static LevelProgressManager Instance;

    // plain list (not a NetworkList) to avoid deallocation crashes across scenes.
    // completion is server-tracked and doesn't strictly need per-frame sync.
    private HashSet<string> completedLevels = new HashSet<string>();

    private void Awake()
    {
        // persist across scenes so it isn't deallocated when levels load/unload
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsCompleted(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        return completedLevels.Contains(sceneName);
    }

    // called server-side when a level is beaten
    public void MarkCompleted(string sceneName)
    {
        if (!IsServer) return;
        if (string.IsNullOrEmpty(sceneName)) return;
        if (completedLevels.Contains(sceneName)) return;

        completedLevels.Add(sceneName);

        // sync to all clients so their briefings show it completed
        MarkCompletedClientRpc(sceneName);
    }

    [ClientRpc]
    private void MarkCompletedClientRpc(string sceneName)
    {
        completedLevels.Add(sceneName);
    }

    public bool IsUnlocked(LevelDefinition level)
    {
        if (level == null) return true;
        if (level.requiredLevel == null) return true;
        return IsCompleted(level.requiredLevel.sceneName);
    }
}