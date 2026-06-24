using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints; // drag spawn points here in Inspector

    private int spawnIndex = 0;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;

        // spawn already connected clients
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            SpawnPlayer(clientId);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // don't spawn if already has a player object
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            return;

        // pick spawn point — cycle through available ones
        Vector3 spawnPos = spawnPoints != null && spawnPoints.Length > 0
            ? spawnPoints[spawnIndex % spawnPoints.Length].position
            : Vector3.zero;

        spawnIndex++;

        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        Debug.Log($"Spawned player for ClientId: {clientId} at {spawnPos}");
    }
}