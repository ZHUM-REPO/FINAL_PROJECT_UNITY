using UnityEngine;

public class GravityMoveSpell : SpellBase
{
    [Header("Gravity Move Settings")]
    public float grabRange = 10f;
    public float maxWeight = 50f;       // Rigidbody mass limit
    public float holdDistance = 3f;     // how far in front of player object floats
    public float throwForce = 20f;
    public float manaPerSecond = 8f;
    public LayerMask grabbableMask;

    [HideInInspector] public bool upgradeLongerRange = false;       // level 1
    [HideInInspector] public bool upgradeHeavierObjects = false;    // level 2
    [HideInInspector] public bool upgradeStrongerThrow = false;     // level 3

    private Rigidbody heldObject = null;
    private bool isHolding = false;

    // TryCast starts the grab, TryThrow throws, TryDrop drops
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
        if (!isHolding || heldObject == null) return;

        heldObject.useGravity = true;
        float actualForce = upgradeStrongerThrow ? throwForce * 1.5f : throwForce;
        heldObject.AddForce(Camera.main.transform.forward * actualForce,
                            ForceMode.Impulse);
        ReleaseObject();
        GrantXP();
    }

    public void TryDrop()
    {
        if (!isHolding || heldObject == null) return;
        ReleaseObject();
        GrantXP();
    }

    private void ReleaseObject()
    {
        if (heldObject != null)
            heldObject.useGravity = true;

        heldObject = null;
        isHolding = false;
        playerStats.SetCasting(false);
        StartCooldown();

        if (castParticles != null) castParticles.Stop();
    }

    protected override void Cast() { } // not used — uses TryCast/TryThrow/TryDrop

    protected override void Update()
    {
        base.Update();

        if (!isHolding || heldObject == null) return;

        // drain mana while holding
        if (!playerStats.ConsumeMana(manaPerSecond * Time.deltaTime))
        {
            TryDrop(); // out of mana — drop it
            return;
        }

        // float object in front of camera at holdDistance
        Vector3 targetPos = Camera.main.transform.position
                          + Camera.main.transform.forward * holdDistance;
        heldObject.MovePosition(
            Vector3.Lerp(heldObject.position, targetPos, Time.deltaTime * 10f));
    }

    public override void ApplyUpgradeLevel(int upgradesBought)
    {
        upgradeLongerRange    = upgradesBought >= 1;
        upgradeHeavierObjects = upgradesBought >= 2;
        upgradeStrongerThrow  = upgradesBought >= 3;
    }
}