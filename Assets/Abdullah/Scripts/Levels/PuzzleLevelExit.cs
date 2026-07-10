using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleLevelExit : NetworkBehaviour
{
    [Header("This Level")]
    public string levelSceneName = "Level-2";
    public int rewardScore = 1000;

    [Header("Exit Settings")]
    public string playerTag = "Player";
    public bool useTriggerZone = true;      // walk into the elevator to finish
    public bool useButton = true;           // OR press E at the elevator to finish
    public float interactRange = 3f;
    public GameObject interactPrompt;       // "Press E to leave" (hidden by default)

    [Header("Timing")]
    public float returnDelay = 2f;          // brief pause before returning to office

    private bool completed = false;
    private Transform localPlayer;
    private bool inRange = false;

    private void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void OnEnable()  { PlayerInputs.OnInteractInput += HandleInteract; }
    private void OnDisable() { PlayerInputs.OnInteractInput -= HandleInteract; }

    private void Update()
    {
        if (!useButton || completed) return;

        if (localPlayer == null) { localPlayer = FindLocalPlayer(); return; }

        float dist = Vector3.Distance(localPlayer.position, transform.position);
        bool nowInRange = dist <= interactRange;

        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (interactPrompt != null) interactPrompt.SetActive(inRange);
        }
    }

    // ─── Button (press E at the elevator) ───────────────────

    private void HandleInteract()
    {
        if (!useButton || !inRange || completed) return;
        RequestCompleteServerRpc();
    }

    // ─── Trigger zone (walk into the elevator) ──────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerZone || completed) return;
        if (!IsServer) return;   // server decides
        if (!other.CompareTag(playerTag)) return;

        CompleteLevel();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCompleteServerRpc()
    {
        CompleteLevel();
    }

    // ─── Completion ─────────────────────────────────────────

    private void CompleteLevel()
    {
        if (!IsServer) return;
        if (completed) return;
        completed = true;

        Debug.Log("[PuzzleLevelExit] Level complete!");

        // mark it done (guarded so it can't break the flow)
        if (LevelProgressManager.Instance != null)
        {
            try { LevelProgressManager.Instance.MarkCompleted(levelSceneName); }
            catch (System.Exception e) { Debug.LogWarning($"MarkCompleted failed: {e.Message}"); }
        }

        // reward everyone
        GiveRewardsClientRpc(rewardScore);

        // return to office after a short delay
        StartCoroutine(ReturnToOfficeAfterDelay());
    }

    private System.Collections.IEnumerator ReturnToOfficeAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        if (MultiplayerManager.Instance != null)
            MultiplayerManager.Instance.LoadGameScene("Office-Level");
        else
            Debug.LogError("[PuzzleLevelExit] MultiplayerManager missing!");
    }

    [ClientRpc]
    private void GiveRewardsClientRpc(int reward)
    {
        foreach (var pp in FindObjectsByType<PlayerProgression>(FindObjectsSortMode.None))
        {
            NetworkObject no = pp.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner)
            {
                pp.AddScore(reward);
                Debug.Log($"[PuzzleLevelExit] Reward: +{reward} score");
                break;
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