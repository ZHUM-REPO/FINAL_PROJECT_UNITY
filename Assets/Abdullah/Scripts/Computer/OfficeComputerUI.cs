using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System;

public class OfficeComputerUI : MonoBehaviour
{
    // true whenever the LOCAL player has a computer open
    public static bool IsAnyComputerOpen = false;

    [Header("Screen UI (World-Space)")]
    public GraphicRaycaster screenRaycaster;      // MEDIUM screen
    public GraphicRaycaster bigScreenRaycaster;   // BIG screen

    [Header("Camera")]
    public CinemachineCamera computerCamera;
    public int activePriority = 20;

    [Header("HUD to hide while using computer")]
    public GameObject partyHUD;   // drag your PartyHUD object here

    private Action onClosed;
    private Transform user;
    private bool isOpen = false;

    private void Start()
    {
        if (computerCamera != null)
        {
            computerCamera.Priority = activePriority;
            computerCamera.gameObject.SetActive(false);
        }

        if (screenRaycaster != null) screenRaycaster.enabled = false;
        if (bigScreenRaycaster != null) bigScreenRaycaster.enabled = false;
    }

    public void OpenComputer(Transform player, Action closedCallback)
    {
        user = player;
        onClosed = closedCallback;
        isOpen = true;
        IsAnyComputerOpen = true;

        if (computerCamera != null)
            computerCamera.gameObject.SetActive(true);

        if (screenRaycaster != null) screenRaycaster.enabled = true;
        if (bigScreenRaycaster != null) bigScreenRaycaster.enabled = true;

        // hide the party HUD so it doesn't block the small screen
        if (partyHUD != null) partyHUD.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetPlayerControl(false);
    }

    public void CloseComputer()
    {
        isOpen = false;
        IsAnyComputerOpen = false;

        if (computerCamera != null)
            computerCamera.gameObject.SetActive(false);

        if (screenRaycaster != null) screenRaycaster.enabled = false;
        if (bigScreenRaycaster != null) bigScreenRaycaster.enabled = false;

        // bring the party HUD back
        if (partyHUD != null) partyHUD.SetActive(true);

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