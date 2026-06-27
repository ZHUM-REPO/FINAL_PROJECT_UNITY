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
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
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

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // skip host — already spawned in OnServerStarted
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        Debug.Log($"Client {clientId} connected — waiting then spawning.");

        // wait one frame to make sure client is fully registered
        StartCoroutine(SpawnPlayerDelayed(clientId));
    }

    private IEnumerator SpawnPlayerDelayed(ulong clientId)
    {
        // wait for end of frame so NetworkManager fully registers the client
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // safety check
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} not found in ConnectedClients.");
            return;
        }

        // don't spawn if already has a player object
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.Log($"Client {clientId} already has a player object.");
            return;
        }

        // pick spawn point
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

        Debug.Log($"Spawned player for ClientId: {clientId} at {spawnPos}");
    }
}