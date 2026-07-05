using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class NetworkMovement : NetworkBehaviour
{
    private AbodiMovements movements;
    private AbodiCamera cameraScript;
    private PlayerInputs playerInputs;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private CinemachineCamera cinemachineCamera;
    private AudioListener audioListener;
    private CharacterController characterController;

    private void Awake()
    {
        movements = GetComponent<AbodiMovements>();
        cameraScript = GetComponent<AbodiCamera>();
        playerInputs = GetComponent<PlayerInputs>();
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();
        audioListener = GetComponentInChildren<AudioListener>();
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            EnableLocalPlayer();
        else
            DisableRemotePlayer();
    }

    private void EnableLocalPlayer()
    {
        if (movements != null)
        {
            movements.enabled = true;
            movements.ResetVerticalVelocity();
        }
        if (cameraScript != null)
        {
            cameraScript.enabled = true;
            cameraScript.ResetRotation();
        }
        if (playerInputs != null) playerInputs.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
        if (audioListener != null) audioListener.enabled = true;
        if (cinemachineCamera != null) cinemachineCamera.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Local player enabled.");
    }

    private void DisableRemotePlayer()
    {
        if (movements != null) movements.enabled = false;
        if (cameraScript != null) cameraScript.enabled = false;
        if (playerInputs != null) playerInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;
        if (audioListener != null) audioListener.enabled = false;
        if (cinemachineCamera != null) cinemachineCamera.enabled = false;

        Debug.Log($"Remote player {OwnerClientId} disabled.");
    }

    // Runs on the OWNER. The server calls this so the owning client
    // moves its own player to the spawn point.
    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc(Vector3 position, Quaternion rotation)
    {
        if (characterController != null) characterController.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        if (characterController != null) characterController.enabled = true;

        if (movements != null) movements.ResetVerticalVelocity();

        Debug.Log($"Owner teleported to spawn: {position}");
    }
}