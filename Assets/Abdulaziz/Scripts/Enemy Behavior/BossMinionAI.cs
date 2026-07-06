using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class BossMinionAI : MonoBehaviour
{
    enum State { Chase, Attack }

    [Header("Target")]
    [SerializeField] Transform player;

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float damage = 4f;          // deliberately low
    [SerializeField] float hitDelay = 0.3f;
    [SerializeField] float attackDuration = 0.7f;

    [Header("Animation")]
    [SerializeField] float speedDampTime = 0.1f;

    NavMeshAgent agent;
    Animator animator;
    State state;
    float attackTimer;
    bool hitApplied;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    void OnEnable() => state = State.Chase;

    void Update()
    {
        if (player == null) return;

        switch (state)
        {
            case State.Chase: TickChase(); break;
            case State.Attack: TickAttack(); break;
        }

        float speed01 = agent.speed > 0f ? Mathf.Clamp01(agent.velocity.magnitude / agent.speed) : 0f;
        animator.SetFloat(SpeedHash, speed01, speedDampTime, Time.deltaTime);
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
            if (DistanceToPlayer() <= attackRange && player.TryGetComponent(out IDamageable target))
                target.TakeDamage(damage);
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