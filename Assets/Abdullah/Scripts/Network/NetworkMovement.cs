using Unity.Netcode;
using UnityEngine;

public class NetworkMovement : NetworkBehaviour
{
    private AbodiMovements movements;
    private AbodiCamera cameraScript;
    private PlayerInputs playerInputs;

    private void Awake()
    {
        movements = GetComponent<AbodiMovements>();
        cameraScript = GetComponent<AbodiCamera>();
        playerInputs = GetComponent<PlayerInputs>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) return;

        // disable movement and camera for non-owners
        // so other players don't control your character
        if (movements != null) movements.enabled = false;
        if (cameraScript != null) cameraScript.enabled = false;
        if (playerInputs != null) playerInputs.enabled = false;
    }
}