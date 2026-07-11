using UnityEngine;

public class HealSpell : SpellBase
{
    [Header("Heal Settings")]
    public float healAmount = 40f;
    public int qteSequenceLength = 4;

    [HideInInspector] public bool upgradeIncreasedHeal = false;     // level 1
    [HideInInspector] public bool upgradeHealOverTime = false;      // level 2
    [HideInInspector] public bool upgradeAuraHeal = false;          // level 3 — small heal to nearby teammates

    protected override void Cast()
    {
        if (QTEManager.Instance == null) return;

        playerStats.SetCasting(true);

        QTEManager.Instance.StartQTE(OnQTEComplete, qteSequenceLength);
    }

    private void OnQTEComplete(QTEResult result)
    {
        playerStats.SetCasting(false);

        if (result.grade == QTEGrade.Fail)
        {
            Debug.Log("Heal failed — QTE failed.");
            return;
        }

        float actual = upgradeIncreasedHeal ? healAmount * 1.5f : healAmount;
        float finalHeal = actual * result.effectMultiplier;

        playerStats.Heal(finalHeal);
        PlayCastParticles();

        Debug.Log($"Healed self for {finalHeal} ({result.grade})");

        if (upgradeHealOverTime)
            StartCoroutine(HealOverTime(finalHeal * 0.5f, 5f));

        if (upgradeAuraHeal)
            ApplyAuraHeal(finalHeal * 0.3f);
    }

    private System.Collections.IEnumerator HealOverTime(float total, float duration)
    {
        float healPerSecond = total / duration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            playerStats.Heal(healPerSecond * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ApplyAuraHeal(float amount)
    {
        // finds nearby teammates in a radius and heals them
        Collider[] hits = Physics.OverlapSphere(transform.position, 6f);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            PlayerStats teammate = hit.GetComponent<PlayerStats>();
            if (teammate != null)
                teammate.Heal(amount);
        }
    }
}