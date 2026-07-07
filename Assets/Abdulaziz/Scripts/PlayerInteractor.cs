using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The owning player aims a ray from their camera and, when the Interact action
/// fires, asks the SERVER to run the interaction (server-authoritative). Detection
/// must happen on the owner because the camera/aim is local, but the actual
/// Interact() runs on the server so the result is consistent for all players.
///
/// Interactable objects that go through the server must have a NetworkObject and an
/// IInteractable. Non-networked interactables fall back to a local Interact().
/// Input comes from PlayerInputs.OnInteractInput (new Input System).
/// </summary>
public class PlayerInteractor : NetworkBehaviour
{
    [Header("Aim")]
    [Tooltip("The camera the ray is cast from. If left empty, the main camera is " +
             "used and re-found automatically whenever it changes (e.g. new scene). " +
             "You can also set it explicitly from your spawn code via SetCamera().")]
    [SerializeField] private Camera aimCamera;

    [Tooltip("How far the player can reach to interact.")]
    [SerializeField] private float interactRange = 3f;

    [Tooltip("Which layers the ray can hit. Set this to the layers your " +
             "interactable objects live on.")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Tooltip("Whether the ray should hit trigger colliders.")]
    [SerializeField] private bool hitTriggers = false;

    // What the owner is currently aiming at.
    private IInteractable _current;
    private NetworkObject _currentNetObj;

    /// <summary>Set the camera explicitly — call this from your spawn code once
    /// the player's camera exists.</summary>
    public void SetCamera(Camera cam) => aimCamera = cam;

    private void OnEnable()
    {
        PlayerInputs.OnInteractInput += HandleInteract;
    }

    private void OnDisable()
    {
        PlayerInputs.OnInteractInput -= HandleInteract;
    }

    private void Update()
    {
        // Only the owner aims — its camera is the one that matters, and its
        // PlayerInputs is the only one that fires OnInteractInput.
        if (!IsOwner) return;
        UpdateAim(ResolveCamera());
    }

    private void HandleInteract()
    {
        if (!IsOwner) return;
        if (_current == null) return;

        // Server-authoritative: if the target is networked, ask the server to run
        // the interaction. Otherwise interact locally (non-networked object).
        if (_currentNetObj != null)
            InteractServerRpc(_currentNetObj.NetworkObjectId);
        else
            _current.Interact();
    }

    [ServerRpc]
    private void InteractServerRpc(ulong targetNetworkObjectId)
    {
        // Run the interaction on the server instance of the target object.
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject netObj))
        {
            IInteractable interactable = netObj.GetComponentInChildren<IInteractable>(true);
            interactable?.Interact();
        }
    }

    private void UpdateAim(Camera cam)
    {
        _current = null;
        _currentNetObj = null;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        QueryTriggerInteraction q = hitTriggers
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayers, q))
        {
            // GetComponentInParent handles the collider being on a child mesh
            // while the IInteractable / NetworkObject sits on the parent.
            _current = hit.collider.GetComponentInParent<IInteractable>();
            _currentNetObj = hit.collider.GetComponentInParent<NetworkObject>();
        }
    }

    // Use the assigned camera if it's still valid; otherwise fall back to the
    // current main camera and remember it. This self-heals across scene changes.
    private Camera ResolveCamera()
    {
        if (aimCamera != null) return aimCamera;

        aimCamera = Camera.main;   // requires the scene camera to be tagged MainCamera
        return aimCamera;
    }

    /// <summary>True when the owner is aiming at something interactable
    /// (useful for showing a prompt or highlight).</summary>
    public bool HasTarget => _current != null;

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Camera cam = aimCamera != null ? aimCamera : Camera.main;
        if (cam == null) return;

        Gizmos.color = _current != null ? Color.green : Color.cyan;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * interactRange);
    }
}