using UnityEngine;

public class KitManager : MonoBehaviour
{
    [Header("Equipped Kit")]
    public KitDefinition equippedKit;

    [Header("Spell References — assigned at runtime")]
    private SpellBase spellOne;
    private SpellBase spellTwo;
    private int activeSpellIndex = 0; // 0 = spell one, 1 = spell two

    [Header("References")]
    public Transform spellSpawnPoint; // drag SpawnPoint here in Inspector

    // held spell tracking
    private FireBreathSpell fireBreath;
    private GravityMoveSpell gravityMove;
    private bool isHoldingCast = false;

    private void OnEnable()
    {
        PlayerInputs.OnCastSpellInput += HandleCastSpell;
        PlayerInputs.OnCastSpellCanceled += HandleCastSpellCanceled;
        PlayerInputs.OnChangeSpellInput += HandleChangeSpell;
        PlayerInputs.OnChangeKitInput += HandleChangeKit;
        PlayerInputs.OnThrowInput += HandleThrow;
        PlayerInputs.OnDropInput += HandleDrop;
    }

    private void OnDisable()
    {
        PlayerInputs.OnCastSpellInput -= HandleCastSpell;
        PlayerInputs.OnCastSpellCanceled -= HandleCastSpellCanceled;
        PlayerInputs.OnChangeSpellInput -= HandleChangeSpell;
        PlayerInputs.OnChangeKitInput -= HandleChangeKit;
        PlayerInputs.OnThrowInput -= HandleThrow;
        PlayerInputs.OnDropInput -= HandleDrop;
    }

    private void Start()
    {
        if (equippedKit != null)
            EquipKit(equippedKit);
    }

    // ─── Kit Equipping ────────────────────────────────────

    public void EquipKit(KitDefinition kit)
    {
        // destroy old spells if any
        if (spellOne != null) Destroy(spellOne.gameObject);
        if (spellTwo != null) Destroy(spellTwo.gameObject);

        equippedKit = kit;
        activeSpellIndex = 0;
        fireBreath = null;
        gravityMove = null;

        // spawn spell one
        if (kit.spellOnePrefab != null)
        {
            GameObject s1 = Instantiate(kit.spellOnePrefab, transform);
            spellOne = s1.GetComponent<SpellBase>();
            spellOne.SetKitName(kit.kitName);
            CacheHeldSpells(spellOne);

            // pass spawn point to spell
            AssignSpawnPoint(spellOne);
        }

        // spawn spell two
        if (kit.spellTwoPrefab != null)
        {
            GameObject s2 = Instantiate(kit.spellTwoPrefab, transform);
            spellTwo = s2.GetComponent<SpellBase>();
            spellTwo.SetKitName(kit.kitName);
            CacheHeldSpells(spellTwo);

            // pass spawn point to spell
            AssignSpawnPoint(spellTwo);
        }

        Debug.Log($"Kit equipped: {kit.kitName}");
    }

    // Pass the spawn point to the next spell

    private void AssignSpawnPoint(SpellBase spell)
    {
        if (spellSpawnPoint == null) return;

        // assign to fireball
        FireballSpell fireball = spell as FireballSpell;
        if (fireball != null) fireball.spawnPoint = spellSpawnPoint;

        // assign to fire breath
        FireBreathSpell breath = spell as FireBreathSpell;
        if (breath != null) breath.spawnPoint = spellSpawnPoint;

        // assign to freeze
        FreezeSpell freeze = spell as FreezeSpell;
        if (freeze != null) freeze.spawnPoint = spellSpawnPoint;

        // assign to ice wall — uses Camera.main so no spawnPoint needed

        // assign to gravity move — uses Camera.main so no spawnPoint needed
    }

    // cache held spell references so we can call their special methods
    private void CacheHeldSpells(SpellBase spell)
    {
        if (spell is FireBreathSpell fb) fireBreath = fb;
        if (spell is GravityMoveSpell gm) gravityMove = gm;
    }

    // ─── Active Spell ─────────────────────────────────────

    private SpellBase GetActiveSpell()
    {
        return activeSpellIndex == 0 ? spellOne : spellTwo;
    }

    // ─── Input Handlers ───────────────────────────────────

    private void HandleCastSpell()
    {
        SpellBase active = GetActiveSpell();
        if (active == null) return;

        isHoldingCast = true;

        // held spells use their own TryCast
        if (active is FireBreathSpell fb)
        {
            fb.TryCast();
            return;
        }

        if (active is GravityMoveSpell gm)
        {
            gm.TryCast();
            return;
        }

        // all other spells use base TryCast
        active.TryCast();
    }

    private void HandleCastSpellCanceled()
    {
        isHoldingCast = false;

        SpellBase active = GetActiveSpell();
        if (active == null) return;

        // stop held spells on release
        if (active is FireBreathSpell fb)
        {
            fb.TryStop();
            return;
        }

        // gravity move doesn't stop on cast release
        // it stops on throw or drop input instead
    }

    private void HandleChangeSpell()
    {
        // stop any held spell before switching
        StopHeldSpells();

        activeSpellIndex = activeSpellIndex == 0 ? 1 : 0;

        SpellBase active = GetActiveSpell();
        string spellName = active != null ? active.spellName : "None";
        Debug.Log($"Switched to spell: {spellName}");
    }

    private void HandleChangeKit()
    {
        // for now just logs — full kit selection UI comes in office hub phase
        Debug.Log("Change kit requested — will open kit selector in lobby.");
    }

    private void HandleThrow()
    {
        if (gravityMove != null)
            gravityMove.TryThrow();
    }

    private void HandleDrop()
    {
        if (gravityMove != null)
            gravityMove.TryDrop();
    }

    private void StopHeldSpells()
    {
        SpellBase active = GetActiveSpell();
        if (active is FireBreathSpell fb) fb.TryStop();
        if (active is GravityMoveSpell gm) gm.TryDrop();
    }

    // ─── Public API (for lobby kit selection later) ───────

    public string GetActiveSpellName()
    {
        SpellBase active = GetActiveSpell();
        return active != null ? active.spellName : "None";
    }

    public float GetActiveSpellCooldown()
    {
        SpellBase active = GetActiveSpell();
        return active != null ? active.GetCooldownProgress() : 0f;
    }

    public int GetActiveSpellIndex() => activeSpellIndex;
}