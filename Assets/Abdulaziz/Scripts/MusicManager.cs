using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Owns the game's music. Every track plays continuously and loops — the manager
/// never stops any of them, it only changes volume: the active track at Active
/// Volume, the rest at 0. Each track is paired with the door that triggers it.
///
/// Server-driven, heard by everyone: the server decides in OnNetworkSpawn and
/// broadcasts a ClientRpc so each machine starts its own local music. Audio is
/// local to each machine, so this is how all players hear the soundtrack.
///
/// Requires a NetworkObject on this GameObject (in-scene placed object). Put this on
/// one always-active object and drag it into the doors that switch music.
/// </summary>
public class MusicManager : NetworkBehaviour
{
    /// <summary>Pairs a level door with the clip + AudioSource that should play
    /// when the player goes through it.</summary>
    [System.Serializable]
    public class DoorTrack
    {
        [Tooltip("The door that triggers this track (e.g. the Club door).")]
        public DoorRoomPedestal door;

        [Tooltip("The music clip that plays ONLY when the player passes through that door.")]
        public AudioClip clip;

        [Tooltip("The AudioSource the clip plays through. The manager loads the clip " +
                 "onto it, loops it, and mutes it until the door is used.")]
        public AudioSource track;
    }

    [Header("Door Tracks")]
    [Tooltip("Pair each level door with its clip and source. " +
             "e.g. Element 0 = Club door -> Clip 1 on Track 1.")]
    [SerializeField] private DoorTrack[] doorTracks;

    [Header("Final Door")]
    [Tooltip("The heart door. Its music plays when the player passes through to " +
             "the final area.")]
    [SerializeField] private FinalDoor finalDoor;

    [Tooltip("The clip that plays when the player reaches the final area.")]
    [SerializeField] private AudioClip finalClip;

    [Tooltip("The AudioSource the final clip plays through.")]
    [SerializeField] private AudioSource finalTrack;

    [Header("World Map")]
    [Tooltip("The default clip: audible when music starts and restored when the " +
             "player leaves a level.")]
    [SerializeField] private AudioClip worldMapClip;

    [Tooltip("The AudioSource the world-map clip plays through.")]
    [SerializeField] private AudioSource worldMapTrack;

    [Header("Volume")]
    [Tooltip("Volume of the audible track. Muted tracks are set to 0.")]
    [SerializeField] private float activeVolume = 1f;

    // What's audible now, and the trail to fall back through when leaving levels.
    private AudioSource _current;
    private readonly Stack<AudioSource> _history = new Stack<AudioSource>();
    private bool _started;

    public override void OnNetworkSpawn()
    {
        // Server decides, every client starts its own local music (audio is local
        // to each machine, so this is how all players hear the soundtrack).
        if (IsServer) BeginMusicClientRpc();
    }

    [ClientRpc]
    private void BeginMusicClientRpc() => BeginMusic();

    /// <summary>Start all tracks and play the world-map theme. Starts the tracks
    /// from the top, so it's best called once a listener exists.</summary>
    public void BeginMusic()
    {
        EnsureStarted();
        PlayWorldMapMusic();
    }

    /// <summary>Called by a level door when the player passes through it.</summary>
    public void PlayForDoor(DoorRoomPedestal door)
    {
        EnsureStarted();

        AudioSource track = FindTrack(door);
        if (track == null)
        {
            Debug.LogError($"{name}: no track mapped to door '{(door != null ? door.name : "null")}' in Door Tracks.", this);
            return;
        }

        MakeAudible(track);
    }

    /// <summary>Called by the FinalDoor when the player reaches the final area.</summary>
    public void PlayForFinalDoor(FinalDoor door)
    {
        EnsureStarted();

        if (finalDoor != null && door != finalDoor)
        {
            Debug.LogWarning($"{name}: PlayForFinalDoor called by an unexpected door '{(door != null ? door.name : "null")}'.", this);
            return;
        }
        if (finalTrack == null)
        {
            Debug.LogError($"{name}: no Final Track assigned.", this);
            return;
        }

        MakeAudible(finalTrack);
    }

    /// <summary>Leave a level: fall back to whatever was audible before (the world map).</summary>
    public void ReturnToPreviousMusic()
    {
        EnsureStarted();

        AudioSource previous = _history.Count > 0 ? _history.Pop() : worldMapTrack;
        if (previous == null || previous == _current) return;

        if (_current != null) _current.volume = 0f;
        previous.volume = activeVolume;
        _current = previous;
    }

    /// <summary>Play the world-map theme and forget the history.</summary>
    public void PlayWorldMapMusic()
    {
        EnsureStarted();

        _history.Clear();
        if (worldMapTrack == null || worldMapTrack == _current) return;

        if (_current != null) _current.volume = 0f;
        worldMapTrack.volume = activeVolume;
        _current = worldMapTrack;
    }

    // Start every track playing, looping, and muted — once. Called lazily by the
    // public methods so the first music request also boots the tracks up.
    private void EnsureStarted()
    {
        if (_started) return;
        _started = true;

        if (doorTracks != null)
            for (int i = 0; i < doorTracks.Length; i++)
                if (doorTracks[i] != null) Prepare(doorTracks[i].track, doorTracks[i].clip);

        Prepare(finalTrack, finalClip);
        Prepare(worldMapTrack, worldMapClip);
    }

    // Find the source paired with a given door.
    private AudioSource FindTrack(DoorRoomPedestal door)
    {
        if (door == null || doorTracks == null) return null;

        for (int i = 0; i < doorTracks.Length; i++)
            if (doorTracks[i] != null && doorTracks[i].door == door)
                return doorTracks[i].track;

        return null;
    }

    // Mute the current track, un-mute the new one. Neither stops playing.
    private void MakeAudible(AudioSource track)
    {
        if (track == _current) return;

        if (_current != null) { _history.Push(_current); _current.volume = 0f; }
        track.volume = activeVolume;
        _current = track;
    }

    // Load the clip onto the source, then get it looping, muted, and playing.
    private void Prepare(AudioSource source, AudioClip clip)
    {
        if (source == null) return;
        if (clip != null) source.clip = clip;   // only override if a clip was set
        source.loop = true;
        source.volume = 0f;
        if (source.clip != null && !source.isPlaying) source.Play();
    }
}