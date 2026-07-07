using System;
using UnityEngine;

/// <summary>
/// The heart door at the end of the puzzle. Walking through it checks the
/// required collectibles:
///  - all collected  -> teleport to the next area.
///  - any missing    -> teleport back to spawn and fire Denied so the UI
///                      can show the "not all shapes" message.
/// </summary>
public class FinalDoor : MonoBehaviour
{
    [Header("Requirements")]
    [Tooltip("The poker-shape objects the player must have collected. " +
             "Drag the club and diamond ShapeCollectibles in here.")]
    [SerializeField] private ShapeCollectible[] requiredItems;

    [Header("Destinations")]
    [Tooltip("Where the player goes when all shapes are collected.")]
    [SerializeField] private Transform nextArea;

    [Tooltip("Where the player goes when shapes are missing " +
             "(the spawn point plane/transform).")]
    [SerializeField] private Transform spawnPoint;

    [Header("Player")]
    [Tooltip("The tag the door uses to recognize the player.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Seconds before the door can teleport again.")]
    [SerializeField] private float teleportCooldown = 1f;

    private float _lastTeleportTime = -999f;

    // Fired when the player is turned away for missing shapes.
    public event Action Denied;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - _lastTeleportTime < teleportCooldown) return;
        if (!other.CompareTag(playerTag)) return;

        _lastTeleportTime = Time.time;

        if (AllCollected())
        {
            Teleport(other.transform, nextArea);
        }
        else
        {
            Teleport(other.transform, spawnPoint);
            Denied?.Invoke();
        }
    }

    // Public so the custom editor's "Confirm" button can check it. Returns true
    // when every assigned collectible reports IsCollected.
    public bool AllCollected()
    {
        if (requiredItems == null || requiredItems.Length == 0) return true;

        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] != null && !requiredItems[i].IsCollected)
                return false;
        }
        return true;
    }

    // Read-only access to the requirement list for the editor's status report.
    public ShapeCollectible[] RequiredItems => requiredItems;

    private void Teleport(Transform player, Transform destination)
    {
        if (destination == null) return;

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

        // Trigger volume, colored by health:
        //   green  = collider present AND Is Trigger on  -> good to go
        //   red    = collider present but NOT a trigger  -> OnTriggerEnter won't fire
        //   red X  = no collider at all                  -> nothing to detect the player
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col.isTrigger)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
            else
            {
                // Collider exists but Is Trigger is off — the door can't detect the player.
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                DrawWarningCross(col.bounds.center, col.bounds.size);
            }
        }
        else
        {
            // No collider at all — draw a warning marker at the door itself.
            Gizmos.color = Color.red;
            Vector3 size = Vector3.one;
            Gizmos.DrawWireCube(transform.position, size);
            DrawWarningCross(transform.position, size);
        }

        // Magenta lines: the collectibles this door requires.
        if (requiredItems != null)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < requiredItems.Length; i++)
            {
                if (requiredItems[i] == null) continue;
                Gizmos.DrawLine(transform.position, requiredItems[i].transform.position);
                Gizmos.DrawWireSphere(requiredItems[i].transform.position, 0.25f);
            }
        }

        // Green line: the next area on success.
        if (nextArea != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextArea.position);
            Gizmos.DrawWireSphere(nextArea.position, 0.3f);
        }

        // Red line: where a denial sends the player.
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, spawnPoint.position);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
        }
    }

    // Draws a big X across the box so a bad trigger is obvious in the Scene view.
    private void DrawWarningCross(Vector3 center, Vector3 size)
    {
        Vector3 h = size * 0.5f;
        Gizmos.DrawLine(center + new Vector3(-h.x, -h.y, 0f), center + new Vector3(h.x, h.y, 0f));
        Gizmos.DrawLine(center + new Vector3(h.x, -h.y, 0f), center + new Vector3(-h.x, h.y, 0f));
    }
}