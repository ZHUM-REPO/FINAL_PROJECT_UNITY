using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the game's music. Every track plays continuously and loops from the start —
/// the manager never stops any of them, it only changes volume: the active track at
/// Active Volume, the rest at 0. Each track is paired with the door that triggers it
/// and the clip it should play, so passing through a specific door makes that
/// specific clip audible.
///
/// Put this on one always-active object and drag it into the doors that switch music.
/// </summary>
public class MusicManager : MonoBehaviour
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
                 "onto it, loops it, and mutes it at launch until the door is used.")]
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
    [Tooltip("The default clip: audible at start and restored when the player " +
             "leaves a level.")]
    [SerializeField] private AudioClip worldMapClip;

    [Tooltip("The AudioSource the world-map clip plays through.")]
    [SerializeField] private AudioSource worldMapTrack;

    [Header("Volume")]
    [Tooltip("Volume of the audible track. Muted tracks are set to 0.")]
    [SerializeField] private float activeVolume = 1f;

    // What's audible now, and the trail to fall back through when leaving levels.
    private AudioSource _current;
    private readonly Stack<AudioSource> _history = new Stack<AudioSource>();

    private void Awake()
    {
        // World map FIRST, so its clip is playing the instant the scene loads.
        Prepare(worldMapTrack, worldMapClip);
        if (worldMapTrack != null)
        {
            worldMapTrack.volume = activeVolume;
            _current = worldMapTrack;
        }

        // Then start every other track playing + looping + muted in the background,
        // so none of them ever stop and switching is just a volume change.
        if (doorTracks != null)
            for (int i = 0; i < doorTracks.Length; i++)
                if (doorTracks[i] != null) Prepare(doorTracks[i].track, doorTracks[i].clip);

        Prepare(finalTrack, finalClip);
    }

    /// <summary>Called by a level door when the player passes through it. Plays the
    /// clip/source paired with that door in the Door Tracks list.</summary>
    public void PlayForDoor(DoorRoomPedestal door)
    {
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
        AudioSource previous = _history.Count > 0 ? _history.Pop() : worldMapTrack;
        if (previous == null || previous == _current) return;

        if (_current != null) _current.volume = 0f;
        previous.volume = activeVolume;
        _current = previous;
    }

    /// <summary>Jump straight back to the world-map theme and forget the history.</summary>
    public void PlayWorldMapMusic()
    {
        _history.Clear();
        if (worldMapTrack == null || worldMapTrack == _current) return;

        if (_current != null) _current.volume = 0f;
        worldMapTrack.volume = activeVolume;
        _current = worldMapTrack;
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

    // Load the clip onto the source, then get it looping, muted, and playing so
    // it's ready to be un-muted later.
    private void Prepare(AudioSource source, AudioClip clip)
    {
        if (source == null) return;
        if (clip != null) source.clip = clip;   // only override if a clip was set
        source.loop = true;
        source.volume = 0f;
        if (source.clip != null && !source.isPlaying) source.Play();
    }
}