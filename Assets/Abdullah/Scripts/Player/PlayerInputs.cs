using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
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

    public void MoveInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused)
        {
            OnMoveInput?.Invoke(Vector2.zero);
            return;
        }

        OnMoveInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void LookInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused)
        {
            OnLookInput?.Invoke(Vector2.zero);
            return;
        }

        OnLookInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnJumpInput?.Invoke();
    }

    public void CrouchInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnCrouchInput?.Invoke();
    }

    public void SprintInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnSprintInput?.Invoke();
    }

    public void PauseInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("ESC Pressed");
            OnPauseInput?.Invoke();
        }
    }

    public void CastSpellInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnCastSpellInput?.Invoke();

        if (context.canceled)
            OnCastSpellCanceled?.Invoke();
    }

    public void ChangeSpellInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnChangeSpellInput?.Invoke();
    }

    public void ChangeKitInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnChangeKitInput?.Invoke();
    }

    public void ThrowInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnThrowInput?.Invoke();
    }

    public void DropInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnDropInput?.Invoke();
    }

    public void InteractInput(InputAction.CallbackContext context)
    {
        if (PauseMenuUI.isPaused) return;

        if (context.performed)
            OnInteractInput?.Invoke();
    }
}