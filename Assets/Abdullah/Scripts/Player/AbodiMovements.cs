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
    // private float myHeight;
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
    void Start()
    {
        myCharacter = GetComponent<CharacterController>();
        mySpeed = moveSpeed;
        // myHeight = normalHeight;
    }
    private void HandleMoveInput(Vector2 mInput)
    {
        moveInput = mInput;
    }

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
        if (myCharacter.isGrounded && !isCrouching)
        {
            isCrouching = true;
        }
        else if (myCharacter.isGrounded && isCrouching)
        {
            isCrouching = false;
        }
    }

    private void HandleSprintInput()
    {
        if (myCharacter.isGrounded && !isSprinting && !isCrouching && playerStats.canSprint)
        {
            isSprinting = true;
        }
        else if (myCharacter.isGrounded && isSprinting)
        {
            isSprinting = false;
        }
    }

    private void GravityLogic()
    {
        verticalVelocity += gravity * Time.deltaTime;
        if (myCharacter.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }
    }
    
    private void Update()
    {   
        GravityLogic();

        if (isCrouching)
        {
            // myHeight = crouchHeight;
            myCharacter.height = crouchHeight;
            mySpeed = crouchSpeed;
        }
        else if (!isCrouching)
        {
            // myHeight = normalHeight;
            myCharacter.height = normalHeight;
            mySpeed = moveSpeed;
        }

        // myCharacter.height = myHeight;

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
        else if (!isSprinting && !isCrouching)
        {
            playerStats.EnduranceRegain();
            mySpeed = moveSpeed;
        }

        // HandleMoveInput(playerInputs.inputMove);
        moveDirection = moveInput.x * transform.right + moveInput.y * transform.forward;
        finalDirection = moveDirection * mySpeed;
        finalDirection.y = verticalVelocity;
        myCharacter.Move(finalDirection * Time.deltaTime);
    }
}
