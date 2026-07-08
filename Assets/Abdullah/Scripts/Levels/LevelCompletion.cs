using Unity.Netcode;
using UnityEngine;

public class LevelCompletion : NetworkBehaviour
{
    [Header("This Level")]
    public string levelSceneName = "Level-1";   // must match the scene name
    public int rewardScore = 100;

    private bool completed = false;

    // call this when the objective is finished (server-side)
    public void CompleteLevel()
    {
        if (!IsServer) return;
        if (completed) return;
        completed = true;

        // mark it done so it shows completed in the briefing
        LevelProgressManager.Instance?.MarkCompleted(levelSceneName);

        // give every player the reward
        GiveRewardsClientRpc(rewardScore);

        // send everyone back to the office
        MultiplayerManager.Instance.LoadGameScene("Office-Level");
    }

    [ClientRpc]
    private void GiveRewardsClientRpc(int reward)
    {
        // add reward to the local player's progression
        foreach (var pp in FindObjectsByType<PlayerProgression>(FindObjectsSortMode.None))
        {
            NetworkObject no = pp.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner)
            {
                pp.AddScore(reward);
                Debug.Log($"Level reward: +{reward} score");
                break;
            }
        }
    }
}