using UnityEngine;

/// <summary>
/// Keeps a door disabled until every assigned balance scale is solved (balanced),
/// then enables it so the player can go back. Lives on an always-active object
/// (NOT on the door itself — a disabled GameObject can't run OnEnable to hear
/// the events). It subscribes to each scale's BalanceChanged in OnEnable and
/// unsubscribes in OnDisable.
/// </summary>
public class ScaleReturnDoor : MonoBehaviour
{
    [Header("Requirements")]
    [Tooltip("Every scale here must be balanced for the door to open. " +
             "Assign one, or several to require them all.")]
    [SerializeField] private BalanceScale[] scales;

    [Header("Door")]
    [Tooltip("The door GameObject to enable once the puzzle is solved. " +
             "Start it disabled in the scene.")]
    [SerializeField] private GameObject door;

    [Tooltip("If on, the door stays open once solved even if a scale is later " +
             "disturbed. If off, it closes again whenever a scale goes unbalanced.")]
    [SerializeField] private bool stayOpenOnceSolved = true;

    private bool _opened;

    private void OnEnable()
    {
        if (scales != null)
        {
            for (int i = 0; i < scales.Length; i++)
                if (scales[i] != null) scales[i].BalanceChanged += HandleBalanceChanged;
        }

        // Apply the correct door state now: closed until solved, open the moment
        // the puzzle is solved (or already solved when this is enabled).
        Evaluate();
    }

    private void OnDisable()
    {
        if (scales == null) return;
        for (int i = 0; i < scales.Length; i++)
            if (scales[i] != null) scales[i].BalanceChanged -= HandleBalanceChanged;
    }

    private void HandleBalanceChanged(bool balanced) => Evaluate();

    private void Evaluate()
    {
        bool shouldOpen;

        // Once solved with stay-open on, it stays open for good — even across a
        // disable/re-enable of this controller.
        if (_opened && stayOpenOnceSolved)
        {
            shouldOpen = true;
        }
        else
        {
            shouldOpen = AllBalanced();
            if (shouldOpen) _opened = true;
        }

        // Drive the door to the correct state every time, so solving the puzzle
        // always enables it.
        if (door != null) door.SetActive(shouldOpen);
    }

    private bool AllBalanced()
    {
        if (scales == null || scales.Length == 0) return false;

        for (int i = 0; i < scales.Length; i++)
        {
            if (scales[i] == null || !scales[i].IsBalanced)
                return false;
        }
        return true;
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Magenta lines: the scales this gate is waiting on.
        if (scales != null)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < scales.Length; i++)
            {
                if (scales[i] == null) continue;
                Gizmos.DrawLine(transform.position, scales[i].transform.position);
            }
        }

        // Green line: the door this gate controls.
        if (door != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, door.transform.position);
            Gizmos.DrawWireCube(door.transform.position, Vector3.one * 0.5f);
        }
    }
}