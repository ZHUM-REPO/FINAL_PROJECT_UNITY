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

    // public Vector2 inputMove;


    public void MoveInput(InputAction.CallbackContext context)
    {
        // inputMove = context.ReadValue<Vector2>();
        // OnMoveInput?.Invoke(inputMove);
        OnMoveInput?.Invoke(context.ReadValue<Vector2>());

    }

    public void LookInput(InputAction.CallbackContext context)
    {
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpInput?.Invoke();
        }
    }
    
    public void CrouchInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnCrouchInput?.Invoke();
        }
    }

    public void SprintInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnSprintInput?.Invoke();
        }
    }

    public void PauseInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPauseInput?.Invoke();
        }
    }
}
