using UnityEngine;

/// <summary>
/// A doorway marked with a card shape. When the player walks through it,
/// the door checks the linked pedestal's cube:
///  - face matches this door's shape -> teleport to the intended level.
///  - face doesn't match             -> teleport back to the spawn point.
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

    private float _lastTeleportTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - _lastTeleportTime < teleportCooldown) return;
        if (!other.CompareTag(playerTag)) return;
        if (linkedPedestal == null) { Debug.LogWarning($"{name}: no Linked Pedestal assigned.", this); return; }

        // The whole puzzle in one line: right face -> the level, wrong face -> spawn.
        Transform destination = (linkedPedestal.CurrentFace == requiredFace)
            ? levelDestination
            : spawnPoint;

        if (destination == null) return;

        _lastTeleportTime = Time.time;
        Teleport(other.transform, destination);
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

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Cyan box: the door's trigger volume.
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
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