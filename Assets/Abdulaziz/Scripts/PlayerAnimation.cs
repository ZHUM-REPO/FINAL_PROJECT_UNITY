using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2.5f;

    private bool isCrouching;

    void Update()
    {
        HandleCrouch();
        UpdateAnimator();
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }

        animator.SetBool("IsCrouching", isCrouching);
    }

    void UpdateAnimator()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool moving = Mathf.Abs(horizontal) > 0.01f ||
                      Mathf.Abs(vertical) > 0.01f;

        float targetSpeed = 0f;

        if (moving)
        {
            if (isCrouching)
            {
                // Crouch Walk
                targetSpeed = 0.5f;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                // Run
                targetSpeed = 1f;
            }
            else
            {
                // Walk
                targetSpeed = 0.5f;
            }
        }

        // Smooth blend
        float currentSpeed = animator.GetFloat("Speed");

        animator.SetFloat(
            "Speed",
            Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f)
        );
    }
}