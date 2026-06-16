using UnityEngine;

public class AbodiMovements : MonoBehaviour
{
    [SerializeField] private CharacterController myCharacter;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 3f;
    // [SerializeField] private PlayerInputs playerInputs;
    private Vector2 moveInput;
    private Vector3 move;
    private Vector3 velocity;
    private bool isJumping = false;


    private void OnEnable()
    {
        PlayerInputs.OnMoveInput += HandleMoveInput;
        PlayerInputs.OnJumpInput += HandleJumpInput;
    }

    private void OnDisable()
    {
        PlayerInputs.OnMoveInput -= HandleMoveInput;
        PlayerInputs.OnJumpInput -= HandleJumpInput;
    }
    void Start()
    {
        myCharacter = GetComponent<CharacterController>();
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
        myCharacter.Move(move * moveSpeed * Time.deltaTime);
        GravityLogic();
    }
}
