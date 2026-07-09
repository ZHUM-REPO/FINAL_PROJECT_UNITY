using Unity.Netcode;
using UnityEngine;

public class LevelCompletion : NetworkBehaviour
{
    [Header("This Level")]
    public string levelSceneName = "Level-3";
    public int rewardScore = 2000;

    [Header("Win Condition")]
    public BossAI boss;
    public float returnDelay = 4f;

    private bool completed = false;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[LevelCompletion] Spawned. Boss assigned = {boss != null}");
        if (boss != null)
            boss.Died += OnBossDied;
    }

    public override void OnNetworkDespawn()
    {
        if (boss != null)
            boss.Died -= OnBossDied;
    }

    private void OnBossDied()
    {
        Debug.Log($"[LevelCompletion] Boss died event received. IsServer={IsServer}");
        if (!IsServer) return;
        CompleteLevel();
    }

    public void CompleteLevel()
    {
        if (!IsServer) return;
        if (completed) return;
        completed = true;

        Debug.Log("[LevelCompletion] Completing level...");

        // mark completed (guarded so it can't stop the flow)
        if (LevelProgressManager.Instance != null)
        {
            try { LevelProgressManager.Instance.MarkCompleted(levelSceneName); }
            catch (System.Exception e) { Debug.LogWarning($"MarkCompleted failed: {e.Message}"); }
        }

        // give rewards to everyone
        GiveRewardsClientRpc(rewardScore);

        // return to office after a delay
        StartCoroutine(ReturnToOfficeAfterDelay());
    }

    private System.Collections.IEnumerator ReturnToOfficeAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        Debug.Log("[LevelCompletion] Returning to office...");
        if (MultiplayerManager.Instance != null)
            MultiplayerManager.Instance.LoadGameScene("Office-Level");
        else
            Debug.LogError("[LevelCompletion] MultiplayerManager missing — can't return to office!");
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
                Debug.Log($"[LevelCompletion] Reward: +{reward} score");
                break;
            }
        }
    }
}