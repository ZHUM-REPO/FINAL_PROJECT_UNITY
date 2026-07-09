using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A weighable prop. Its logical weight is SERVER-AUTHORITATIVE, stored in a
/// NetworkVariable so every client agrees on it. The magic script must change the
/// weight on the server (directly, or through a ServerRpc) — clients can't write it.
///
/// Requires a NetworkObject. For a pan on the server to detect this object where
/// clients actually see it, this object also needs its movement synced
/// (NetworkTransform), since the server scans using its own physics positions.
/// </summary>
public class WeightedObject : NetworkBehaviour
{
    [Tooltip("Starting logical weight. Seeded into the networked weight on spawn. " +
             "Not kilograms, just a number.")]
    [SerializeField] private float weight = 1f;

    [Tooltip("Optional ID so a scale can require *specific* objects, not just matching totals.")]
    [SerializeField] private string objectId = "";

    // Server-authoritative weight, replicated to all clients.
    private readonly NetworkVariable<float> _netWeight = new NetworkVariable<float>();

    /// Fires (on every machine) whenever the weight changes so pans can recalculate.
    public event System.Action<WeightedObject> WeightChanged;

    /// Fired in OnDisable so a pan can drop this object immediately.
    public event System.Action<WeightedObject> Disabled;

    public string ObjectId => objectId;

    /// <summary>
    /// The networked weight. Getter reads the synced value on any machine.
    /// Setter is SERVER-ONLY (writing the NetworkVariable) — call it from the
    /// magic script on the server, e.g. inside a ServerRpc.
    /// </summary>
    public float Weight
    {
        get => _netWeight.Value;
        set
        {
            if (!IsServer)
            {
                Debug.LogWarning($"{name}: Weight can only be set on the server.", this);
                return;
            }
            if (Mathf.Approximately(_netWeight.Value, value)) return;
            _netWeight.Value = value;   // replicates -> OnValueChanged -> WeightChanged
        }
    }

    public override void OnNetworkSpawn()
    {
        // Seed the starting weight on the server BEFORE subscribing, so the initial
        // value doesn't count as a change.
        if (IsServer) _netWeight.Value = weight;

        _netWeight.OnValueChanged += OnNetWeightChanged;
    }

    public override void OnNetworkDespawn()
    {
        _netWeight.OnValueChanged -= OnNetWeightChanged;
    }

    private void OnNetWeightChanged(float previous, float current) => WeightChanged?.Invoke(this);

    /// <summary>Force a refresh if the weight was changed some other way.</summary>
    public void NotifyWeightChanged() => WeightChanged?.Invoke(this);

    private void OnDisable() => Disabled?.Invoke(this);
}