using UnityEngine;

/// <summary>
/// Teleports the player to a specified transform when they enter this
/// trigger volume. Put it on an object with a Collider marked Is Trigger.
/// </summary>
public class TeleportTrigger : MonoBehaviour
{
    [Tooltip("Where the player is sent on contact.")]
    [SerializeField] private Transform destination;

    [Tooltip("The tag the trigger uses to recognize the player.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Seconds before this trigger can teleport again. Prevents an " +
             "instant re-trigger loop if the destination overlaps another trigger.")]
    [SerializeField] private float cooldown = 1f;

    private float _lastTeleportTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - _lastTeleportTime < cooldown) return;
        if (!other.CompareTag(playerTag)) return;
        if (destination == null) { Debug.LogWarning($"{name}: no Destination assigned.", this); return; }

        _lastTeleportTime = Time.time;
        Teleport(other.transform);
    }

    private void Teleport(Transform player)
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

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

        if (destination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, destination.position);
            Gizmos.DrawWireSphere(destination.position, 0.3f);
        }
    }
}