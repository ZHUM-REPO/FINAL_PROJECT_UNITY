using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Keeps exactly one active AudioListener in a networked game: enables it only on
/// the local player's copy and disables it on every remote copy. Put this on the
/// player prefab. Without this, each player prefab's listener is active at once,
/// which Unity rejects and audio breaks.
/// </summary>
public class LocalPlayerAudio : NetworkBehaviour
{
    [Tooltip("The player's AudioListener. Auto-found in children if left empty.")]
    [SerializeField] private AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        if (audioListener == null)
            audioListener = GetComponentInChildren<AudioListener>(true);

        if (audioListener != null)
            audioListener.enabled = IsOwner;   // only the local player hears
    }
}