using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using System;

public class OfficeComputerUI : MonoBehaviour
{
    [Header("Screen UI (World-Space)")]
    public Canvas screenCanvas;          // world-space canvas on the monitor
    public GameObject screenContent;     // the kit/upgrade UI root (can stay always visible)
    public GraphicRaycaster screenRaycaster; // on the world-space canvas

    [Header("Camera")]
    public CinemachineCamera computerCamera; // vcam framed on the monitor screen
    public int activePriority = 20;
    public int inactivePriority = 0;

    private Action onClosed;
    private Transform user;
    private bool isOpen = false;

    private void Start()
    {
        if (computerCamera != null)
            computerCamera.Priority = inactivePriority;

        // the screen is always physically on, but not clickable until in use
        if (screenRaycaster != null)
            screenRaycaster.enabled = false;
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseComputer();
    }

    public void OpenComputer(Transform player, Action closedCallback)
    {
        user = player;
        onClosed = closedCallback;
        isOpen = true;

        // blend camera in to frame the monitor
        if (computerCamera != null)
            computerCamera.Priority = activePriority;

        // make the world-space screen clickable + free the cursor
        if (screenRaycaster != null)
            screenRaycaster.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetPlayerControl(false);
    }

    public void CloseComputer()
    {
        isOpen = false;

        if (computerCamera != null)
            computerCamera.Priority = inactivePriority;

        if (screenRaycaster != null)
            screenRaycaster.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetPlayerControl(true);

        onClosed?.Invoke();
        user = null;
    }

    private void SetPlayerControl(bool enabled)
    {
        if (user == null) return;

        AbodiMovements move = user.GetComponent<AbodiMovements>();
        if (move != null) move.enabled = enabled;

        AbodiCamera look = user.GetComponent<AbodiCamera>();
        if (look != null) look.enabled = enabled;
    }
}