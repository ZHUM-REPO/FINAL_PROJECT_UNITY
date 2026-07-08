using UnityEngine;

/// <summary>
/// Watches one or more ShapeCollectibles and enables a disabled door once they're
/// collected, so the player can head back to the start. Lives on an always-active
/// object (NOT on the door itself — a disabled GameObject can't run OnEnable to
/// hear the events). Subscribes to each collectible's Collected event in OnEnable
/// and unsubscribes in OnDisable, with an IsCollected sync for anything already
/// picked up before this was enabled.
/// </summary>
public class CollectibleReturnDoor : MonoBehaviour
{
    [Header("Requirements")]
    [Tooltip("Every collectible here must be collected for the door to open. " +
             "Assign one, or several to require them all.")]
    [SerializeField] private ShapeCollectible[] requiredItems;

    [Header("Door")]
    [Tooltip("The door GameObject to enable once collected. Start it disabled.")]
    [SerializeField] private GameObject door;

    [Tooltip("If on, the door stays open once opened. If off, it tracks the " +
             "collected state live (rarely needed, since pickups don't un-collect).")]
    [SerializeField] private bool stayOpenOnceCollected = true;

    private bool _opened;

    private void OnEnable()
    {
        if (requiredItems != null)
        {
            for (int i = 0; i < requiredItems.Length; i++)
                if (requiredItems[i] != null) requiredItems[i].Collected += HandleCollected;
        }

        // Apply the correct door state now: closed until collected, open the
        // moment the requirement is met (or already met when this is enabled).
        Evaluate();
    }

    private void OnDisable()
    {
        if (requiredItems == null) return;
        for (int i = 0; i < requiredItems.Length; i++)
            if (requiredItems[i] != null) requiredItems[i].Collected -= HandleCollected;
    }

    private void HandleCollected(ShapeCollectible item) => Evaluate();

    private void Evaluate()
    {
        bool shouldOpen;

        // Once opened with stay-open on, it stays open for good — even across a
        // disable/re-enable of this controller.
        if (_opened && stayOpenOnceCollected)
        {
            shouldOpen = true;
        }
        else
        {
            shouldOpen = AllCollected();
            if (shouldOpen) _opened = true;
        }

        // Drive the door to the correct state every time, so collecting the
        // required items always enables it.
        if (door != null) door.SetActive(shouldOpen);
    }

    private bool AllCollected()
    {
        if (requiredItems == null || requiredItems.Length == 0) return false;

        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] == null || !requiredItems[i].IsCollected)
                return false;
        }
        return true;
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Magenta lines: the collectibles this door is waiting on.
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

        // Green line: the door this controller enables.
        if (door != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, door.transform.position);
            Gizmos.DrawWireCube(door.transform.position, Vector3.one * 0.5f);
        }
    }
}