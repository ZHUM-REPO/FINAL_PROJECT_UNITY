using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerDeath : NetworkBehaviour
{
    [Header("Spectate")]
    [SerializeField] private KeyCode cycleSpectateKey = KeyCode.Space;

    [Header("Return on total wipe")]
    [SerializeField] private float wipeReturnDelay = 3f;

    // synced downed state — server writes, everyone reads
    private NetworkVariable<bool> networkIsDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private PlayerStats playerStats;
    private AbodiMovements movements;
    private AbodiCamera cameraScript;
    private KitManager kitManager;
    private CinemachineCamera myCamera;

    // spectating
    private List<PlayerDeath> livingTeammates = new List<PlayerDeath>();
    private int spectateIndex = 0;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        movements = GetComponent<AbodiMovements>();
        cameraScript = GetComponent<AbodiCamera>();
        kitManager = GetComponent<KitManager>();
        myCamera = GetComponentInChildren<CinemachineCamera>();
    }

    public override void OnNetworkSpawn()
    {
        networkIsDead.OnValueChanged += OnDeadStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        networkIsDead.OnValueChanged -= OnDeadStateChanged;
    }

    public bool IsDead() => networkIsDead.Value;

    private void Update()
    {
        // the SERVER watches for players hitting 0 health
        if (IsServer && !networkIsDead.Value && playerStats.IsDead())
        {
            Die();
        }

        // the downed OWNER can cycle the spectate target
        if (IsOwner && networkIsDead.Value)
        {
            if (Input.GetKeyDown(cycleSpectateKey))
                CycleSpectateTarget();
        }
    }

    // ─── Death (server) ─────────────────────────────────────

    private void Die()
    {
        if (!IsServer) return;
        networkIsDead.Value = true;   // syncs to everyone
        Debug.Log($"{gameObject.name} has died.");

        // check if the whole team is now down
        CheckForTotalWipe();
    }

    private void OnDeadStateChanged(bool wasDead, bool isNowDead)
    {
        if (isNowDead)
            EnterDownedState();
        else
            ExitDownedState();
    }

    // ─── Downed state (all clients) ─────────────────────────

    private void EnterDownedState()
    {
        // disable control for the owner; body stays in the world
        if (IsOwner)
        {
            if (movements != null) movements.enabled = false;
            if (cameraScript != null) cameraScript.enabled = false;
            if (kitManager != null) kitManager.enabled = false;

            // start spectating a living teammate
            BuildLivingTeammatesList();
            spectateIndex = 0;
            SpectateCurrent();

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void ExitDownedState()
    {
        // revived — restore control for the owner
        if (IsOwner)
        {
            if (movements != null) movements.enabled = true;
            if (cameraScript != null) cameraScript.enabled = true;
            if (kitManager != null) kitManager.enabled = true;

            // re-activate own camera
            if (myCamera != null) myCamera.Priority = 20;
        }
    }

    // ─── Spectating (owner only) ────────────────────────────

    private void BuildLivingTeammatesList()
    {
        livingTeammates.Clear();
        foreach (var pd in FindObjectsByType<PlayerDeath>(FindObjectsSortMode.None))
        {
            if (pd == this) continue;
            if (!pd.IsDead())
                livingTeammates.Add(pd);
        }
    }

    private void SpectateCurrent()
    {
        // lower our own camera priority
        if (myCamera != null) myCamera.Priority = 0;

        BuildLivingTeammatesList();
        if (livingTeammates.Count == 0) return;   // no one left to watch

        spectateIndex = Mathf.Clamp(spectateIndex, 0, livingTeammates.Count - 1);
        PlayerDeath target = livingTeammates[spectateIndex];

        // raise the teammate's camera so we see through their eyes
        CinemachineCamera targetCam = target.GetComponentInChildren<CinemachineCamera>(true);
        if (targetCam != null) targetCam.Priority = 25;
    }

    private void CycleSpectateTarget()
    {
        BuildLivingTeammatesList();
        if (livingTeammates.Count == 0) return;

        // reset previous target's camera priority
        PlayerDeath prev = (spectateIndex < livingTeammates.Count) ? livingTeammates[spectateIndex] : null;

        spectateIndex = (spectateIndex + 1) % livingTeammates.Count;
        SpectateCurrent();
    }

    // ─── Revive (called by ResurrectSpell via server) ───────

    public void Resurrect(float healthPercent)
    {
        // route through the server so the revive syncs
        ResurrectServerRpc(healthPercent);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResurrectServerRpc(float healthPercent)
    {
        if (!networkIsDead.Value) return;

        // restore health on the server
        playerStats.myHealth = playerStats.maxHealth * Mathf.Clamp01(healthPercent);
        SyncRevivedHealthClientRpc(playerStats.myHealth);

        networkIsDead.Value = false;   // clears downed state for everyone
        Debug.Log($"{gameObject.name} resurrected with {playerStats.myHealth} HP");
    }

    [ClientRpc]
    private void SyncRevivedHealthClientRpc(float health)
    {
        playerStats.myHealth = health;
    }

    // ─── Total wipe check (server) ──────────────────────────

    private void CheckForTotalWipe()
    {
        if (!IsServer) return;

        // are ALL players down?
        foreach (var pd in FindObjectsByType<PlayerDeath>(FindObjectsSortMode.None))
        {
            if (!pd.IsDead())
                return;   // at least one alive — no wipe
        }

        // everyone is down → failed run, return to office with NO reward
        Debug.Log("[PlayerDeath] Total wipe — returning to office, no reward.");
        StartCoroutine(ReturnToOfficeAfterDelay());
    }

    private System.Collections.IEnumerator ReturnToOfficeAfterDelay()
    {
        yield return new WaitForSeconds(wipeReturnDelay);

        if (MultiplayerManager.Instance != null)
            MultiplayerManager.Instance.LoadGameScene("Office-Level");
    }
}