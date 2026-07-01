using UnityEngine;
using Unity.Netcode;

public class ComputerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 3f;
    public GameObject interactPrompt;   // "Press E" world/screen prompt, hidden by default

    [Header("Computer View")]
    public OfficeComputerUI computerUI; // the UI + camera controller for this computer

    private Transform localPlayer;
    private bool playerInRange = false;
    private bool isInUse = false;

    private void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

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
        // find the local player once it exists
        if (localPlayer == null)
        {
            localPlayer = FindLocalPlayer();
            return;
        }

        // if currently using the computer, no range checks needed
        if (isInUse) return;

        float distance = Vector3.Distance(localPlayer.position, transform.position);
        bool nowInRange = distance <= interactRange;

        // only update the prompt when the state changes
        if (nowInRange != playerInRange)
        {
            playerInRange = nowInRange;
            if (interactPrompt != null)
                interactPrompt.SetActive(playerInRange);
        }
    }

    private Transform FindLocalPlayer()
    {
        PlayerStats[] all = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        foreach (PlayerStats ps in all)
        {
            NetworkObject netObj = ps.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
                return ps.transform;
        }
        return null;
    }

    private void HandleInteract()
    {
        // open only if near and not already open
        if (!playerInRange || isInUse) return;
        if (computerUI == null) return;

        isInUse = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        computerUI.OpenComputer(localPlayer, OnComputerClosed);
    }

    // called back by the UI when the player exits
    private void OnComputerClosed()
    {
        isInUse = false;
    }
}