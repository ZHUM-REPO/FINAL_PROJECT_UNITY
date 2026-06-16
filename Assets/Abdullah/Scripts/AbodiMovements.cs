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
    // [SerializeField] private PlayerInputs playerInputs;
    private float mySpeed;
    private float myHeight;
    private Vector2 moveInput;
    private Vector3 move;
    private Vector3 velocity;
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
        myHeight = normalHeight;
    }
    private void HandleMoveInput(Vector2 mInput)
    {
        moveInput = mInput;
    }

    private void HandleJumpInput()
    {
        if (myCharacter.isGrounded && velocity.y < 0 && !isJumping)
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
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
        
    }

    private void GravityLogic()
    {
        velocity.y += gravity * Time.deltaTime;
        if (myCharacter.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
        }
        myCharacter.Move(velocity * Time.deltaTime);
    }
    
    private void Update()
    {   
        // HandleMoveInput(playerInputs.inputMove);
        move = moveInput.x * transform.right + moveInput.y * transform.forward;
        myCharacter.Move(move * mySpeed * Time.deltaTime);

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

    }
}
