using UnityEngine;

public class AbodiMovements : MonoBehaviour
{
    [SerializeField] private CharacterController myCharacter;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    // [SerializeField] private PlayerInputs playerInputs;
    private Vector3 move;
    private Vector3 velocity;


    private void OnEnable()
    {
        PlayerInputs.OnMoveInput += HandleMoveInput;
    }

    private void OnDisable()
    {
        PlayerInputs.OnMoveInput -= HandleMoveInput;
    }
    void Start()
    {
        myCharacter = GetComponent<CharacterController>();
    }


    private void HandleMoveInput(Vector2 moveInput)
    {
        move = moveInput.x * transform.right + moveInput.y * transform.forward;
    }

    private void GravityLogic()
    {
        velocity.y += gravity * Time.deltaTime;
        if (myCharacter.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        myCharacter.Move(velocity * Time.deltaTime);
    }
    

    private void Update()
    {   
        // HandleMoveInput(playerInputs.inputMove);
        myCharacter.Move(move * moveSpeed * Time.deltaTime);
        GravityLogic();
    }
}
