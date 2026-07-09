using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class BossMinionAI : NetworkBehaviour, IDamageable
{
    enum State { Spawning, Chase, Attack, Dead }

    [Header("Health")]
    [SerializeField] float maxHealth = 20f;
    [SerializeField] float destroyDelay = 5f;

    private NetworkVariable<float> networkHealth = new NetworkVariable<float>(
        -1f,
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

    [Header("Gravity Reaction")]
    [SerializeField] float gravityRecoverTime = 2f;   // how long agent stays off after being flung

    public float MaxHealth => maxHealth;
    public float CurrentHealth => networkHealth.Value;
    public bool IsDead => state == State.Dead
                          || (networkHealth.Value >= 0f && networkHealth.Value <= 0f);

    NavMeshAgent agent;
    Animator animator;
    Rigidbody rb;
    State state = State.Spawning;
    float attackTimer;
    bool hitApplied;
    float targetSwitchTimer;

    bool underGravityControl = false;
    float gravityRecoverTimer = 0f;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int DeathHash = Animator.StringToHash("Death");

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
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
            if (agent != null) agent.enabled = false;
            state = State.Chase;
        }
    }

    void Update()
    {
        if (!IsServer) return;
        if (state == State.Dead) return;
        if (state == State.Spawning) return;

        // if being flung by gravity, let physics take over and count down recovery
        if (underGravityControl)
        {
            gravityRecoverTimer -= Time.deltaTime;
            if (gravityRecoverTimer <= 0f)
                EndGravityControl();
            else
                return;   // skip AI while airborne
        }

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

    // ─── Gravity control (called by the gravity spells) ───

    // GRAVITY PUSH: apply a knockback impulse. Runs through the server.
    public void ApplyGravityPush(Vector3 force)
    {
        if (IsServer) DoGravityPush(force);
        else GravityPushServerRpc(force);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GravityPushServerRpc(Vector3 force)
    {
        DoGravityPush(force);
    }

    private void DoGravityPush(Vector3 force)
    {
        if (IsDead) return;
        BeginGravityControl();
        if (rb != null)
            rb.AddForce(force, ForceMode.Impulse);
    }

    // GRAVITY MOVE: the spell holds/moves the minion. Server sets its position.
    public void SetGravityHold(bool held)
    {
        if (IsServer) DoSetGravityHold(held);
        else SetGravityHoldServerRpc(held);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetGravityHoldServerRpc(bool held)
    {
        DoSetGravityHold(held);
    }

    private void DoSetGravityHold(bool held)
    {
        if (IsDead) return;
        if (held) BeginGravityControl();
        else
        {
            // when released/thrown, give a recovery window before the agent resumes
            gravityRecoverTimer = gravityRecoverTime;
        }
    }

    // move the minion to a position while held (server authoritative)
    public void MoveWhileHeld(Vector3 position)
    {
        if (IsServer) transform.position = position;
        else MoveWhileHeldServerRpc(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveWhileHeldServerRpc(Vector3 position)
    {
        transform.position = position;
    }

    // throw: apply an impulse and start recovery
    public void ThrowMinion(Vector3 force)
    {
        if (IsServer) DoThrow(force);
        else ThrowServerRpc(force);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ThrowServerRpc(Vector3 force)
    {
        DoThrow(force);
    }

    private void DoThrow(Vector3 force)
    {
        if (IsDead) return;
        BeginGravityControl();
        if (rb != null)
            rb.AddForce(force, ForceMode.Impulse);
    }

    private void BeginGravityControl()
    {
        underGravityControl = true;
        gravityRecoverTimer = gravityRecoverTime;

        // turn OFF the agent so physics can move the minion
        if (agent != null && agent.enabled)
            agent.enabled = false;

        // let the rigidbody move freely
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void EndGravityControl()
    {
        underGravityControl = false;

        // snap the minion back onto the NavMesh and re-enable the agent
        if (agent != null)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                transform.position = hit.position;

            agent.enabled = true;
        }

        if (rb != null)
            rb.isKinematic = true;   // agent drives movement again
    }

    // ─── Damage ────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        TakeDamageServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float amount)
    {
        if (state == State.Dead) return;
        if (networkHealth.Value < 0f) return;

        networkHealth.Value = Mathf.Max(0f, networkHealth.Value - amount);
        if (networkHealth.Value <= 0f) Die();
    }

    void Die()
    {
        if (state == State.Dead) return;
        state = State.Dead;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        DeathClientRpc();
        StartCoroutine(DespawnAfterDelay());
    }

    private System.Collections.IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn();
        else
            Destroy(gameObject);
    }

    // ─── Targeting ─────────────────────────────────────────

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

        return others.Count > 0 ? others[Random.Range(0, others.Count)] : valid[0];
    }

    // ─── State ticks ───────────────────────────────────────

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
        if (agent.enabled) agent.isStopped = false;
    }

    void EnterAttack()
    {
        state = State.Attack;
        attackTimer = 0f;
        hitApplied = false;
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
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

    // ─── ClientRpcs ────────────────────────────────────────

    [ClientRpc] private void SetSpeedClientRpc(float speed01) => animator.SetFloat(SpeedHash, speed01, speedDampTime, Time.deltaTime);
    [ClientRpc] private void AttackClientRpc() => animator.SetTrigger(AttackHash);
    [ClientRpc]
    private void DeathClientRpc()
    {
        if (agent != null && agent.enabled) agent.isStopped = true;
        if (TryGetComponent(out Collider col)) col.enabled = false;
        animator.SetTrigger(DeathHash);
    }
}