using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class BossMinionAI : MonoBehaviour, IDamageable
{
    enum State { Chase, Attack, Dead }

    [Header("Health")]
    [SerializeField] float maxHealth = 20f;
    [SerializeField] float currentHealth = 20f; // shown in the inspector; drains live at runtime
    [SerializeField] float deathDuration = 2f;  // how long the death clip plays before despawn

    [Header("Target")]
    [SerializeField] Transform player;

    [Header("Targeting")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] float targetSwitchInterval = 30f; // switch to a different player this often (seconds)
    [SerializeField] bool ignoreDeadPlayers = true;    // skip downed players when picking a target

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float damage = 4f;          // deliberately low
    [SerializeField] float hitDelay = 0.3f;
    [SerializeField] float attackDuration = 0.7f;

    [Header("Animation")]
    [SerializeField] float speedDampTime = 0.1f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => state == State.Dead || currentHealth <= 0f;

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
        currentHealth = maxHealth;
    }

    void OnEnable() => state = State.Chase;

    void Update()
    {
        if (state == State.Dead) return;

        UpdateTarget(); // co-op: rotate between active players every targetSwitchInterval
        if (player == null) return;

        switch (state)
        {
            case State.Chase: TickChase(); break;
            case State.Attack: TickAttack(); break;
        }

        float speed01 = agent.speed > 0f ? Mathf.Clamp01(agent.velocity.magnitude / agent.speed) : 0f;
        animator.SetFloat(SpeedHash, speed01, speedDampTime, Time.deltaTime);
    }

    // IDamageable: the player's spells call this when they hit the minion.
    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        if (state == State.Dead) return;
        state = State.Dead;

        // stop moving and lock the agent
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // stop it being hit again while the death anim plays
        if (TryGetComponent(out Collider col)) col.enabled = false;

        animator.SetTrigger(DeathHash);
        Destroy(gameObject, deathDuration); // despawn after the death clip finishes
    }

    // CO-OP TARGETING
    // Rotates focus between active players every targetSwitchInterval seconds; switches
    // immediately if the current target goes inactive or dies. Inactive objects filtered out.
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
        if (valid.Count == 1) return valid[0]; // only one player — nothing to switch to

        // pick a random player that isn't the current one; with two players this alternates
        List<Transform> others = new List<Transform>();
        foreach (Transform t in valid)
            if (t != player) others.Add(t);

        return others.Count > 0
            ? others[Random.Range(0, others.Count)]
            : valid[0];
    }

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
        animator.SetTrigger(AttackHash);
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
}