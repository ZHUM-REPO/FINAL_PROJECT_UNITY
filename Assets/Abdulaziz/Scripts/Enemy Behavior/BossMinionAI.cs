using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class BossMinionAI : NetworkBehaviour, IDamageable
{
    enum State { Chase, Attack, Dead }

    [Header("Health")]
    [SerializeField] float maxHealth = 20f;
    [SerializeField] float destroyDelay = 5f;   // seconds after death before the minion is destroyed

    // shared health — server writes, everyone reads
    private NetworkVariable<float> networkHealth = new NetworkVariable<float>(
        20f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Target")]
    [SerializeField] Transform player;

    [Header("Targeting")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] float targetSwitchInterval = 30f;
    [SerializeField] bool ignoreDeadPlayers = true;

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float damage = 4f;
    [SerializeField] float hitDelay = 0.3f;
    [SerializeField] float attackDuration = 0.7f;

    [Header("Animation")]
    [SerializeField] float speedDampTime = 0.1f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => networkHealth.Value;
    public bool IsDead => state == State.Dead || networkHealth.Value <= 0f;

    NavMeshAgent agent;
    Animator animator;
    State state;
    float attackTimer;
    bool hitApplied;
    float targetSwitchTimer;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int DeathHash = Animator.StringToHash("Death");

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            networkHealth.Value = maxHealth;
            state = State.Chase;
        }
        else
        {
            // clients don't run the AI — the NavMeshAgent is server-driven,
            // position comes via NetworkTransform
            if (agent != null) agent.enabled = false;
        }
    }

    void Update()
    {
        if (!IsServer) return;          // AI runs on the server only
        if (state == State.Dead) return;

        UpdateTarget();
        if (player == null) return;

        switch (state)
        {
            case State.Chase: TickChase(); break;
            case State.Attack: TickAttack(); break;
        }

        float speed01 = agent.speed > 0f ? Mathf.Clamp01(agent.velocity.magnitude / agent.speed) : 0f;
        SetSpeedClientRpc(speed01);
    }

    // IDamageable: the player's spells call this when they hit the minion.
    // Routes to the server so the shared health is the one that changes.
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        TakeDamageServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float amount)
    {
        if (IsDead) return;

        networkHealth.Value = Mathf.Max(0f, networkHealth.Value - amount);
        if (networkHealth.Value <= 0f) Die();
    }

    void Die()
    {
        if (state == State.Dead) return;
        state = State.Dead;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // play the death anim on everyone
        DeathClientRpc();

        // despawn the networked object after the death anim plays
        StartCoroutine(DespawnAfterDelay());
    }

    private System.Collections.IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn();   // removes it for all clients
        else
            Destroy(gameObject);
    }

    // ─── Targeting (server-only) ──────────────────────────

    void UpdateTarget()
    {
        targetSwitchTimer -= Time.deltaTime;

        bool targetInvalid = player == null
                             || !player.gameObject.activeInHierarchy
                             || (ignoreDeadPlayers && player.TryGetComponent(out PlayerStats ps) && ps.IsDead());

        if (!targetInvalid && targetSwitchTimer > 0f) return;

        targetSwitchTimer = targetSwitchInterval;
        player = PickDifferentPlayer();
    }

    Transform PickDifferentPlayer()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(playerTag);
        List<Transform> valid = new List<Transform>();

        foreach (GameObject p in tagged)
        {
            if (p == null || !p.activeInHierarchy) continue;
            if (ignoreDeadPlayers && p.TryGetComponent(out PlayerStats ps) && ps.IsDead()) continue;
            valid.Add(p.transform);
        }

        if (valid.Count == 0) return null;
        if (valid.Count == 1) return valid[0];

        List<Transform> others = new List<Transform>();
        foreach (Transform t in valid)
            if (t != player) others.Add(t);

        return others.Count > 0
            ? others[Random.Range(0, others.Count)]
            : valid[0];
    }

    // ─── State ticks (server-only) ────────────────────────

    void TickChase()
    {
        if (DistanceToPlayer() <= attackRange)
        {
            EnterAttack();
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void TickAttack()
    {
        FacePlayer();
        attackTimer += Time.deltaTime;

        if (!hitApplied && attackTimer >= hitDelay)
        {
            hitApplied = true;
            if (DistanceToPlayer() <= attackRange)
            {
                IDamageable target = player.GetComponentInParent<IDamageable>();
                if (target != null) target.TakeDamage(damage);
            }
        }

        if (attackTimer >= attackDuration)
        {
            if (DistanceToPlayer() <= attackRange) EnterAttack();
            else EnterChase();
        }
    }

    void EnterChase()
    {
        state = State.Chase;
        agent.isStopped = false;
    }

    void EnterAttack()
    {
        state = State.Attack;
        attackTimer = 0f;
        hitApplied = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        AttackClientRpc();
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    float DistanceToPlayer()
    {
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    // ─── ClientRpcs: animation on all clients ─────────────

    [ClientRpc]
    private void SetSpeedClientRpc(float speed01)
    {
        animator.SetFloat(SpeedHash, speed01, speedDampTime, Time.deltaTime);
    }

    [ClientRpc]
    private void AttackClientRpc()
    {
        animator.SetTrigger(AttackHash);
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        agent.isStopped = true;
        if (TryGetComponent(out Collider col)) col.enabled = false;
        animator.SetTrigger(DeathHash);
    }
}