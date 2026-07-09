using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossArenaTrigger : NetworkBehaviour
{
    [Header("Boss to wake")]
    [SerializeField] BossAI boss;   // the boss (stays active, starts dormant)

    [Header("Extra objects to enable on entry")]
    [SerializeField] GameObject[] objectsToActivate;  // e.g. the boss UI canvas

    [Header("Settings")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] bool triggerOnce = true;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // only the server decides when the fight starts
        if (!IsServer) return;
        if (triggerOnce && triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        StartFightClientRpc();

        // stop the trigger from firing again
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    // runs on everyone: turn on the boss UI and begin the fight
    [ClientRpc]
    private void StartFightClientRpc()
    {
        if (objectsToActivate != null)
        {
            foreach (GameObject go in objectsToActivate)
                if (go != null) go.SetActive(true);
        }

        if (boss != null)
            boss.BeginFight();
    }
}