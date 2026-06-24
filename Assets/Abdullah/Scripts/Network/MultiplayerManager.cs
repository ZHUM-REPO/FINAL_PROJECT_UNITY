using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Unity.Services.Vivox;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using Unity.Networking.Transport.Error;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;

public class MultiplayerManager : MonoBehaviour
{
    public TMP_InputField codeInput;
    public TMP_Text statusText; 
    private ISession session;
    private String voiceChannelName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await VivoxService.Instance.InitializeAsync();
            statusText.text = "Ready...";
        }

    }

    public async void Host()
    {
        statusText.text = "Creating...";
        session = await MultiplayerService.Instance.CreateSessionAsync(
        new SessionOptions
        {
          MaxPlayers  = 4, IsPrivate = false
        }
        );
        NetworkManager.Singleton.StartHost();
        voiceChannelName = "Voice" + session.Code;
        await JoinVoiceChannel();

        statusText.text = "Code:" + session.Code;
    }

    public async void Join()
    {
        statusText.text = "Joining...";
        
        session= session = await MultiplayerService.Instance.JoinSessionByCodeAsync(
            codeInput.text.Trim()
        );
        NetworkManager.Singleton.StartClient();

        voiceChannelName = "voice_" + codeInput.text.Trim();
        await JoinVoiceChannel();

        statusText.text = "Joined + Voice On";
                    Debug.Log("Done");
    }

    private async System.Threading.Tasks.Task JoinVoiceChannel()
    {
        await VivoxService.Instance.JoinGroupChannelAsync(
            voiceChannelName,
            ChatCapability.AudioOnly
        );

        Debug.Log("Joined voice channel; " + voiceChannelName);
    }

    public async void LeaveVoice()
    {
        if (!string.IsNullOrEmpty(voiceChannelName))
        {
            await VivoxService.Instance.LeaveChannelAsync(voiceChannelName);
            Debug.Log("Left voice channel");
        }
    }

    public void NextScene()
    {
        if (!NetworkManager.Singleton.IsServer)
        return;

        NetworkManager.Singleton.SceneManager.LoadScene("AboodScene", LoadSceneMode.Single);
    }
}
