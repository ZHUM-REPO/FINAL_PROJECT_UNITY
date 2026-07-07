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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
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
            if (statusText != null) statusText.text = "Ready...";
            Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            if (statusText != null) statusText.text = "Init failed.";
            Debug.LogError($"Service init failed: {e.Message}");
        }
    }

    // ─── Host ───────────────────────────────────────────────

    public async Task<bool> Host()
    {
        if (!isInitialized) return false;

        try
        {
            if (statusText != null) statusText.text = "Creating session...";

            // WithRelayNetwork() routes the connection through Unity Relay
            // (works across the internet). The SDK starts the host
            // connection itself — do NOT call StartHost() manually.
            var options = new SessionOptions { MaxPlayers = 4, IsPrivate = false }
                .WithRelayNetwork();

            session = await MultiplayerService.Instance.CreateSessionAsync(options);

            voiceChannelName = "Voice_" + session.Code;
            await JoinVoiceChannel();

            if (statusText != null) statusText.text = "Code: " + session.Code;
            Debug.Log($"Session created. Code: {session.Code}");
            return true;
        }
        catch (Exception e)
        {
            if (statusText != null) statusText.text = "Failed to host.";
            Debug.LogError($"Host failed: {e.Message}");
            return false;
        }
    }

    // ─── Join ───────────────────────────────────────────────

    public async Task<bool> Join(string code)
    {
        if (!isInitialized) return false;
        if (string.IsNullOrEmpty(code))
        {
            if (statusText != null) statusText.text = "Enter a code first.";
            return false;
        }

        try
        {
            if (statusText != null) statusText.text = "Joining...";

            // Joining a relay-configured session connects this client
            // through Relay automatically — do NOT call StartClient().
            // Netcode scene sync then pulls us into the host's scene.
            session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            voiceChannelName = "Voice_" + code;
            await JoinVoiceChannel();

            if (statusText != null) statusText.text = "Joined!";
            Debug.Log("Joined session successfully.");
            return true;
        }
        catch (Exception e)
        {
            if (statusText != null) statusText.text = "Failed to join.";
            Debug.LogError($"Join failed: {e.Message}");
            return false;
        }
    }

    // ─── Solo ───────────────────────────────────────────────

    public void StartSolo()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("NetworkManager not found — loading office directly.");
            SceneManager.LoadScene("Level-0");
            return;
        }

        NetworkManager.Singleton.StartHost();

        // load the office through the NETWORKED scene manager (same as hosting)
        // so scene NetworkObjects like KitSelectionManager spawn properly
        NetworkManager.Singleton.SceneManager.LoadScene("Level-0", LoadSceneMode.Single);

        if (statusText != null) statusText.text = "Solo mode.";
        Debug.Log("Started solo session.");
    }

    // ─── Voice ──────────────────────────────────────────────

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

    // ─── Scene Loading ──────────────────────────────────────

    public void LoadOfficeScene()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Level-0", LoadSceneMode.Single);
    }

    public void LoadGameScene(string sceneName)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName, LoadSceneMode.Single);
    }

    // ─── Leave Session ──────────────────────────────────────

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

            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.Shutdown();

            if (statusText != null) statusText.text = "Left session.";
            SceneManager.LoadScene("Main-Menu");
        }
        catch (Exception e)
        {
            Debug.LogError($"Leave session failed: {e.Message}");
        }
    }

    // ─── Getters ────────────────────────────────────────────

    public string GetSessionCode() => session?.Code ?? "";
    public int GetPlayerCount() => session?.Players.Count ?? 1;
    public bool IsHost() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
    public bool IsInitialized() => isInitialized;
}