using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using System;

public class OfficeComputerUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject computerScreenPanel;   // the full-screen computer UI, hidden by default

    [Header("Camera")]
    public CinemachineCamera computerCamera;  // vcam framed on the monitor
    public int activePriority = 20;           // higher than the player cam so it takes over
    public int inactivePriority = 0;

    private Action onClosed;
    private Transform user;
    private bool isOpen = false;

    private void Start()
    {
        if (computerScreenPanel != null)
            computerScreenPanel.SetActive(false);

        if (computerCamera != null)
            computerCamera.Priority = inactivePriority;
    }

    private void Update()
    {
        if (!isOpen) return;

        // Esc also exits (Interact handled by ComputerInteraction, but allow Esc here)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseComputer();
    }

    public void OpenComputer(Transform player, Action closedCallback)
    {
        user = player;
        onClosed = closedCallback;
        isOpen = true;

        // blend camera to the monitor
        if (computerCamera != null)
            computerCamera.Priority = activePriority;

        // show UI + free the cursor
        if (computerScreenPanel != null)
            computerScreenPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // stop the player from moving/looking while in the computer
        SetPlayerControl(false);
    }

    public void CloseComputer()
    {
        isOpen = false;

        if (computerCamera != null)
            computerCamera.Priority = inactivePriority;

        if (computerScreenPanel != null)
            computerScreenPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetPlayerControl(true);

        onClosed?.Invoke();
        user = null;
    }

    // disable/enable the local player's movement + camera look while using the computer
    private void SetPlayerControl(bool enabled)
    {
        if (user == null) return;

        AbodiMovements move = user.GetComponent<AbodiMovements>();
        if (move != null) move.enabled = enabled;

        AbodiCamera look = user.GetComponent<AbodiCamera>();
        if (look != null) look.enabled = enabled;
    }
}