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

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    [Header("UI")]
    public TMP_InputField codeInput;
    public TMP_Text statusText;

    private ISession session;
    private string voiceChannelName;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            await VivoxService.Instance.InitializeAsync();

            isInitialized = true;
            statusText.text = "Ready...";
            Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            statusText.text = "Init failed.";
            Debug.LogError($"Service init failed: {e.Message}");
        }
    }

    // ─── Host ─────────────────────────────────────────────

    public async void Host()
    {
        if (!isInitialized) return;

        try
        {
            statusText.text = "Creating session...";

            session = await MultiplayerService.Instance.CreateSessionAsync(
                new SessionOptions { MaxPlayers = 4, IsPrivate = false }
            );

            NetworkManager.Singleton.StartHost();

            voiceChannelName = "Voice_" + session.Code;
            await JoinVoiceChannel();

            statusText.text = "Code: " + session.Code;
            Debug.Log($"Session created. Code: {session.Code}");
        }
        catch (Exception e)
        {
            statusText.text = "Failed to host.";
            Debug.LogError($"Host failed: {e.Message}");
        }
    }

    // ─── Join ─────────────────────────────────────────────

    public async void Join()
    {
        if (!isInitialized) return;
        if (string.IsNullOrEmpty(codeInput.text))
        {
            statusText.text = "Enter a code first.";
            return;
        }

        try
        {
            statusText.text = "Joining...";

            session = await MultiplayerService.Instance.JoinSessionByCodeAsync(
                codeInput.text.Trim()
            );

            NetworkManager.Singleton.StartClient();

            voiceChannelName = "Voice_" + codeInput.text.Trim();
            await JoinVoiceChannel();

            statusText.text = "Joined!";
            Debug.Log("Joined session successfully.");
        }
        catch (Exception e)
        {
            statusText.text = "Failed to join.";
            Debug.LogError($"Join failed: {e.Message}");
        }
    }

    // ─── Solo ─────────────────────────────────────────────

    public void StartSolo()
    {
        NetworkManager.Singleton.StartHost();
        statusText.text = "Solo mode.";
        Debug.Log("Started solo session.");
    }

    // ─── Voice ────────────────────────────────────────────

    private async Task JoinVoiceChannel()
    {
        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(
                voiceChannelName,
                ChatCapability.AudioOnly
            );
            Debug.Log($"Joined voice channel: {voiceChannelName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Voice channel join failed: {e.Message}");
        }
    }

    public async void LeaveVoice()
    {
        if (!string.IsNullOrEmpty(voiceChannelName))
        {
            try
            {
                await VivoxService.Instance.LeaveChannelAsync(voiceChannelName);
                Debug.Log("Left voice channel.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Leave voice failed: {e.Message}");
            }
        }
    }

    // ─── Scene Loading ────────────────────────────────────

    public void LoadGameScene()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "AboodScene", LoadSceneMode.Single);
    }

    // ─── Leave Session ────────────────────────────────────

    public async void LeaveSession()
    {
        try
        {
            LeaveVoice();

            if (session != null)
            {
                await session.LeaveAsync();
                session = null;
            }

            NetworkManager.Singleton.Shutdown();
            statusText.text = "Left session.";
        }
        catch (Exception e)
        {
            Debug.LogError($"Leave session failed: {e.Message}");
        }
    }

    // ─── Getters ──────────────────────────────────────────

    public string GetSessionCode() => session?.Code ?? "";
    public int GetPlayerCount() => session?.Players.Count ?? 1;
    public bool IsHost() => NetworkManager.Singleton.IsHost;
}