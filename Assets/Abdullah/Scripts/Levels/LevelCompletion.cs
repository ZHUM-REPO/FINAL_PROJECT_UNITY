using Unity.Netcode;
using UnityEngine;

public class LevelCompletion : NetworkBehaviour
{
    [Header("This Level")]
    public string levelSceneName = "Level-3";   // exact scene name
    public int rewardScore = 2000;

    [Header("Win Condition")]
    public BossAI boss;                 // drag the boss here (Level 3)
    public float returnDelay = 4f;      // wait after the boss dies before returning

    private bool completed = false;

    public override void OnNetworkSpawn()
    {
        // listen for the boss's death (server drives completion)
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
        // only the server runs completion
        if (!IsServer) return;
        CompleteLevel();
    }

    public void CompleteLevel()
    {
        if (!IsServer) return;
        if (completed) return;
        completed = true;

        // mark it done so it shows completed in the briefing
        LevelProgressManager.Instance?.MarkCompleted(levelSceneName);

        // give every player the reward
        GiveRewardsClientRpc(rewardScore);

        // return to the office after a short delay (lets the death anim play)
        StartCoroutine(ReturnToOfficeAfterDelay());
    }

    private System.Collections.IEnumerator ReturnToOfficeAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);
        MultiplayerManager.Instance.LoadGameScene("Office-Level");
    }

    [ClientRpc]
    private void GiveRewardsClientRpc(int reward)
    {
        // add the reward to the local player's progression
        foreach (var pp in FindObjectsByType<PlayerProgression>(FindObjectsSortMode.None))
        {
            NetworkObject no = pp.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner)
            {
                pp.AddScore(reward);
                Debug.Log($"Level complete! Reward: +{reward} score");
                break;
            }
        }
    }
}