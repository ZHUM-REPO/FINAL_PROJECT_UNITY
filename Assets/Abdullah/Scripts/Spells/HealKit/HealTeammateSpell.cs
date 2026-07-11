using UnityEngine;

public class HealTeammateSpell : SpellBase
{
    [Header("Heal Teammate Settings")]
    public float healAmount = 50f;
    public float maxTargetDistance = 15f;
    public int qteSequenceLength = 4;
    public LayerMask playerMask;
    public ParticleSystem healBeamParticles;

    [HideInInspector] public bool upgradeIncreasedHeal = false;     // level 1
    [HideInInspector] public bool upgradeLongerRange = false;       // level 2
    [HideInInspector] public bool upgradeChainHeal = false;         // level 3

    private PlayerStats targetStats;

    protected override void Cast()
    {
        // raycast to find teammate
        float actualRange = upgradeLongerRange ? maxTargetDistance * 1.5f : maxTargetDistance;
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));

        if (!Physics.Raycast(ray, out RaycastHit hit, actualRange, playerMask))
        {
            Debug.Log("No teammate in sight to heal.");
            return;
        }

        targetStats = hit.collider.GetComponent<PlayerStats>();
        if (targetStats == null || targetStats == playerStats)
        {
            Debug.Log("Invalid heal target.");
            return;
        }

        playerStats.SetCasting(true);
        QTEManager.Instance.StartQTE(OnQTEComplete, qteSequenceLength);
    }

    private void OnQTEComplete(QTEResult result)
    {
        playerStats.SetCasting(false);

        if (result.grade == QTEGrade.Fail || targetStats == null)
        {
            Debug.Log("Teammate heal failed.");
            return;
        }

        float actual = upgradeIncreasedHeal ? healAmount * 1.5f : healAmount;
        float finalHeal = actual * result.effectMultiplier;

        targetStats.Heal(finalHeal);
        PlayCastParticles();

        if (healBeamParticles != null)
            Instantiate(healBeamParticles,
                        targetStats.transform.position,
                        Quaternion.identity);

        Debug.Log($"Healed teammate for {finalHeal} ({result.grade})");

        // chain heal — bounces to one more nearby teammate
        if (upgradeChainHeal)
            ApplyChainHeal(finalHeal * 0.4f, targetStats);
    }

    private void ApplyChainHeal(float amount, PlayerStats alreadyHealed)
    {
        Collider[] hits = Physics.OverlapSphere(
            alreadyHealed.transform.position, 8f, playerMask);

        foreach (Collider hit in hits)
        {
            PlayerStats next = hit.GetComponent<PlayerStats>();
            if (next == null || next == playerStats || next == alreadyHealed) continue;

            next.Heal(amount);
            Debug.Log($"Chain healed {hit.name} for {amount}");
            break; // only one chain target
        }
    }
}