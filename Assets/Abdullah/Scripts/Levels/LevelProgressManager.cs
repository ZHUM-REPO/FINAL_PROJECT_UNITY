using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class LevelProgressManager : NetworkBehaviour
{
    public static LevelProgressManager Instance;

    private NetworkList<FixedString64Bytes> completedLevels;

    private void Awake()
    {
        Instance = this;
        completedLevels = new NetworkList<FixedString64Bytes>();
    }

    public bool IsCompleted(string sceneName)
    {
        foreach (var s in completedLevels)
            if (s.ToString() == sceneName) return true;
        return false;
    }

    // call this (server-side) from a level's win logic
    public void MarkCompleted(string sceneName)
    {
        if (!IsServer) return;
        if (IsCompleted(sceneName)) return;
        completedLevels.Add(new FixedString64Bytes(sceneName));
    }

    public bool IsUnlocked(LevelDefinition level)
    {
        if (level.requiredLevel == null) return true;
        return IsCompleted(level.requiredLevel.sceneName);
    }
}