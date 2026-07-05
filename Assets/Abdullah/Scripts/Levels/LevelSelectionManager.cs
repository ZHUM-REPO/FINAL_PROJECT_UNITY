using Unity.Netcode;
using UnityEngine;

public class LevelSelectionManager : NetworkBehaviour
{
    public static LevelSelectionManager Instance;

    [Header("Levels (index order must match holders)")]
    public LevelDefinition[] levels;   // size 3

    public NetworkVariable<int> selectedLevel = new NetworkVariable<int>(
        -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public event System.Action OnSelectionChanged;

    private void Awake() { Instance = this; }

    public override void OnNetworkSpawn()
    {
        selectedLevel.OnValueChanged += (_, __) => OnSelectionChanged?.Invoke();
    }

    public void SelectLevel(int levelIndex)
    {
        SelectLevelServerRpc(levelIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectLevelServerRpc(int levelIndex, ServerRpcParams p = default)
    {
        // host-only
        if (p.Receive.SenderClientId != NetworkManager.ServerClientId) return;
        if (levelIndex < 0 || levelIndex >= levels.Length) return;

        selectedLevel.Value = levelIndex;
        Debug.Log($"Level selected: {levels[levelIndex].sceneName}");
    }

    public string GetSelectedSceneName()
    {
        int i = selectedLevel.Value;
        if (i < 0 || i >= levels.Length) return null;
        return levels[i].sceneName;
    }
}