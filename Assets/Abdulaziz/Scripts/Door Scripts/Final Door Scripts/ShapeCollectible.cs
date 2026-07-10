using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A poker-shape pickup (one per level). Collection is SERVER-AUTHORITATIVE: the
/// local player requests a pickup (proximity + PlayerInputs.OnInteractInput), the
/// server sets a NetworkVariable, and that replicates so every client — and the
/// FinalDoor — sees it collected.
///
/// On collect the pickup's MeshRenderer is disabled so it vanishes, while the
/// GameObject and its NetworkObject stay active and spawned — nothing in the netcode
/// lifecycle is disturbed and FinalDoor can still read IsCollected.
///
/// Requires a NetworkObject.
/// </summary>
public class ShapeCollectible : NetworkBehaviour
{
    [Header("Shape")]
    [Tooltip("Which shape this pickup represents (Club or Diamond).")]
    [SerializeField] private CubeFace shape;

    [Header("Interaction")]
    [Tooltip("How close the local player must be to collect this.")]
    [SerializeField] private float interactRange = 3f;

    [Tooltip("Optional 'Press E' prompt, hidden until in range.")]
    [SerializeField] private GameObject interactPrompt;

    [Tooltip("The pickup's renderer, disabled on collect so it disappears without " +
             "deactivating the GameObject. Auto-found in children if left empty.")]
    [SerializeField] private MeshRenderer meshRenderer;

    // Server-authoritative collected state, replicated to everyone.
    private readonly NetworkVariable<bool> _netCollected = new NetworkVariable<bool>();

    /// <summary>True once the player has picked this up (synced).</summary>
    public bool IsCollected => _netCollected.Value;

    public CubeFace Shape => shape;

    // Fired on every machine the moment this is collected.
    public event Action<ShapeCollectible> Collected;

    private Transform localPlayer;
    private bool inRange;

    public override void OnNetworkSpawn()
    {
        _netCollected.OnValueChanged += OnCollectedChanged;

        if (meshRenderer == null) meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Late joiners: reflect an already-collected item immediately.
        if (_netCollected.Value) ApplyCollectedVisual();
    }

    public override void OnNetworkDespawn()
    {
        _netCollected.OnValueChanged -= OnCollectedChanged;
    }

    // Subscribe to the permanent interact event.
    private void OnEnable()  { PlayerInputs.OnInteractInput += HandleInteract; }
    private void OnDisable() { PlayerInputs.OnInteractInput -= HandleInteract; }

    private void Update()
    {
        if (_netCollected.Value) return;   // nothing to do once collected
        if (localPlayer == null) { localPlayer = FindLocalPlayer(); return; }

        float dist = Vector3.Distance(localPlayer.position, transform.position);
        bool nowInRange = dist <= interactRange;

        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (interactPrompt != null) interactPrompt.SetActive(inRange);
        }
    }

    // Fired by PlayerInputs when the local player presses Interact.
    private void HandleInteract()
    {
        if (_netCollected.Value) return;
        if (!inRange) return;
        TryCollect();
    }

    /// <summary>Request collection. Public so editor buttons / other systems can
    /// trigger it. Routes to the server for authority.</summary>
    public void Interact() => TryCollect();

    private void TryCollect()
    {
        if (!IsSpawned) return;           // NetworkObject not spawned yet -> can't route through the network
        if (_netCollected.Value) return;

        if (IsServer)
            _netCollected.Value = true;   // host / server-routed: set directly
        else
            CollectServerRpc();           // client: ask the server
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectServerRpc()
    {
        if (_netCollected.Value) return;
        _netCollected.Value = true;       // replicates -> OnCollectedChanged everywhere
    }

    private void OnCollectedChanged(bool previous, bool current)
    {
        if (!current) return;
        Collected?.Invoke(this);
        ApplyCollectedVisual();
    }

    // Hide the pickup by disabling its renderer. The GameObject and NetworkObject
    // stay active/spawned, so nothing in the netcode lifecycle breaks and FinalDoor
    // can still read IsCollected.
    private void ApplyCollectedVisual()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (meshRenderer != null) meshRenderer.enabled = false;
    }

    private Transform FindLocalPlayer()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null && nm.LocalClient != null && nm.LocalClient.PlayerObject != null)
            return nm.LocalClient.PlayerObject.transform;
        return null;
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = inRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}