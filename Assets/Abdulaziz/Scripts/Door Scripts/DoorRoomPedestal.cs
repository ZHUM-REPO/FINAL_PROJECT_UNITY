using UnityEngine;

/// <summary>
/// A doorway marked with a card shape. When the player walks through it,
/// the door checks the linked pedestal's cube:
///  - face matches this door's shape -> teleport to the intended level.
///  - face doesn't match             -> teleport back to the spawn point.
/// Plays a door-opening sound as the player goes through, then tells the
/// MusicManager which track should be audible.
/// </summary>
public class DoorRoomPedestal : MonoBehaviour
{
    [Header("Shape")]
    [Tooltip("The card shape displayed on this door. The cube must show " +
             "this face for the door to lead to its level.")]
    [SerializeField] private CubeFace requiredFace;

    [Tooltip("The CubePedestal whose cube must match this door's shape.")]
    [SerializeField] private CubePedestal linkedPedestal;

    [Header("Destinations")]
    [Tooltip("Where the player goes when the cube's face matches this door.")]
    [SerializeField] private Transform levelDestination;

    [Tooltip("Where the player goes when the face does NOT match " +
             "(the spawn point plane/transform).")]
    [SerializeField] private Transform spawnPoint;

    [Header("Player")]
    [Tooltip("The tag the door uses to recognize the player.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Seconds before the door can teleport again. Prevents an " +
             "instant re-trigger loop if a destination overlaps another trigger.")]
    [SerializeField] private float teleportCooldown = 1f;

    [Header("Audio")]
    [Tooltip("Source that plays the door-opening sound.")]
    [SerializeField] private AudioSource doorOpenSource;

    [Tooltip("Sound played as the door opens / the player goes through.")]
    [SerializeField] private AudioClip doorOpenClip;

    [Header("Music")]
    [Tooltip("The manager that controls which track is audible. This door must be " +
             "paired with its track in the manager's Door Tracks list.")]
    [SerializeField] private MusicManager musicManager;

    [Header("Debug")]
    [Tooltip("Logs each trigger entry and the match decision to the Console. " +
             "Turn off once it works.")]
    [SerializeField] private bool debugLogs = true;

    private float _lastTeleportTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        // If NOTHING logs here when you walk through, the trigger isn't firing:
        // the door's collider needs Is Trigger ON, and must be on THIS object.
        if (debugLogs) Debug.Log($"{name}: OnTriggerEnter by '{other.name}' (tag '{other.tag}').", this);

        if (Time.time - _lastTeleportTime < teleportCooldown) return;
        if (!other.CompareTag(playerTag))
        {
            if (debugLogs) Debug.Log($"{name}: ignored — '{other.name}' is not tagged '{playerTag}'.", this);
            return;
        }
        if (linkedPedestal == null) { Debug.LogWarning($"{name}: no Linked Pedestal assigned.", this); return; }

        bool match = linkedPedestal.CurrentFace == requiredFace;

        if (debugLogs)
            Debug.Log($"{name}: cube face = {linkedPedestal.CurrentFace}, door needs {requiredFace} -> {(match ? "MATCH" : "mismatch")}.", this);

        // The whole puzzle in one line: right face -> the level, wrong face -> spawn.
        Transform destination = match ? levelDestination : spawnPoint;

        if (destination == null)
        {
            // This is the silent killer: a match with no Level Destination (or a
            // mismatch with no Spawn Point) assigned does nothing at all.
            Debug.LogWarning($"{name}: {(match ? "Level Destination" : "Spawn Point")} is not assigned — nowhere to send the player.", this);
            return;
        }

        _lastTeleportTime = Time.time;

        PlayDoorOpen();                        // door-opening sound as they go through
        Teleport(other.transform, destination);
        UpdateMusic(match);                    // switch the audible track AFTER teleport
    }

    private void Teleport(Transform player, Transform destination)
    {
        // A CharacterController fights direct transform moves, so disable it
        // while repositioning, then turn it back on.
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.SetPositionAndRotation(destination.position, destination.rotation);

        if (cc != null) cc.enabled = true;
    }

    private void PlayDoorOpen()
    {
        if (doorOpenSource == null || doorOpenClip == null)
        {
            Debug.LogError($"{name}: door-opening audio isn't fully assigned (source and/or clip missing).", this);
            return;
        }

        doorOpenSource.PlayOneShot(doorOpenClip);
    }

    private void UpdateMusic(bool match)
    {
        if (musicManager == null)
        {
            Debug.LogError($"{name}: no Music Manager assigned — can't switch tracks.", this);
            return;
        }

        // Match sends the player INTO the level -> this door's track (looked up
        // in the manager's Door Tracks list). Mismatch sends them back to spawn
        // (world map) -> world-map track.
        if (match)
            musicManager.PlayForDoor(this);
        else
            musicManager.PlayWorldMapMusic();
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Trigger box: green if a collider is present and Is Trigger is ON,
        // red if it's missing or NOT a trigger (OnTriggerEnter won't fire).
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        else
        {
            Vector3 c = col != null ? col.bounds.center : transform.position;
            Vector3 s = col != null ? col.bounds.size : Vector3.one;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(c, s);
            Vector3 h = s * 0.5f;
            Gizmos.DrawLine(c + new Vector3(-h.x, -h.y, 0f), c + new Vector3(h.x, h.y, 0f));
            Gizmos.DrawLine(c + new Vector3(h.x, -h.y, 0f), c + new Vector3(-h.x, h.y, 0f));
        }

        // Yellow line: which pedestal controls this door.
        if (linkedPedestal != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, linkedPedestal.transform.position);
        }

        // Green line: the level this door leads to on a match.
        if (levelDestination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, levelDestination.position);
            Gizmos.DrawWireSphere(levelDestination.position, 0.3f);
        }

        // Red line: where a mismatch sends the player.
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, spawnPoint.position);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
        }
    }
}