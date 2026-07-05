using UnityEngine;

public class AbodiMovements : MonoBehaviour
{
    [SerializeField] private CharacterController myCharacter;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private PlayerStats playerStats;

    private float mySpeed;
    private float verticalVelocity = 0f;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Vector3 finalDirection;
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool isSprinting = false;

    private void OnEnable()
    {
        PlayerInputs.OnMoveInput += HandleMoveInput;
        PlayerInputs.OnJumpInput += HandleJumpInput;
        PlayerInputs.OnCrouchInput += HandleCrouchInput;
        PlayerInputs.OnSprintInput += HandleSprintInput;
    }

    private void OnDisable()
    {
        PlayerInputs.OnMoveInput -= HandleMoveInput;
        PlayerInputs.OnJumpInput -= HandleJumpInput;
        PlayerInputs.OnCrouchInput -= HandleCrouchInput;
        PlayerInputs.OnSprintInput -= HandleSprintInput;
    }

    private void Start()
    {
        if (myCharacter == null)
            myCharacter = GetComponent<CharacterController>();

        mySpeed = moveSpeed;
    }

    private void HandleMoveInput(Vector2 mInput) => moveInput = mInput;

    private void HandleJumpInput()
    {
        if (myCharacter.isGrounded && verticalVelocity < 0 && !isJumping)
        {
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
            isJumping = true;
        }
    }

    private void HandleCrouchInput()
    {
        if (!myCharacter.isGrounded) return;

        isCrouching = !isCrouching;

        if (isCrouching) isSprinting = false;
    }

    private void HandleSprintInput()
    {
        if (!myCharacter.isGrounded || isCrouching) return;

        if (!isSprinting && playerStats.canSprint)
            isSprinting = true;
        else
            isSprinting = false;
    }

    private void GravityLogic()
    {
        if (myCharacter.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }
        verticalVelocity += gravity * Time.deltaTime;
    }

    private void SpeedLogic()
    {
        if (isCrouching)
        {
            mySpeed = crouchSpeed;
            return;
        }

        if (isSprinting)
        {
            if (!playerStats.canSprint)
            {
                isSprinting = false;
                mySpeed = moveSpeed;
                return;
            }

            playerStats.EnduranceDrain();
            mySpeed = moveSpeed * sprintMultiplier;
        }
        else
        {
            playerStats.EnduranceRegain();
            mySpeed = moveSpeed;
        }
    }

    private void Update()
    {
        GravityLogic();
        SpeedLogic();

        myCharacter.height = isCrouching ? crouchHeight : normalHeight;

        moveDirection = moveInput.x * transform.right + moveInput.y * transform.forward;
        finalDirection = moveDirection * mySpeed;
        finalDirection.y = verticalVelocity;
        myCharacter.Move(finalDirection * Time.deltaTime);
    }

    public void ResetVerticalVelocity()
    {
        verticalVelocity = 0f;
        isJumping = false;
    }
}