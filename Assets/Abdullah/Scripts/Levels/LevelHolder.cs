using UnityEngine;
using Unity.Netcode;

public class LevelHolder : MonoBehaviour
{
    [Header("This Level")]
    public LevelDefinition level;
    public int levelIndex = 0;              // matches LevelSelectionManager order
    public float interactRange = 3f;
    public GameObject interactPrompt;       // "Press E to view", hidden by default

    [Header("Popup")]
    public LevelInfoPopup popup;            // the single popup in the scene

    private Transform localPlayer;
    private bool inRange = false;

    private void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void OnEnable()  { PlayerInputs.OnInteractInput += HandleInteract; }
    private void OnDisable() { PlayerInputs.OnInteractInput -= HandleInteract; }

    private void Update()
    {
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
}