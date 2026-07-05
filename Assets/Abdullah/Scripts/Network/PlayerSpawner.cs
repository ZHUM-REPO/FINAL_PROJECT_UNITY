using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    private int spawnIndex = 0;

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server already running — spawning all connected players.");
            StartCoroutine(SpawnAllConnectedPlayers());
        }
        else
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started — spawning host player.");
        SpawnPlayer(NetworkManager.Singleton.LocalClientId);
    }

    private IEnumerator SpawnAllConnectedPlayers()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null) continue;
            SpawnPlayer(client.ClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        StartCoroutine(SpawnPlayerDelayed(clientId));
    }

    private IEnumerator SpawnPlayerDelayed(ulong clientId)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} not found.");
            return;
        }

        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.Log($"Client {clientId} already has a player.");
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[spawnIndex % spawnPoints.Length];
            spawnPos = point.position;
            spawnRot = point.rotation;
            spawnIndex++;
        }

        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);

        NetworkObject netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);

        // place the host's own copy (works because host = owner of its player)
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.SetPositionAndRotation(spawnPos, spawnRot);
        if (cc != null) cc.enabled = true;

        // ask the OWNING client to place ITS player at the spawn point.
        // with client-authoritative movement only the owner can move
        // its own player, so the server must request it via RPC.
        NetworkMovement netMovement = player.GetComponent<NetworkMovement>();
        if (netMovement != null)
            netMovement.TeleportToSpawnRpc(spawnPos, spawnRot);

        Debug.Log($"Spawned player for ClientId: {clientId} at {spawnPos}");
    }
}