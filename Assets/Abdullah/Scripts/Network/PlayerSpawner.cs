using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject prefab;
    

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            return;

       

        GameObject player = Instantiate(
            prefab,
            prefab.transform.position,
            prefab.transform.rotation
        );

        player.GetComponent<NetworkObject>()
              .SpawnAsPlayerObject(clientId, true);

        Debug.Log($"Spawned {prefab.name} for ClientId {clientId}");
    }
}