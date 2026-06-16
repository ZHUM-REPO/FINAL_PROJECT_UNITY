using UnityEngine;

public class AbodiCamera : MonoBehaviour
{
    [SerializeField] private GameObject myCamera;
    [SerializeField] private float mouseSensitivity = 100f;
    private float xRotation = 0f;
    private float mouseX;
    private float mouseY;

    private void OnEnable()
    {
        PlayerInputs.OnLookInput += HandleLookInput;
    }

    private void OnDisable()
    {
        PlayerInputs.OnLookInput -= HandleLookInput;
    }

    public void HandleLookInput(Vector2 lookInput)
    {
        mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        myCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
