using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KitManager : MonoBehaviour
{
    [Header("Equipped Kit")]
    public KitDefinition equippedKit;

    [Header("Multi-Kit Support")]
    private List<KitDefinition> assignedKits = new List<KitDefinition>();
    private int activeKitIndex = 0;

    [Header("Spell References — assigned at runtime")]
    private SpellBase spellOne;
    private SpellBase spellTwo;
    private int activeSpellIndex = 0;

    [Header("References")]
    public Transform spellSpawnPoint;

    [Header("Solo Test — remove before multiplayer")]
    public bool soloTestMode = true;
    public KitDefinition[] allKits;

    // held spell tracking
    private FireBreathSpell fireBreath;
    private GravityMoveSpell gravityMove;
    private bool isHoldingCast = false;

    private NetworkObject networkObject;

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

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
        if (spellSpawnPoint == null)
            Debug.LogWarning("SpellSpawnPoint is not assigned in KitManager!");

        if (soloTestMode && allKits != null && allKits.Length > 0)
        {
            SetAssignedKits(new List<KitDefinition>(allKits));
            Debug.Log("Solo test mode — all kits assigned.");
        }
        else if (equippedKit != null)
        {
            EquipKit(equippedKit);
        }
    }

    // ─── Kit Equipping ────────────────────────────────────

    public void EquipKit(KitDefinition kit)
    {
        if (spellOne != null) Destroy(spellOne.gameObject);
        if (spellTwo != null) Destroy(spellTwo.gameObject);

        equippedKit = kit;
        activeSpellIndex = 0;
        fireBreath = null;
        gravityMove = null;

        if (kit.spellOnePrefab != null)
        {
            GameObject s1 = Instantiate(kit.spellOnePrefab, transform);
            spellOne = s1.GetComponent<SpellBase>();
            spellOne.SetKitName(kit.kitName);
            CacheHeldSpells(spellOne);
            AssignSpawnPoint(spellOne);
        }

        if (kit.spellTwoPrefab != null)
        {
            GameObject s2 = Instantiate(kit.spellTwoPrefab, transform);
            spellTwo = s2.GetComponent<SpellBase>();
            spellTwo.SetKitName(kit.kitName);
            CacheHeldSpells(spellTwo);
            AssignSpawnPoint(spellTwo);
        }

        Debug.Log($"Kit equipped: {kit.kitName}");
    }

    private void AssignSpawnPoint(SpellBase spell)
    {
        if (spellSpawnPoint == null) return;

        FireballSpell fireball = spell as FireballSpell;
        if (fireball != null) fireball.spawnPoint = spellSpawnPoint;

        FireBreathSpell breath = spell as FireBreathSpell;
        if (breath != null) breath.spawnPoint = spellSpawnPoint;

        FreezeSpell freeze = spell as FreezeSpell;
        if (freeze != null) freeze.spawnPoint = spellSpawnPoint;
    }

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
        if (networkObject != null && !networkObject.IsOwner) return;
        if (OfficeComputerUI.IsAnyComputerOpen) return;

        SpellBase active = GetActiveSpell();
        if (active == null) return;

        isHoldingCast = true;

        if (active is FireBreathSpell fb) { fb.TryCast(); return; }
        if (active is GravityMoveSpell gm) { gm.TryCast(); return; }

        active.TryCast();
    }

    private void HandleCastSpellCanceled()
    {
        if (networkObject != null && !networkObject.IsOwner) return;
        if (OfficeComputerUI.IsAnyComputerOpen) return;

        isHoldingCast = false;

        SpellBase active = GetActiveSpell();
        if (active == null) return;

        if (active is FireBreathSpell fb) fb.TryStop();
    }

    private void HandleChangeSpell()
    {
        if (networkObject != null && !networkObject.IsOwner) return;
        if (OfficeComputerUI.IsAnyComputerOpen) return;

        StopHeldSpells();
        activeSpellIndex = activeSpellIndex == 0 ? 1 : 0;

        SpellBase active = GetActiveSpell();
        string spellName = active != null ? active.spellName : "None";
        Debug.Log($"Switched to spell: {spellName}");
    }

    public void SetAssignedKits(List<KitDefinition> kits)
    {
        assignedKits = kits;
        if (assignedKits.Count > 0)
            EquipKit(assignedKits[0]);
    }

    private void HandleChangeKit()
    {
        if (networkObject != null && !networkObject.IsOwner) return;

        if (assignedKits.Count <= 1) return;

        StopHeldSpells();
        activeKitIndex = (activeKitIndex + 1) % assignedKits.Count;
        EquipKit(assignedKits[activeKitIndex]);
        Debug.Log($"Switched to kit: {assignedKits[activeKitIndex].kitName}");
    }

    private void HandleThrow()
    {
        if (networkObject != null && !networkObject.IsOwner) return;
        if (OfficeComputerUI.IsAnyComputerOpen) return;
        if (gravityMove != null) gravityMove.TryThrow();
    }

    private void HandleDrop()
    {
        if (networkObject != null && !networkObject.IsOwner) return;
        if (OfficeComputerUI.IsAnyComputerOpen) return;
        if (gravityMove != null) gravityMove.TryDrop();
    }

    private void StopHeldSpells()
    {
        SpellBase active = GetActiveSpell();
        if (active is FireBreathSpell fb) fb.TryStop();
        if (active is GravityMoveSpell gm) gm.TryDrop();
    }

    // ─── Public API ───────────────────────────────────────

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

    public void AddKit(KitDefinition kit)
    {
        if (networkObject != null && !networkObject.IsOwner) return;
        if (assignedKits.Contains(kit)) return;

        assignedKits.Add(kit);

        // equip it right away if it's the only kit
        if (assignedKits.Count == 1)
        {
            activeKitIndex = 0;
            EquipKit(assignedKits[0]);
        }

        Debug.Log($"Kit added: {kit.kitName}");
    }

    public void RemoveKit(KitDefinition kit)
    {
        if (networkObject != null && !networkObject.IsOwner) return;
        if (!assignedKits.Contains(kit)) return;

        assignedKits.Remove(kit);

        // if we removed the equipped kit, equip another or clear
        if (assignedKits.Count > 0)
        {
            activeKitIndex = 0;
            EquipKit(assignedKits[0]);
        }
        else
        {
            if (spellOne != null) Destroy(spellOne.gameObject);
            if (spellTwo != null) Destroy(spellTwo.gameObject);
            spellOne = null;
            spellTwo = null;
            equippedKit = null;
        }

        Debug.Log($"Kit removed: {kit.kitName}");
    }
}