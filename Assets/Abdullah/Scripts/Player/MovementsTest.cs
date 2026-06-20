using UnityEngine;
using UnityEngine.InputSystem;

public class MovementsTest : MonoBehaviour
{
    [SerializeField] private CharacterController myCharacter;
    [SerializeField] private GameObject myCamera;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 3f; 
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float xRotation = 0f;
    [SerializeField] private float mySpeed;
    private Vector3 moveDirection;
    private Vector3 finalDirection;
    private Vector2 moveInput;
    private float verticalVelocity = -2;
    private Vector2 cameraRotation;
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool isSprinting = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myCharacter = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mySpeed = moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        cameraRotation = context.ReadValue<Vector2>() * mouseSensitivity;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(!myCharacter.isGrounded) return;

        if (context.performed && myCharacter.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed && myCharacter.isGrounded && !isCrouching)
        {
            isCrouching = true;
            myCharacter.height = crouchHeight;
            mySpeed = crouchSpeed;
        }
        else if(context.performed && myCharacter.isGrounded && isCrouching)
        {
            isCrouching = false;
            myCharacter.height = normalHeight;
            mySpeed = moveSpeed;
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed && myCharacter.isGrounded && !isSprinting && !isCrouching)
        {
            isSprinting = true;
        }
        else if (context.performed && myCharacter.isGrounded && isSprinting)
        {
            isSprinting = false;
        }
    }

    void Update()
    {        
        // Gravity logic:
        if (myCharacter.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        else if(!myCharacter.isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Sprint logic:
        if (isSprinting)
        {
            mySpeed = moveSpeed * sprintMultiplier;
        }
        else if (!isSprinting && !isCrouching)
        {
            mySpeed = moveSpeed;
        }

        // Movements logic:
        moveDirection = moveInput.x * transform.right + moveInput.y * transform.forward;
        finalDirection = moveDirection * mySpeed;
        finalDirection.y = verticalVelocity;

        myCharacter.Move(finalDirection * Time.deltaTime);
    }

    void LateUpdate()
    {
        // Camera rotation logic:
        xRotation -= cameraRotation.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        myCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * cameraRotation.x);
    }
}
