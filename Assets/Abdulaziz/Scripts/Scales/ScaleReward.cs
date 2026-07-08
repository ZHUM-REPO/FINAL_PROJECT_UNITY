using System.Collections;
using UnityEngine;

/// <summary>
/// Standalone reaction: when the scale becomes balanced (solved), drop a reward
/// object down so the player can pick it up. The reward carries a ShapeCollectible,
/// so collecting it counts toward opening the FinalDoor.
///
/// Same pattern as ScaleColorReaction — subscribes to BalanceChanged in OnEnable,
/// unsubscribes in OnDisable — so it can sit alongside other reactions on one scale.
/// The drop is coroutine-driven (no Rigidbody), matching the falling-object approach
/// used elsewhere in the project.
/// </summary>
[DisallowMultipleComponent]
public class ScaleReward : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("The scale to watch. Drops the reward when it gets solved.")]
    [SerializeField] private BalanceScale scale;

    [Header("Reward")]
    [Tooltip("The object that drops down. Give it a ShapeCollectible so the " +
             "player can pick it up for the final door.")]
    [SerializeField] private Transform reward;

    [Tooltip("Where the reward drops to — a spot within the player's reach.")]
    [SerializeField] private Transform dropTarget;

    [Tooltip("How long (seconds) the drop takes.")]
    [SerializeField] private float dropDuration = 0.75f;

    private bool _dropped;
    private Coroutine _dropRoutine;

    private void OnEnable()
    {
        if (scale != null) scale.BalanceChanged += HandleBalanceChanged;
    }

    private void OnDisable()
    {
        if (scale != null) scale.BalanceChanged -= HandleBalanceChanged;

        // If disabled mid-drop, snap to the target so it can't hang in the air.
        if (_dropRoutine != null && reward != null && dropTarget != null)
        {
            reward.position = dropTarget.position;
            _dropRoutine = null;
        }
    }

    private void HandleBalanceChanged(bool balanced)
    {
        // Drop once, the first time the scale is solved. A later unbalance
        // won't raise it back up — a collected reward shouldn't reset.
        if (!balanced || _dropped) return;
        if (reward == null || dropTarget == null) return;

        _dropped = true;
        _dropRoutine = StartCoroutine(DropReward());
    }

    private IEnumerator DropReward()
    {
        Vector3 start = reward.position;
        Vector3 end = dropTarget.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dropDuration;
            // t * t eases in, so it accelerates like a fall.
            reward.position = Vector3.Lerp(start, end, t * t);
            yield return null;
        }

        reward.position = end;   // land exactly on target
        _dropRoutine = null;
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos || reward == null || dropTarget == null) return;

        // Yellow line: the path the reward will fall along.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(reward.position, dropTarget.position);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(dropTarget.position, 0.25f);
    }
}