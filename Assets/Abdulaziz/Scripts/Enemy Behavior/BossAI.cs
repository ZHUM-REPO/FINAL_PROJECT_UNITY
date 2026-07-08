using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : NetworkBehaviour
{
    enum State { Intro, StandUp, Chase, NormalAttack, HeavyAttack, Retreat, Dead }

    [Header("Target")]
    [SerializeField] Transform player;
    [SerializeField] Damage damageable;

    [Header("Targeting")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] float targetSwitchInterval = 30f;
    [SerializeField] bool ignoreDeadPlayers = true;

    [Header("Intro")]
    [SerializeField] float wakeDelay = 3f;
    [SerializeField] float standUpDuration = 1.5f;

    [Header("Movement")]
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float rotationSpeed = 720f;

    [Header("Normal Attack")]
    [SerializeField] float attackRange = 2.5f;
    [SerializeField] float normalDamage = 10f;
    [SerializeField] float normalHitDelay = 0.4f;
    [SerializeField] float normalAttackDuration = 0.9f;
    [SerializeField] int attacksBeforeRetreat = 5;

    [Header("Heavy Attack (AoE)")]
    [SerializeField] float heavyDamage = 25f;
    [SerializeField] float heavyHitDelay = 0.7f;
    [SerializeField] float heavyAttackDuration = 1.4f;
    [SerializeField] float heavyRecovery = 0.5f;
    [SerializeField] float heavyCooldown = 8f;
    [SerializeField] float heavyRadius = 4f;
    [SerializeField] LayerMask damageableMask;

    [Header("Retreat")]
    [SerializeField] float retreatDistance = 10f;
    [SerializeField] float maxRetreatTime = 3f;

    [Header("Phase 2")]
    [SerializeField] float secondPhaseHealthFraction = 0.5f;
    [SerializeField] GameObject minionPrefab;
    [SerializeField] Transform[] minionSpawnPoints;
    [SerializeField] int minionsPerWave = 3;
    [SerializeField] float minionSpawnRadius = 3f;
    [SerializeField] float minionSpawnInterval = 30f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = true;

    public event Action StoodUp;
    public event Action FightStarted;
    public event Action PhaseTwoStarted;
    public event Action Died;
    public event Action<float> SpeedChanged;
    public event Action NormalAttacked;
    public event Action HeavyAttacked;

    NavMeshAgent agent;

    State state;
    float lastSpeed01 = -1f;
    float introTimer;
    float targetSwitchTimer;

    float attackTimer;
    bool hitApplied;
    int normalAttackCount;
    float heavyCooldownTimer;
    float retreatTimer;

    bool secondPhaseStarted;
    float minionSpawnTimer;

    readonly Collider[] aoeBuffer = new Collider[16];

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.speed = runSpeed;

        if (damageable == null) damageable = GetComponent<Damage>();
        if (damageable == null) damageable = GetComponentInChildren<Damage>();

        if (damageable == null)
            Debug.LogError("[BossAI] No Damage component found.", this);
    }

    public override void OnNetworkSpawn()
    {
        // clients don't run the AI — disable the agent so it can't move locally.
        // the NetworkTransform syncs position from the server instead.
        if (!IsServer && agent != null)
            agent.enabled = false;

        // everyone subscribes to health events (for UI + phase reactions)
        if (damageable != null)
        {
            damageable.HealthChanged += OnHealthChanged;
            damageable.Died += OnDied;
        }

        if (IsServer)
            InitServerState();
    }

    public override void OnNetworkDespawn()
    {
        if (damageable != null)
        {
            damageable.HealthChanged -= OnHealthChanged;
            damageable.Died -= OnDied;
        }
    }

    void InitServerState()
    {
        state = State.Intro;
        introTimer = 0f;
        normalAttackCount = 0;
        heavyCooldownTimer = heavyCooldown;
        secondPhaseStarted = false;
        StopAgent();

        if (damageable != null)
            damageable.Invulnerable = true;
    }

    void Update()
    {
        if (!IsServer) return;   // AI runs on the server only

        UpdateTarget();
        if (player == null) return;
        if (state == State.Dead) return;

        if (state == State.Intro) { TickIntro(); return; }
        if (state == State.StandUp) { TickStandUp(); return; }

        heavyCooldownTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Chase: TickChase(); break;
            case State.NormalAttack: TickNormalAttack(); break;
            case State.HeavyAttack: TickHeavyAttack(); break;
            case State.Retreat: TickRetreat(); break;
        }

        if (secondPhaseStarted)
        {
            minionSpawnTimer -= Time.deltaTime;
            if (minionSpawnTimer <= 0f)
            {
                minionSpawnTimer = minionSpawnInterval;
                SpawnMinions();
            }
        }

        ReportSpeed(agent.velocity.magnitude);
    }

    void TickIntro()
    {
        introTimer += Time.deltaTime;
        if (introTimer >= wakeDelay)
        {
            state = State.StandUp;
            introTimer = 0f;
            StoodUpClientRpc();   // broadcast stand-up to everyone
        }
    }

    void TickStandUp()
    {
        introTimer += Time.deltaTime;
        if (introTimer >= standUpDuration)
        {
            if (damageable != null) damageable.Invulnerable = false;
            FightStartedClientRpc();
            EnterChase();
        }
    }

    void TickChase()
    {
        if (PlanarDistanceToPlayer() <= attackRange)
        {
            if (heavyCooldownTimer <= 0f) EnterHeavyAttack();
            else EnterNormalAttack();
            return;
        }

        agent.SetDestination(player.position);
        FaceDirection(agent.desiredVelocity);
    }

    void TickNormalAttack()
    {
        FacePlayer();
        attackTimer += Time.deltaTime;

        if (!hitApplied && attackTimer >= normalHitDelay)
        {
            hitApplied = true;
            normalAttackCount++;
            if (PlanarDistanceToPlayer() <= attackRange)
            {
                IDamageable target = player.GetComponentInParent<IDamageable>();
                if (target != null) target.TakeDamage(normalDamage);
            }
        }

        if (attackTimer >= normalAttackDuration)
        {
            if (normalAttackCount >= attacksBeforeRetreat)
            {
                normalAttackCount = 0;
                EnterRetreat();
            }
            else if (heavyCooldownTimer <= 0f) EnterHeavyAttack();
            else if (PlanarDistanceToPlayer() > attackRange) EnterChase();
            else EnterNormalAttack();
        }
    }

    void TickHeavyAttack()
    {
        if (!hitApplied) FacePlayer();

        attackTimer += Time.deltaTime;

        if (!hitApplied && attackTimer >= heavyHitDelay)
        {
            hitApplied = true;
            DealAoeDamage();
        }

        if (attackTimer >= heavyAttackDuration + heavyRecovery)
            EnterRetreat();
    }

    void TickRetreat()
    {
        retreatTimer += Time.deltaTime;
        if (PlanarDistanceToPlayer() >= retreatDistance || retreatTimer >= maxRetreatTime)
        {
            EnterChase();
            return;
        }

        FaceDirection(agent.desiredVelocity);
    }

    void EnterChase()
    {
        state = State.Chase;
        agent.isStopped = false;
    }

    void EnterNormalAttack()
    {
        state = State.NormalAttack;
        attackTimer = 0f;
        hitApplied = false;
        StopAgent();
        NormalAttackClientRpc();
    }

    void EnterHeavyAttack()
    {
        state = State.HeavyAttack;
        attackTimer = 0f;
        hitApplied = false;
        heavyCooldownTimer = heavyCooldown;
        StopAgent();
        HeavyAttackClientRpc();
    }

    void EnterRetreat()
    {
        state = State.Retreat;
        retreatTimer = 0f;
        agent.isStopped = false;

        Vector3 away = transform.position - player.position;
        away.y = 0f;
        Vector3 target = transform.position + away.normalized * retreatDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, retreatDistance, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(target);
    }

    void OnHealthChanged(float current)
    {
        // this runs on everyone (health is networked), but only the server
        // makes phase-2 gameplay decisions
        if (!IsServer) return;
        if (secondPhaseStarted || damageable.IsDead) return;

        if (current <= damageable.MaxHealth * secondPhaseHealthFraction)
        {
            secondPhaseStarted = true;
            minionSpawnTimer = minionSpawnInterval;
            SpawnMinions();
            PhaseTwoClientRpc();
        }
    }

    void OnDied()
    {
        if (!IsServer) return;
        if (state == State.Dead) return;

        state = State.Dead;
        StopAgent();
        DiedClientRpc();
    }

    void SpawnMinions()
    {
        if (minionPrefab == null) return;

        if (minionSpawnPoints != null && minionSpawnPoints.Length > 0)
        {
            foreach (Transform point in minionSpawnPoints)
            {
                Vector3 pos = point != null ? point.position : transform.position;
                Quaternion rot = point != null ? point.rotation : transform.rotation;
                SpawnMinionAt(pos, rot);
            }
        }
        else
        {
            for (int i = 0; i < minionsPerWave; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * minionSpawnRadius;
                Vector3 pos = transform.position + new Vector3(offset.x, 0f, offset.y);
                SpawnMinionAt(pos, transform.rotation);
            }
        }
    }

    void SpawnMinionAt(Vector3 pos, Quaternion rot)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, minionSpawnRadius + 2f, NavMesh.AllAreas))
            pos = hit.position;

        // network-spawn the minion so all clients see it
        GameObject minion = Instantiate(minionPrefab, pos, rot);
        NetworkObject netObj = minion.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn();
        else
            Debug.LogError("[BossAI] Minion prefab has no NetworkObject!", this);
    }

    void DealAoeDamage()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, heavyRadius, aoeBuffer, damageableMask);
        for (int i = 0; i < count; i++)
        {
            if (aoeBuffer[i].transform.IsChildOf(transform)) continue;
            IDamageable target = aoeBuffer[i].GetComponentInParent<IDamageable>();
            if (target != null) target.TakeDamage(heavyDamage);
        }
    }

    void StopAgent()
    {
        if (agent == null || !agent.enabled) return;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    void ReportSpeed(float planarSpeed)
    {
        float speed01 = runSpeed > 0f ? Mathf.Clamp01(planarSpeed / runSpeed) : 0f;
        if (Mathf.Abs(speed01 - lastSpeed01) > 0.01f)
        {
            lastSpeed01 = speed01;
            SpeedClientRpc(speed01);
        }
    }

    void FaceDirection(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);
    }

    void FacePlayer() => FaceDirection(player.position - transform.position);

    float PlanarDistanceToPlayer()
    {
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

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
            ? others[UnityEngine.Random.Range(0, others.Count)]
            : valid[0];
    }

    // ─── ClientRpcs: broadcast animation/events to everyone ──

    [ClientRpc] void StoodUpClientRpc() => StoodUp?.Invoke();
    [ClientRpc] void FightStartedClientRpc() => FightStarted?.Invoke();
    [ClientRpc] void PhaseTwoClientRpc() => PhaseTwoStarted?.Invoke();
    [ClientRpc] void DiedClientRpc() => Died?.Invoke();
    [ClientRpc] void NormalAttackClientRpc() => NormalAttacked?.Invoke();
    [ClientRpc] void HeavyAttackClientRpc() => HeavyAttacked?.Invoke();
    [ClientRpc] void SpeedClientRpc(float s) => SpeedChanged?.Invoke(s);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, heavyRadius);
    }
}