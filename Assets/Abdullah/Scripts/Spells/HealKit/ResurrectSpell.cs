using UnityEngine;

public class ResurrectSpell : SpellBase
{
    [Header("Resurrect Settings")]
    public float resurrectHealthPercent = 0.30f; // revived with 30% health
    public float maxResurrectDistance = 5f;
    public LayerMask deadPlayerMask;

    [HideInInspector] public bool upgradeMoreHealth = false;        // level 1
    [HideInInspector] public bool upgradeFasterQTE = false;         // level 2 — fewer buttons
    [HideInInspector] public bool upgradeFullRevive = false;        // level 3

    private PlayerStats targetStats;
    private PlayerDeath targetDeath;

    protected override void Cast()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));

        if (!Physics.Raycast(ray, out RaycastHit hit,
                             maxResurrectDistance, deadPlayerMask))
        {
            Debug.Log("No dead teammate nearby to resurrect.");
            return;
        }

        targetDeath = hit.collider.GetComponent<PlayerDeath>();
        if (targetDeath == null || !targetDeath.IsDead())
        {
            Debug.Log("Target is not dead.");
            return;
        }

        targetStats = hit.collider.GetComponent<PlayerStats>();
        playerStats.SetCasting(true);

        // resurrect QTE is harder — more buttons, less time per input
        // upgradeFasterQTE reduces the sequence length as a reward
        int qteLength = upgradeFasterQTE ? 5 : 8;
        QTEManager.Instance.StartQTE(OnQTEComplete, qteLength);
    }

    private void OnQTEComplete(QTEResult result)
    {
        playerStats.SetCasting(false);

        if (result.grade == QTEGrade.Fail || targetStats == null)
        {
            Debug.Log("Resurrection failed.");
            return;
        }

        float healthPercent;

        if (upgradeFullRevive)
            healthPercent = 1.0f;
        else if (upgradeMoreHealth)
            healthPercent = Mathf.Lerp(0.20f, 0.70f, result.effectMultiplier);
        else
            healthPercent = Mathf.Lerp(0.10f, resurrectHealthPercent,
                                        result.effectMultiplier);

        targetDeath.Resurrect(healthPercent);
        PlayCastParticles();

        Debug.Log($"Resurrected teammate with {healthPercent * 100f}% health ({result.grade})");
    }
}