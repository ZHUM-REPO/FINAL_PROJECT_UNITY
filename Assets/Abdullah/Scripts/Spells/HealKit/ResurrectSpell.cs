using UnityEngine;

public class ResurrectSpell : SpellBase
{
    [Header("Resurrect Settings")]
    public float resurrectHealthPercent = 0.30f;
    public float maxResurrectDistance = 5f;
    public LayerMask deadPlayerMask;

    [HideInInspector] public bool upgradeMoreHealth = false;
    [HideInInspector] public bool upgradeFasterQTE = false;
    [HideInInspector] public bool upgradeFullRevive = false;

    private PlayerStats targetStats;
    private PlayerDeath targetDeath;

    protected override void Cast()
    {
        // a dead caster can't resurrect
        PlayerDeath myDeath = GetComponentInParent<PlayerDeath>();
        if (myDeath != null && myDeath.IsDead()) return;

        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));

        if (!Physics.Raycast(ray, out RaycastHit hit,
                             maxResurrectDistance, deadPlayerMask))
        {
            Debug.Log("No dead teammate nearby to resurrect.");
            return;
        }

        targetDeath = hit.collider.GetComponentInParent<PlayerDeath>();
        if (targetDeath == null || !targetDeath.IsDead())
        {
            Debug.Log("Target is not dead.");
            return;
        }

        targetStats = hit.collider.GetComponentInParent<PlayerStats>();
        playerStats.SetCasting(true);

        int qteLength = upgradeFasterQTE ? 5 : 8;
        QTEManager.Instance.StartQTE(OnQTEComplete, qteLength);
    }

    private void OnQTEComplete(QTEResult result)
    {
        playerStats.SetCasting(false);

        if (result.grade == QTEGrade.Fail || targetDeath == null)
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

        // routes through the server now (PlayerDeath.Resurrect sends a ServerRpc)
        targetDeath.Resurrect(healthPercent);
        PlayCastParticles();

        Debug.Log($"Resurrected teammate with {healthPercent * 100f}% health ({result.grade})");
    }

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeMoreHealth = upgradesBought >= 1;
        upgradeFasterQTE  = upgradesBought >= 2;
        upgradeFullRevive = upgradesBought >= 3;
    }
}