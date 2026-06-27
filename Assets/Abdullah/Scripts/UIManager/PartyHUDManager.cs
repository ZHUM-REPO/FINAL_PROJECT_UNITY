using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class PartyHUDManager : MonoBehaviour
{
    [Header("Party HUD Settings")]
    public GameObject playerCardPrefab;
    public Transform cardContainer;

    private Dictionary<ulong, PlayerCardUI> playerCards
        = new Dictionary<ulong, PlayerCardUI>();

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;

        // keep checking for players every second
        StartCoroutine(RefreshPlayerCards());
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeft;
    }

    // keeps checking and adding missing cards every second
    private IEnumerator RefreshPlayerCards()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (!NetworkManager.Singleton.IsListening) continue;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (playerCards.ContainsKey(client.ClientId)) continue;
                if (client.PlayerObject == null) continue;

                AddCard(client.ClientId, client.PlayerObject);
            }
        }
    }

    private void OnPlayerJoined(ulong clientId)
    {
        StartCoroutine(AddCardDelayed(clientId));
    }

    private IEnumerator AddCardDelayed(ulong clientId)
    {
        // wait for player object to be fully spawned
        yield return new WaitForSeconds(0.5f);

        if (playerCards.ContainsKey(clientId)) yield break;
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            yield break;

        NetworkObject playerObj = NetworkManager.Singleton
            .ConnectedClients[clientId].PlayerObject;
        if (playerObj == null) yield break;

        AddCard(clientId, playerObj);
    }

    private void AddCard(ulong clientId, NetworkObject playerObj)
    {
        if (playerCards.ContainsKey(clientId)) return;

        PlayerStats stats = playerObj.GetComponent<PlayerStats>();
        KitManager kit = playerObj.GetComponent<KitManager>();
        NetworkPlayerData netData = playerObj.GetComponent<NetworkPlayerData>();
        PlayerDeath death = playerObj.GetComponent<PlayerDeath>();

        if (stats == null) return;

        GameObject card = Instantiate(playerCardPrefab, cardContainer);
        PlayerCardUI cardUI = card.GetComponent<PlayerCardUI>();
        cardUI.Setup(clientId, stats, kit, netData, death);

        playerCards[clientId] = cardUI;
        Debug.Log($"Added card for player {clientId}");
    }

    private void OnPlayerLeft(ulong clientId)
    {
        if (!playerCards.ContainsKey(clientId)) return;
        Destroy(playerCards[clientId].gameObject);
        playerCards.Remove(clientId);
    }
}