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

    private void Awake()
    {
        movements = GetComponent<AbodiMovements>();
        cameraScript = GetComponent<AbodiCamera>();
        playerInputs = GetComponent<PlayerInputs>();
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();
        audioListener = GetComponentInChildren<AudioListener>();
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
        if (movements != null) movements.enabled = true;
        if (cameraScript != null) cameraScript.enabled = true;
        if (playerInputs != null) playerInputs.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
        if (audioListener != null) audioListener.enabled = true;

        // enable cinemachine camera for local player only
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

        // disable cinemachine camera for remote players
        // so it doesn't take over the local player's view
        if (cinemachineCamera != null) cinemachineCamera.enabled = false;

        Debug.Log($"Remote player {OwnerClientId} disabled.");
    }
}