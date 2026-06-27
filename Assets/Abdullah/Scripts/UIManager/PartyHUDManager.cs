using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;

public class PartyHUDManager : MonoBehaviour
{
    [Header("Party HUD Settings")]
    public GameObject playerCardPrefab;  // prefab for each player's card
    public Transform cardContainer;      // vertical layout group

    private Dictionary<ulong, PlayerCardUI> playerCards
        = new Dictionary<ulong, PlayerCardUI>();

    private void Start()
    {
        // listen for players joining and leaving
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeft;
    }

    private void OnPlayerJoined(ulong clientId)
    {
        // wait a frame for player object to exist
        StartCoroutine(AddCardDelayed(clientId));
    }

    private System.Collections.IEnumerator AddCardDelayed(ulong clientId)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (playerCards.ContainsKey(clientId)) yield break;

        // find the player object
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            yield break;

        NetworkObject playerObj = NetworkManager.Singleton
            .ConnectedClients[clientId].PlayerObject;
        if (playerObj == null) yield break;

        PlayerStats stats = playerObj.GetComponent<PlayerStats>();
        KitManager kit = playerObj.GetComponent<KitManager>();
        NetworkPlayerData netData = playerObj.GetComponent<NetworkPlayerData>();
        PlayerDeath death = playerObj.GetComponent<PlayerDeath>();

        if (stats == null) yield break;

        // spawn card
        GameObject card = Instantiate(playerCardPrefab, cardContainer);
        PlayerCardUI cardUI = card.GetComponent<PlayerCardUI>();
        cardUI.Setup(clientId, stats, kit, netData, death);

        playerCards[clientId] = cardUI;
    }

    private void OnPlayerLeft(ulong clientId)
    {
        if (!playerCards.ContainsKey(clientId)) return;

        Destroy(playerCards[clientId].gameObject);
        playerCards.Remove(clientId);
    }
}