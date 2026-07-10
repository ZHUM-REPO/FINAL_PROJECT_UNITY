using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Moves the movable heart half to a target position (the hidden platform) when a
/// BalanceScale becomes solved. No Rigidbody — just transform motion toward the target
/// via MoveTowards. Server-authoritative: the server drives the move and a
/// NetworkTransform on this object replicates it, so both players see the same thing.
/// The other half has no mover, so it stays put.
///
/// Requires a NetworkObject + NetworkTransform on this GameObject.
/// </summary>
public class HeartHalfMover : NetworkBehaviour
{
    [Header("Trigger")]
    [Tooltip("The scale that starts the move when it becomes balanced/solved. " +
             "Leave empty to trigger manually via MoveToTarget().")]
    [SerializeField] private BalanceScale scale;

    [Header("Target")]
    [Tooltip("Where the heart half moves to (the hidden platform's transform).")]
    [SerializeField] private Transform target;

    [Header("Motion")]
    [Tooltip("Units per second the half travels toward the target.")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Distance from the target that counts as 'arrived'.")]
    [SerializeField] private float arriveDistance = 0.02f;

    [Tooltip("Start moving the moment this spawns, ignoring the scale.")]
    [SerializeField] private bool moveOnSpawn = false;

    // Server-only flag: currently travelling toward the target.
    private bool _moving;

    // Fired on the server when the half reaches the platform.
    public event Action Arrived;

    // BalanceChanged is a plain C# event, safe to (un)subscribe with the Unity
    // enable/disable lifecycle. The handler gates on server authority itself.
    private void OnEnable()  { if (scale != null) scale.BalanceChanged += HandleBalanceChanged; }
    private void OnDisable() { if (scale != null) scale.BalanceChanged -= HandleBalanceChanged; }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Catch cases where we can't rely on a future transition: an explicit
        // move-on-spawn, or a scale that is already solved by the time we spawn.
        if (moveOnSpawn || (scale != null && scale.IsBalanced))
            StartMove();
    }

    // Runs on every machine when the scale's balanced state flips; only the server
    // actually drives the movement (NetworkTransform replicates it to clients).
    private void HandleBalanceChanged(bool balanced)
    {
        if (!IsServer) return;
        if (balanced) StartMove();
    }

    /// <summary>Begin moving to the target. Safe to call from a client — routes to the server.</summary>
    public void MoveToTarget()
    {
        if (!IsSpawned) return;

        if (IsServer) StartMove();
        else          MoveToTargetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveToTargetServerRpc() => StartMove();

    // Server-side move kickoff, with a loud warning for the usual silent failure.
    private void StartMove()
    {
        if (target == null)
        {
            Debug.LogWarning($"{name}: HeartHalfMover has no Target assigned — nothing to move toward.", this);
            return;
        }
        _moving = true;
    }

    private void Update()
    {
        if (!IsServer || !_moving || target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position, target.position, moveSpeed * Time.deltaTime);

        if ((transform.position - target.position).sqrMagnitude <= arriveDistance * arriveDistance)
        {
            transform.position = target.position;
            _moving = false;
            Arrived?.Invoke();
        }
    }
}