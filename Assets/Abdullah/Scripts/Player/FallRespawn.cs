using Unity.Netcode;
using UnityEngine;

public class FallRespawn : NetworkBehaviour
{
    [Header("Fall Detection")]
    [Tooltip("Players below this Y height are considered fallen off the map")]
    [SerializeField] private float killHeight = -20f;

    [Header("Respawn")]
    [SerializeField] private Transform[] respawnPoints;
    [SerializeField] private float fallDamage = 20f;

    [Header("Check rate")]
    [SerializeField] private float checkInterval = 0.5f;

    private float checkTimer = 0f;

    private void Update()
    {
        // only the server checks and handles respawning
        if (!IsServer) return;

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = checkInterval;

        CheckFallenPlayers();
    }

    private void CheckFallenPlayers()
    {
        foreach (var pd in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            // skip dead players (they're handled by the death system)
            PlayerDeath death = pd.GetComponent<PlayerDeath>();
            if (death != null && death.IsDead()) continue;

            if (pd.transform.position.y < killHeight)
                RespawnPlayer(pd);
        }
    }

    private void RespawnPlayer(PlayerStats player)
    {
        // pick a respawn point
        Vector3 respawnPos = transform.position;
        if (respawnPoints != null && respawnPoints.Length > 0)
        {
            Transform point = respawnPoints[Random.Range(0, respawnPoints.Length)];
            if (point != null) respawnPos = point.position;
        }

        // deal fall damage (server-authoritative, syncs via PlayerStats)
        player.TakeDamage(fallDamage);

        // teleport the player back — tell the OWNER to move (client-authoritative movement)
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null)
            TeleportClientRpc(respawnPos, netObj.OwnerClientId);

        Debug.Log($"{player.gameObject.name} fell off the map — respawned with {fallDamage} damage.");
    }

    [ClientRpc]
    private void TeleportClientRpc(Vector3 position, ulong targetClient)
    {
        // only the owning client moves its own player
        if (NetworkManager.Singleton.LocalClientId != targetClient) return;

        foreach (var ps in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            NetworkObject no = ps.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner)
            {
                CharacterController cc = ps.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                ps.transform.position = position;
                if (cc != null) cc.enabled = true;

                // reset any downward velocity so they don't keep falling
                AbodiMovements move = ps.GetComponent<AbodiMovements>();
                if (move != null) move.ResetVerticalVelocity();
                break;
            }
        }
    }
}