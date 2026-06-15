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
    private Vector3 movement;
    private Vector3 velocity;
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
        movement = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        cameraRotation = context.ReadValue<Vector2>() * mouseSensitivity * Time.deltaTime;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && myCharacter.isGrounded && !isJumping)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed && myCharacter.isGrounded && !isCrouching)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Movements logic:
        Vector3 move = movement.x * transform.right + movement.y * transform.forward;
        myCharacter.Move(move * mySpeed * Time.deltaTime);

        // Gravity logic:
        if (myCharacter.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        myCharacter.Move(velocity * Time.deltaTime);

        // Camera rotation logic:
        xRotation -= cameraRotation.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        myCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * cameraRotation.x);

        // Jump logic: 
        if (isJumping && myCharacter.isGrounded)
        {
           isJumping = false; 
        }

        // Crouch logic:
        if (isCrouching)
        {
            myCharacter.height = crouchHeight;
            mySpeed = crouchSpeed;
        }
        else if (!isCrouching)
        {
            myCharacter.height = normalHeight;
            mySpeed = moveSpeed;
        }

        // Sprint logic:







    }
}
