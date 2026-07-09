using UnityEngine;

public class GravityMoveSpell : SpellBase
{
    [Header("Gravity Move Settings")]
    public float grabRange = 10f;
    public float maxWeight = 50f;
    public float holdDistance = 3f;
    public float throwForce = 20f;
    public float manaPerSecond = 8f;
    public LayerMask grabbableMask;

    [HideInInspector] public bool upgradeLongerRange = false;
    [HideInInspector] public bool upgradeHeavierObjects = false;
    [HideInInspector] public bool upgradeStrongerThrow = false;

    private Rigidbody heldObject = null;     // for regular physics objects
    private BossMinionAI heldMinion = null;  // for networked minions
    private bool isHolding = false;

    public new bool TryCast()
    {
        if (isOnCooldown || isHolding) return false;
        if (playerStats.myMana <= 0f) return false;

        float actualRange = upgradeLongerRange ? grabRange * 1.5f : grabRange;
        float actualMaxWeight = upgradeHeavierObjects ? maxWeight * 2f : maxWeight;

        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));

        if (!Physics.Raycast(ray, out RaycastHit hit, actualRange, grabbableMask))
            return false;

        // MINION grab
        BossMinionAI minion = hit.collider.GetComponentInParent<BossMinionAI>();
        if (minion != null)
        {
            heldMinion = minion;
            heldMinion.SetGravityHold(true);   // server disables its agent
            isHolding = true;
            playerStats.SetCasting(true);
            PlayCastParticles();
            return true;
        }

        // REGULAR physics object grab
        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb == null || rb.mass > actualMaxWeight) return false;

        heldObject = rb;
        heldObject.useGravity = false;
        isHolding = true;
        playerStats.SetCasting(true);
        PlayCastParticles();
        return true;
    }

    public void TryThrow()
    {
        if (!isHolding) return;

        float actualForce = upgradeStrongerThrow ? throwForce * 1.5f : throwForce;
        Vector3 force = Camera.main.transform.forward * actualForce;

        if (heldMinion != null)
        {
            heldMinion.ThrowMinion(force);   // server applies the throw
        }
        else if (heldObject != null)
        {
            heldObject.useGravity = true;
            heldObject.AddForce(force, ForceMode.Impulse);
        }

        ReleaseObject();
        GrantXP();
    }

    public void TryDrop()
    {
        if (!isHolding) return;
        ReleaseObject();
        GrantXP();
    }

    private void ReleaseObject()
    {
        if (heldMinion != null)
            heldMinion.SetGravityHold(false);   // server lets the agent recover

        if (heldObject != null)
            heldObject.useGravity = true;

        heldObject = null;
        heldMinion = null;
        isHolding = false;
        playerStats.SetCasting(false);
        StartCooldown();

        if (castParticles != null) castParticles.Stop();
    }

    protected override void Cast() { }

    protected override void Update()
    {
        base.Update();

        if (!isHolding) return;

        // drain mana while holding
        if (!playerStats.ConsumeMana(manaPerSecond * Time.deltaTime))
        {
            TryDrop();
            return;
        }

        Vector3 targetPos = Camera.main.transform.position
                          + Camera.main.transform.forward * holdDistance;

        if (heldMinion != null)
        {
            // tell the server to move the minion to the hold point
            heldMinion.MoveWhileHeld(targetPos);
        }
        else if (heldObject != null)
        {
            heldObject.MovePosition(
                Vector3.Lerp(heldObject.position, targetPos, Time.deltaTime * 10f));
        }
    }

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeLongerRange    = upgradesBought >= 1;
        upgradeHeavierObjects = upgradesBought >= 2;
        upgradeStrongerThrow  = upgradesBought >= 3;
    }
}