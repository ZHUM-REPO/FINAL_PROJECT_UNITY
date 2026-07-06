using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    enum State { Intro, StandUp, Chase, NormalAttack, HeavyAttack, Retreat, Dead }

    [Header("Target")]
    [SerializeField] Transform player;
    [SerializeField] Damage damageable; // the boss's health component (see Awake for how it's resolved)

    [Header("Targeting")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] float targetSwitchInterval = 30f; // switch to a different player this often (seconds)
    [SerializeField] bool ignoreDeadPlayers = true;    // skip downed players when picking a target

    [Header("Intro")]
    [SerializeField] float wakeDelay = 3f;         // clapping / resting pose duration before standing
    [SerializeField] float standUpDuration = 1.5f; // length of the stand-up clip

    [Header("Movement")]
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float rotationSpeed = 720f; // deg/sec

    [Header("Normal Attack")]
    [SerializeField] float attackRange = 2.5f;
    [SerializeField] float normalDamage = 10f;
    [SerializeField] float normalHitDelay = 0.4f;       // when the hit lands within the anim
    [SerializeField] float normalAttackDuration = 0.9f; // full anim length
    [SerializeField] int attacksBeforeRetreat = 5;

    [Header("Heavy Attack (AoE)")]
    [SerializeField] float heavyDamage = 25f;
    [SerializeField] float heavyHitDelay = 0.7f;
    [SerializeField] float heavyAttackDuration = 1.4f;
    [SerializeField] float heavyRecovery = 0.5f;  // extra hold after the anim before moving/rotating again
    [SerializeField] float heavyCooldown = 8f;
    [SerializeField] float heavyRadius = 4f;
    [SerializeField] LayerMask damageableMask;

    [Header("Retreat")]
    [SerializeField] float retreatDistance = 10f;
    [SerializeField] float maxRetreatTime = 3f; // failsafe if cornered

    [Header("Phase 2")]
    [SerializeField] float secondPhaseHealthFraction = 0.5f;
    [SerializeField] GameObject minionPrefab;
    [SerializeField] Transform[] minionSpawnPoints;    // optional — leave empty to spawn around the boss
    [SerializeField] int minionsPerWave = 3;           // used when no spawn points are assigned
    [SerializeField] float minionSpawnRadius = 3f;     // ring radius around the boss for auto-spawning
    [SerializeField] float minionSpawnInterval = 30f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = true; // logs each health/phase/death event to the Console

    public event Action StoodUp;             // fire the stand-up animation
    public event Action FightStarted;        // gameplay begins (health bar / music)
    public event Action PhaseTwoStarted;     // 50% hp reached
    public event Action Died;                // boss defeated (fire death animation / stop music)
    public event Action<float> SpeedChanged; // 0..1 of run speed
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
        agent.updateRotation = false; // rotate manually for tight facing
        agent.speed = runSpeed;

        // Resolve the Damage component. Look on this object first, then children
        // (the Damage component is often on the model child, not the root).
        if (damageable == null) damageable = GetComponent<Damage>();
        if (damageable == null) damageable = GetComponentInChildren<Damage>();

        // If this fires, the boss will NEVER take phase-2 or death actions, because
        // there is no Damage whose events it can subscribe to. Assign one in the inspector.
        if (damageable == null)
            Debug.LogError("[BossAI] No Damage component found on the boss or its children. " +
                           "Phase 2 and death will never trigger. Drag the boss's Damage into the field.", this);

        // Targeting is resolved dynamically at runtime (see UpdateTarget) so the boss
        // chases whichever players actually exist and are active in the level —
        // never a disabled/hidden prefab. Any inspector-assigned player is just an
        // initial value that UpdateTarget will replace on the first frame.
    }

    void OnEnable()
    {
        state = State.Intro;
        introTimer = 0f;
        normalAttackCount = 0;
        heavyCooldownTimer = heavyCooldown; // warm up before first heavy
        secondPhaseStarted = false;
        StopAgent();

        if (damageable != null)
        {
            damageable.Invulnerable = true; // safe until fully stood up

            // SUBSCRIBE: these are invoked from inside Damage.TakeDamage.
            //   Damage.HealthChanged  -> OnHealthChanged  (drives the 50% phase-2 check)
            //   Damage.Died           -> OnDied           (drives the death state)
            damageable.HealthChanged += OnHealthChanged;
            damageable.Died += OnDied;

            // Startup confirmation: if you never see this line in the Console, the boss
            // was never enabled (or the project didn't compile) — nothing downstream can run.
            if (debugLogs)
                Debug.Log($"[BossAI] Enabled & subscribed to Damage. HP = {damageable.CurrentHealth}/{damageable.MaxHealth}", this);
        }
        else
        {
            Debug.LogError("[BossAI] damageable is null in OnEnable — no health events subscribed.", this);
        }
    }

    void OnDisable()
    {
        // UNSUBSCRIBE symmetrically so we never double-subscribe on re-enable.
        if (damageable != null)
        {
            damageable.HealthChanged -= OnHealthChanged;
            damageable.Died -= OnDied;
        }
    }

    void Update()
    {
        UpdateTarget(); // co-op: pick the nearest active player, re-scanning for new/removed ones
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
            StoodUp?.Invoke();
        }
    }

    void TickStandUp()
    {
        introTimer += Time.deltaTime;
        if (introTimer >= standUpDuration)
        {
            // Drop invulnerability here so the boss can actually take damage once standing.
            // If this line never runs (e.g. standUpDuration too long / state stuck),
            // the boss stays invulnerable forever and health never changes.
            if (damageable != null) damageable.Invulnerable = false;
            FightStarted?.Invoke();
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
            else EnterNormalAttack(); // keep swinging
        }
    }

    void TickHeavyAttack()
    {
        // Only aim during the wind-up; once the hit lands, lock facing so the boss
        // doesn't keep rotating through the strike and recovery.
        if (!hitApplied) FacePlayer();

        attackTimer += Time.deltaTime;

        if (!hitApplied && attackTimer >= heavyHitDelay)
        {
            hitApplied = true;
            DealAoeDamage();
        }

        // Stay planted for the full clip PLUS a recovery hold before it may move again.
        if (attackTimer >= heavyAttackDuration + heavyRecovery)
            EnterRetreat(); // always retreat after a heavy
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
        NormalAttacked?.Invoke();
    }

    void EnterHeavyAttack()
    {
        state = State.HeavyAttack;
        attackTimer = 0f;
        hitApplied = false;
        heavyCooldownTimer = heavyCooldown;
        StopAgent();
        HeavyAttacked?.Invoke();
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

    // CALLED BY: Damage.HealthChanged (fired inside Damage.TakeDamage on every hit).
    void OnHealthChanged(float current)
    {
        if (debugLogs)
            Debug.Log($"[BossAI] HealthChanged received: {current}/{damageable.MaxHealth} " +
                      $"(phase-2 threshold = {damageable.MaxHealth * secondPhaseHealthFraction})", this);

        if (secondPhaseStarted || damageable.IsDead) return;

        if (current <= damageable.MaxHealth * secondPhaseHealthFraction)
        {
            secondPhaseStarted = true;
            minionSpawnTimer = minionSpawnInterval;

            if (debugLogs) Debug.Log("[BossAI] Phase 2 reached — spawning first minion wave.", this);

            SpawnMinions();           // first wave right away
            PhaseTwoStarted?.Invoke(); // notify UI / music / VFX
        }
    }

    // CALLED BY: Damage.Died (fired inside Damage.TakeDamage when health hits 0).
    void OnDied()
    {
        if (state == State.Dead) return;

        if (debugLogs) Debug.Log("[BossAI] Died received — entering Dead state, firing death animation.", this);

        state = State.Dead;
        StopAgent();
        Died?.Invoke(); // BossAnimation pulls the Death trigger; BossAudio/UI react here too
    }

    void SpawnMinions()
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning("[BossAI] Phase 2 fired but Minion Prefab is empty — no minions will spawn.", this);
            return;
        }

        // If spawn points are assigned, use them; otherwise spawn around the boss
        // within minionSpawnRadius (default 3m), snapped onto the NavMesh.
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
        // Pull the point onto the nearest walkable NavMesh spot so the agent spawns valid.
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, minionSpawnRadius + 2f, NavMesh.AllAreas))
            pos = hit.position;

        GameObject minion = Instantiate(minionPrefab, pos, rot);
        minion.SetActive(true); // prefab may be saved disabled — turn the spawned copy on
    }

    void DealAoeDamage()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, heavyRadius, aoeBuffer, damageableMask);
        for (int i = 0; i < count; i++)
        {
            if (aoeBuffer[i].transform.IsChildOf(transform)) continue; // skip self

            // search parents so a collider on a child still finds the health on the root
            IDamageable target = aoeBuffer[i].GetComponentInParent<IDamageable>();
            if (target != null)
                target.TakeDamage(heavyDamage);
        }
    }

    void StopAgent()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // snap the run blend down cleanly
    }

    void ReportSpeed(float planarSpeed)
    {
        float speed01 = runSpeed > 0f ? Mathf.Clamp01(planarSpeed / runSpeed) : 0f;
        if (Mathf.Abs(speed01 - lastSpeed01) > 0.01f)
        {
            lastSpeed01 = speed01;
            SpeedChanged?.Invoke(speed01);
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

    // CO-OP TARGETING
    // Rotates the boss's focus between the active players every targetSwitchInterval
    // seconds, so it doesn't fixate on one. Also switches immediately if the current
    // target goes inactive or dies. Inactive objects (e.g. a disabled prefab) are
    // filtered out, so a hidden player is never targeted.
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
        // gather every active (and, optionally, alive) player
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(playerTag);
        List<Transform> valid = new List<Transform>();

        foreach (GameObject p in tagged)
        {
            if (p == null || !p.activeInHierarchy) continue;
            if (ignoreDeadPlayers && p.TryGetComponent(out PlayerStats ps) && ps.IsDead()) continue;
            valid.Add(p.transform);
        }

        if (valid.Count == 0) return null;
        if (valid.Count == 1) return valid[0]; // only one player — nothing to switch to

        // pick a random player that isn't the current one, so the focus actually changes.
        // With exactly two players this simply alternates between them.
        List<Transform> others = new List<Transform>();
        foreach (Transform t in valid)
            if (t != player) others.Add(t);

        return others.Count > 0
            ? others[UnityEngine.Random.Range(0, others.Count)]
            : valid[0];
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, heavyRadius);
    }
}