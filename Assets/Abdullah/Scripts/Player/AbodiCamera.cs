using UnityEngine;

public class AbodiCamera : MonoBehaviour
{
    [SerializeField] private GameObject myCamera;
    [SerializeField] private float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private Vector2 lookInput;

    private void OnEnable()
    {
        PlayerInputs.OnLookInput += HandleLookInput;
    }

    private void OnDisable()
    {
        PlayerInputs.OnLookInput -= HandleLookInput;
    }

    private void HandleLookInput(Vector2 input)
    {
        lookInput = input;
        Debug.Log("LOOK INPUT: " + input);
    }

    void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        myCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}