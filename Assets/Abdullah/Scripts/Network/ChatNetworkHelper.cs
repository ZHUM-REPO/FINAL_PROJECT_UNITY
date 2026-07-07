using Unity.Netcode;
using UnityEngine;

public class ChatNetworkHelper : NetworkBehaviour
{
    public static ChatNetworkHelper Instance;

    public override void OnNetworkSpawn()
    {
        // only set instance for local player
        if (IsOwner)
            Instance = this;
    }

    public void SendChatMessage(string message)
    {
        SendChatServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendChatServerRpc(string message)
    {
        ReceiveChatClientRpc(message);
    }

    [ClientRpc]
    public void ReceiveChatClientRpc(string message)
    {
        TextChatManager.Instance?.DisplayMessage(message);
    }
}