using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The puzzle brain, server-authoritative. The SERVER compares the two pans'
/// totals and writes the result into NetworkVariables (solved state + cosmetic
/// tilt). Every client reads those, so the balance result and the beam tilt agree
/// on all machines. Reactions (color, reward, door) consume BalanceChanged /
/// IsBalanced, which now reflect the synced state.
///
/// Requires a NetworkObject on this GameObject.
/// </summary>
public class BalanceScale : NetworkBehaviour
{
    [Header("Pans")]
    [SerializeField] private ScalePan leftPan;
    [SerializeField] private ScalePan rightPan;

    [Header("Balance Rule")]
    [Tooltip("How close the two totals must be to count as balanced.")]
    [SerializeField] private float balanceTolerance = 0.01f;

    [Tooltip("Optional: also require these exact object IDs to be present. " +
             "Leave empty to balance purely by weight.")]
    [SerializeField] private string[] requiredObjectIds = new string[0];

    [Header("Cosmetic Tilt (no physics)")]
    [SerializeField] private Transform beam;
    [SerializeField] private float maxTiltAngle = 20f;
    [SerializeField] private float weightForMaxTilt = 5f;
    [SerializeField] private float tiltLerpSpeed = 5f;
    [SerializeField] private Vector3 tiltAxis = new Vector3(0f, 0f, 1f);

    [Header("Puzzle Events")]
    public UnityEvent OnBalanced;
    public UnityEvent OnUnbalanced;
    public UnityEvent OnTipLeft;
    public UnityEvent OnTipRight;

    /// <summary>Fires (on every machine) when the balanced state changes.</summary>
    public event System.Action<bool> BalanceChanged;

    // Networked puzzle state. Server writes, everyone reads.
    private const int StateUnset = 0, StateBalanced = 1, StateTipLeft = 2, StateTipRight = 3;
    private readonly NetworkVariable<int> _netState = new NetworkVariable<int>(StateUnset);
    private readonly NetworkVariable<float> _netTilt = new NetworkVariable<float>();

    private bool _prevBalanced;

    public bool IsBalanced => _netState.Value == StateBalanced;

    public override void OnNetworkSpawn()
    {
        // Record the starting state BEFORE subscribing, so the initial baseline
        // doesn't fire events (an empty scale reads balanced otherwise).
        if (IsServer)
        {
            int baseline = ComputeState(out float tilt);
            _netState.Value = baseline;
            _netTilt.Value = tilt;
        }

        _prevBalanced = IsBalanced;
        _netState.OnValueChanged += OnStateChanged;

        // Subscribe to pans on the server only (they only scan on the server).
        if (IsServer)
        {
            if (leftPan != null) leftPan.ContentsChanged += Evaluate;
            if (rightPan != null) rightPan.ContentsChanged += Evaluate;
        }
    }

    public override void OnNetworkDespawn()
    {
        _netState.OnValueChanged -= OnStateChanged;

        if (IsServer)
        {
            if (leftPan != null) leftPan.ContentsChanged -= Evaluate;
            if (rightPan != null) rightPan.ContentsChanged -= Evaluate;
        }
    }

    /// <summary>
    /// Re-checks balance on the SERVER and updates the networked state.
    /// Public so the magic script (running on the server) can force a re-check.
    /// </summary>
    public void Evaluate()
    {
        if (!IsServer) return;
        if (leftPan == null || rightPan == null) return;

        int state = ComputeState(out float tilt);
        _netTilt.Value = tilt;
        if (_netState.Value != state) _netState.Value = state;
    }

    // Pure computation of the current state + cosmetic tilt (server-side).
    private int ComputeState(out float tilt)
    {
        if (leftPan == null || rightPan == null) { tilt = 0f; return StateUnset; }

        float diff = leftPan.TotalWeight - rightPan.TotalWeight; // + = left heavier

        float t = Mathf.Clamp(diff / Mathf.Max(0.0001f, weightForMaxTilt), -1f, 1f);
        tilt = t * maxTiltAngle;

        bool weightsMatch = Mathf.Abs(diff) <= balanceTolerance;
        if (weightsMatch && RequiredObjectsPresent()) return StateBalanced;
        return diff >= 0f ? StateTipLeft : StateTipRight;
    }

    // Runs on EVERY machine when the server changes the state -> fire events locally.
    private void OnStateChanged(int previous, int current)
    {
        bool balancedNow = current == StateBalanced;

        if (balancedNow != _prevBalanced)
        {
            _prevBalanced = balancedNow;
            BalanceChanged?.Invoke(balancedNow);
            if (balancedNow) OnBalanced?.Invoke();
            else OnUnbalanced?.Invoke();
        }

        if (current == StateTipLeft) OnTipLeft?.Invoke();
        else if (current == StateTipRight) OnTipRight?.Invoke();
    }

    private bool RequiredObjectsPresent()
    {
        if (requiredObjectIds == null || requiredObjectIds.Length == 0) return true;
        for (int i = 0; i < requiredObjectIds.Length; i++)
        {
            string id = requiredObjectIds[i];
            if (string.IsNullOrEmpty(id)) continue;
            if (!leftPan.Contains(id) && !rightPan.Contains(id)) return false;
        }
        return true;
    }

    private void Update()
    {
        if (beam == null) return;

        // Every machine lerps the beam toward the networked tilt value.
        Vector3 e = beam.localEulerAngles;
        float currentZ = e.z > 180f ? e.z - 360f : e.z;
        float nextZ = Mathf.Lerp(currentZ, _netTilt.Value, Time.deltaTime * tiltLerpSpeed);

        Quaternion baseRot = Quaternion.Euler(e.x * (1 - tiltAxis.x), e.y * (1 - tiltAxis.y), 0f);
        beam.localRotation = baseRot * Quaternion.AngleAxis(nextZ, tiltAxis.normalized);
    }
}