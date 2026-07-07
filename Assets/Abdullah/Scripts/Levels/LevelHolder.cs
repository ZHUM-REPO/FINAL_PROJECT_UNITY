using UnityEngine;
using Unity.Netcode;

public class LevelHolder : MonoBehaviour
{
    [Header("This Level")]
    public LevelDefinition level;
    public int levelIndex = 0;              // matches LevelSelectionManager order
    public float interactRange = 3f;
    private float reopenBlockTimer = 0f;
    public GameObject interactPrompt;       // "Press E to view", hidden by default

    [Header("Popup")]
    public LevelInfoPopup popup;            // the single popup in the scene

    private Transform localPlayer;
    private bool inRange = false;

    private void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void OnEnable() { PlayerInputs.OnInteractInput += HandleInteract; }
    private void OnDisable() { PlayerInputs.OnInteractInput -= HandleInteract; }

    private void Update()
    {
        if (reopenBlockTimer > 0f)
            reopenBlockTimer -= Time.deltaTime;

        if (localPlayer == null) { localPlayer = FindLocalPlayer(); return; }

        bool popupOpen = popup != null && popup.IsOpen;
        float dist = Vector3.Distance(localPlayer.position, transform.position);
        bool nowInRange = !popupOpen && dist <= interactRange;

        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (interactPrompt != null) interactPrompt.SetActive(inRange);
        }
    }

    private void HandleInteract()
    {
        if (!inRange || popup == null) return;
        if (popup.IsOpen) return;
        if (Time.time - LevelInfoPopup.lastClosedTime < 0.2f) return; // just closed, ignore

        popup.Open(level, levelIndex);
    }

    private Transform FindLocalPlayer()
    {
        foreach (var ps in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            var no = ps.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner) return ps.transform;
        }
        return null;
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Interact range: green while the local player is in range, yellow otherwise.
        Gizmos.color = inRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        // Small marker at the level holder itself.
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.15f);

        // Cyan line to the prompt this holder shows.
        if (interactPrompt != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, interactPrompt.transform.position);
            Gizmos.DrawWireCube(interactPrompt.transform.position, Vector3.one * 0.2f);
        }

        // Magenta line to the popup this holder opens.
        if (popup != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, popup.transform.position);
        }

        // At runtime, draw a line to the local player once found.
        if (localPlayer != null)
        {
            Gizmos.color = inRange ? Color.green : new Color(0.5f, 0.5f, 0.5f, 0.8f);
            Gizmos.DrawLine(transform.position, localPlayer.position);
        }
    }
}