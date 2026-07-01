using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BossAI boss;
    [SerializeField] Damage damageable;

    [Header("UI")]
    [SerializeField] GameObject container;  // whole bar; hidden until the fight starts
    [SerializeField] Image fillImage;       // Image set to Filled / Horizontal
    [SerializeField] Text nameLabel;        // optional (swap for TMP_Text if you use TextMeshPro)
    [SerializeField] string bossName = "Boss";

    [Header("Phase 2")]
    [SerializeField] bool tintOnPhaseTwo = true;
    [SerializeField] Color phaseTwoColor = new Color(0.7f, 0.1f, 0.1f);

    [Header("Fill")]
    [SerializeField] float fillLerpSpeed = 8f; // smooth drain; set 0 to snap instantly
    [SerializeField] float hideDelayAfterDeath = 1f;

    float targetFill = 1f;

    void OnEnable()
    {
        if (boss != null)
        {
            boss.FightStarted += OnFightStarted;
            boss.PhaseTwoStarted += OnPhaseTwoStarted;
            boss.Died += OnDied;
        }
        if (damageable != null) damageable.HealthChanged += OnHealthChanged;

        if (container != null) container.SetActive(false); // stay hidden through the intro
    }

    void OnDisable()
    {
        if (boss != null)
        {
            boss.FightStarted -= OnFightStarted;
            boss.PhaseTwoStarted -= OnPhaseTwoStarted;
            boss.Died -= OnDied;
        }
        if (damageable != null) damageable.HealthChanged -= OnHealthChanged;
    }

    void Update()
    {
        if (fillImage == null) return;
        fillImage.fillAmount = fillLerpSpeed > 0f
            ? Mathf.MoveTowards(fillImage.fillAmount, targetFill, fillLerpSpeed * Time.deltaTime)
            : targetFill;
    }

    void OnFightStarted()
    {
        targetFill = 1f;
        if (fillImage != null) fillImage.fillAmount = 1f;
        if (nameLabel != null) nameLabel.text = bossName;
        if (container != null) container.SetActive(true);
    }

    void OnHealthChanged(float current)
    {
        if (damageable == null || damageable.MaxHealth <= 0f) return;
        targetFill = Mathf.Clamp01(current / damageable.MaxHealth);
    }

    void OnPhaseTwoStarted()
    {
        if (tintOnPhaseTwo && fillImage != null) fillImage.color = phaseTwoColor;
    }

    void OnDied()
    {
        targetFill = 0f;
        if (container != null) Invoke(nameof(HideContainer), hideDelayAfterDeath);
    }

    void HideContainer()
    {
        if (container != null) container.SetActive(false);
    }
}