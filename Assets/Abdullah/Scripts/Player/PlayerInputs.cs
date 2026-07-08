using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerInputs : NetworkBehaviour
{
    public static event Action<Vector2> OnMoveInput;
    public static event Action<Vector2> OnLookInput;
    public static event Action OnJumpInput;
    public static event Action OnCrouchInput;
    public static event Action OnSprintInput;
    public static event Action OnPauseInput;
    public static event Action OnCastSpellInput;
    public static event Action OnCastSpellCanceled;
    public static event Action OnChangeSpellInput;
    public static event Action OnChangeKitInput;
    public static event Action OnThrowInput;
    public static event Action OnDropInput;
    public static event Action OnInteractInput;

    private bool IsLocalPlayer => IsOwner;

    // true when any blocking UI is open (popup, computer, or pause)
    private bool UIOpen =>
        PauseMenuUI.isPaused ||
        OfficeComputerUI.IsAnyComputerOpen ||
        LevelInfoPopup.AnyPopupOpen;

    public void MoveInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        OnMoveInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void LookInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnJumpInput?.Invoke();
    }

    public void CrouchInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnCrouchInput?.Invoke();
    }

    public void SprintInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnSprintInput?.Invoke();
    }

    public void PauseInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;

        // don't open the pause menu while the computer or a popup is open
        if (OfficeComputerUI.IsAnyComputerOpen) return;
        if (LevelInfoPopup.AnyPopupOpen) return;

        if (context.performed)
        {
            Debug.Log("ESC Pressed From Input System");
            OnPauseInput?.Invoke();
        }
    }

    public void CastSpellInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;

        // always allow release so held spells (Fire Breath) can stop,
        // even if a UI opened mid-cast
        if (context.canceled)
        {
            OnCastSpellCanceled?.Invoke();
            return;
        }

        if (UIOpen) return;

        if (context.performed)
            OnCastSpellInput?.Invoke();
    }

    public void ChangeSpellInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnChangeSpellInput?.Invoke();
    }

    public void ChangeKitInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnChangeKitInput?.Invoke();
    }

    public void ThrowInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnThrowInput?.Invoke();
    }

    public void DropInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;
        if (UIOpen) return;
        if (context.performed)
            OnDropInput?.Invoke();
    }

    public void InteractInput(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer) return;

        // Interact must still work while the computer is open (E closes it).
        // Only block it when paused.
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnInteractInput?.Invoke();
    }
}