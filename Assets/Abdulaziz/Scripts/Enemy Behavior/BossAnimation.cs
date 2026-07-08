using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossAnimation : MonoBehaviour
{
    [SerializeField] BossAI boss;
    [SerializeField] float speedDampTime = 0.12f;

    Animator animator;
    float targetSpeed;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int StandUpHash = Animator.StringToHash("StandUp");
    static readonly int NormalAttackHash = Animator.StringToHash("NormalAttack");
    static readonly int HeavyAttackHash = Animator.StringToHash("HeavyAttack");
    static readonly int DeathHash = Animator.StringToHash("Death");

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (boss == null) boss = GetComponentInParent<BossAI>();
    }

    void OnEnable()
    {
        boss.StoodUp += OnStoodUp;
        boss.SpeedChanged += OnSpeedChanged;
        boss.NormalAttacked += OnNormalAttacked;
        boss.HeavyAttacked += OnHeavyAttacked;
        boss.Died += OnDied;
    }

    void OnDisable()
    {
        boss.StoodUp -= OnStoodUp;
        boss.SpeedChanged -= OnSpeedChanged;
        boss.NormalAttacked -= OnNormalAttacked;
        boss.HeavyAttacked -= OnHeavyAttacked;
        boss.Died -= OnDied;
    }

    void Update() => animator.SetFloat(SpeedHash, targetSpeed, speedDampTime, Time.deltaTime);

    void OnStoodUp() => animator.SetTrigger(StandUpHash);
    void OnSpeedChanged(float speed01) => targetSpeed = speed01;
    void OnNormalAttacked() => animator.SetTrigger(NormalAttackHash);
    void OnHeavyAttacked() => animator.SetTrigger(HeavyAttackHash);
    void OnDied() => animator.SetTrigger(DeathHash);
}