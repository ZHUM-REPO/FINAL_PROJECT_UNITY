using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Standalone reaction: when the scale is solved, drop a reward object down so the
/// player can pick it up. Only the SERVER performs the drop (it moves the reward),
/// and the reward's movement replicates to clients via its NetworkTransform — so
/// everyone sees it fall to the same spot. Clients don't move it themselves.
///
/// Stays a plain MonoBehaviour; it checks NetworkManager for server authority.
/// The REWARD object needs a NetworkObject + NetworkTransform for the drop to sync.
/// </summary>
[DisallowMultipleComponent]
public class ScaleReward : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("The scale to watch. Drops the reward when it gets solved.")]
    [SerializeField] private BalanceScale scale;

    [Header("Reward")]
    [Tooltip("The object that drops down. Give it a ShapeCollectible so the " +
             "player can pick it up, plus a NetworkObject + NetworkTransform.")]
    [SerializeField] private Transform reward;

    [Tooltip("Where the reward drops to — a spot within the player's reach.")]
    [SerializeField] private Transform dropTarget;

    [Tooltip("How long (seconds) the drop takes.")]
    [SerializeField] private float dropDuration = 0.75f;

    private bool _dropped;
    private Coroutine _dropRoutine;

    // Only the server drives the drop; the NetworkTransform replicates it.
    private bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

    private void OnEnable()
    {
        if (scale != null) scale.BalanceChanged += HandleBalanceChanged;
    }

    private void OnDisable()
    {
        if (scale != null) scale.BalanceChanged -= HandleBalanceChanged;

        if (_dropRoutine != null && reward != null && dropTarget != null)
        {
            reward.position = dropTarget.position;
            _dropRoutine = null;
        }
    }

    private void HandleBalanceChanged(bool balanced)
    {
        // BalanceChanged fires on every machine, but only the server should move
        // the reward — clients receive the movement through NetworkTransform.
        if (!IsServer) return;

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
            reward.position = Vector3.Lerp(start, end, t * t);   // ease-in like a fall
            yield return null;
        }

        reward.position = end;
        _dropRoutine = null;
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos || reward == null || dropTarget == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(reward.position, dropTarget.position);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(dropTarget.position, 0.25f);
    }
}