using UnityEngine;

public class AbodiCamera : MonoBehaviour
{
    [SerializeField] private GameObject myCamera;
    [SerializeField] private float mouseSensitivity = 100f;
    private float xRotation = 0f;
    private Vector2 _lookInput;

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
        _lookInput = lookInput;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        xRotation -= _lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        myCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * _lookInput.x * mouseSensitivity);
    }
}
