using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ElevatorManager : NetworkBehaviour
{
    [Header("Interaction")]
    public float interactRange = 3f;
    public GameObject buttonPrompt;         // "Host: Press E to launch", hidden by default

    [Header("Countdown")]
    public float countdownTime = 10f;
    public TextMeshProUGUI countdownText;   // optional

    [Header("Doors")]
    public Animator doorAnimator;           // optional — needs a "Close" trigger
    public Transform insideZoneCenter;      // where absent players teleport
    public float insideZoneRadius = 3f;

    private NetworkVariable<bool> isLaunching = new NetworkVariable<bool>(false);
    private NetworkVariable<float> timeLeft = new NetworkVariable<float>(0f);

    private Transform localPlayer;
    private bool inRange = false;
    private bool doorsClosed = false;

    private void Start()
    {
        if (buttonPrompt != null) buttonPrompt.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void OnEnable()  { PlayerInputs.OnInteractInput += HandleInteract; }
    private void OnDisable() { PlayerInputs.OnInteractInput -= HandleInteract; }

    private void Update()
    {
        if (localPlayer == null) { localPlayer = FindLocalPlayer(); }
        else if (!isLaunching.Value)
        {
            bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
            float dist = Vector3.Distance(localPlayer.position, transform.position);
            bool nowInRange = isHost && dist <= interactRange;

            if (nowInRange != inRange)
            {
                inRange = nowInRange;
                if (buttonPrompt != null) buttonPrompt.SetActive(inRange);
            }
        }

        if (isLaunching.Value && countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = Mathf.CeilToInt(timeLeft.Value).ToString();
        }

        if (IsServer && isLaunching.Value)
            TickCountdown();
    }

    private void HandleInteract()
    {
        if (!inRange || isLaunching.Value) return;

        if (LevelSelectionManager.Instance == null ||
            LevelSelectionManager.Instance.GetSelectedSceneName() == null)
        {
            Debug.Log("No level selected yet.");
            return;
        }

        StartLaunchServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartLaunchServerRpc(ServerRpcParams p = default)
    {
        if (p.Receive.SenderClientId != NetworkManager.ServerClientId) return;
        if (isLaunching.Value) return;

        isLaunching.Value = true;
        timeLeft.Value = countdownTime;
        doorsClosed = false;

        if (buttonPrompt != null) buttonPrompt.SetActive(false);
    }

    private void TickCountdown()
    {
        timeLeft.Value -= Time.deltaTime;

        if (!doorsClosed && timeLeft.Value <= 3f)
        {
            doorsClosed = true;
            CloseDoorsClientRpc();
        }

        if (timeLeft.Value <= 0f)
        {
            isLaunching.Value = false;
            LaunchLevel();
        }
    }

    [ClientRpc]
    private void CloseDoorsClientRpc()
    {
        if (doorAnimator != null) doorAnimator.SetTrigger("Close");
    }

    private void LaunchLevel()
    {
        TeleportOutsidePlayers();

        string scene = LevelSelectionManager.Instance.GetSelectedSceneName();
        if (string.IsNullOrEmpty(scene)) return;

        MultiplayerManager.Instance.LoadGameScene(scene);
    }

    private void TeleportOutsidePlayers()
    {
        if (insideZoneCenter == null) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            Transform t = client.PlayerObject.transform;
            if (Vector3.Distance(t.position, insideZoneCenter.position) > insideZoneRadius)
            {
                CharacterController cc = t.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                Vector3 offset = Random.insideUnitSphere;
                offset.y = 0;
                t.position = insideZoneCenter.position + offset;
                if (cc != null) cc.enabled = true;
            }
        }
    }

    private Transform FindLocalPlayer()
    {
        foreach (var ps in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            var no = ps.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner) return ps.transform;
        }
        return null;
    }
}