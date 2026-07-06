using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimation : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;

    [Header("Blend Targets")]
    [SerializeField] private float walkBlend = 0.5f;
    [SerializeField] private float runBlend = 1f;
    [SerializeField] private float crouchBlend = 0.5f;

    [Header("Blend Smoothing")]
    [SerializeField] private float speedDampTime = 0.12f;

    [Header("Input")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    private int speedHash;
    private int crouchHash;
    private int groundedHash;
    private int verticalHash;
    private int jumpHash;

    private bool isCrouching;
    private bool hasValidAnimator;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (controller == null) controller = GetComponent<CharacterController>();

        speedHash = Animator.StringToHash("Speed");
        crouchHash = Animator.StringToHash("IsCrouching");
        groundedHash = Animator.StringToHash("IsGrounded");
        verticalHash = Animator.StringToHash("VerticalSpeed");
        jumpHash = Animator.StringToHash("Jump");

        // check this animator actually has the parameters we need
        hasValidAnimator = animator != null && HasParameter("Speed");

        if (!hasValidAnimator)
            Debug.LogWarning($"PlayerAnimation on '{gameObject.name}' has an animator " +
                             $"without the right parameters — disabling it here.");
    }

    private bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!hasValidAnimator) return;   // skip if wrong animator — no more warnings

        HandleCrouch();
        UpdateState();
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(crouchKey))
            isCrouching = !isCrouching;

        animator.SetBool(crouchHash, isCrouching);
    }

    private void UpdateState()
    {
        bool grounded = controller.isGrounded;
        animator.SetBool(groundedHash, grounded);

        animator.SetFloat(verticalHash, controller.velocity.y);

        if (grounded && !isCrouching && Input.GetKeyDown(jumpKey))
            animator.SetTrigger(jumpHash);

        Vector3 horizontalVel = controller.velocity;
        horizontalVel.y = 0f;
        bool moving = horizontalVel.magnitude > 0.1f;

        float targetSpeed = 0f;
        if (moving)
        {
            if (isCrouching)
                targetSpeed = crouchBlend;
            else if (Input.GetKey(runKey))
                targetSpeed = runBlend;
            else
                targetSpeed = walkBlend;
        }

        animator.SetFloat(speedHash, targetSpeed, speedDampTime, Time.deltaTime);
    }
}