using System.Collections.Generic;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [Header("Score")]
    public int score = 0;

    [Header("Stat Upgrade Costs")]
    public int healthUpgradeCost = 50;
    public int manaUpgradeCost = 50;
    public int enduranceUpgradeCost = 50;

    [Header("Stat Upgrade Amounts")]
    public float healthUpgradeAmount = 25f;
    public float manaUpgradeAmount = 20f;
    public float enduranceUpgradeAmount = 20f;

    [Header("Kit XP")]
    // key: kit name, value: current XP in that kit
    public Dictionary<string, float> kitXP = new Dictionary<string, float>();

    // key: kit name, value: current level (0, 1, 2, 3)
    public Dictionary<string, int> kitLevels = new Dictionary<string, int>();

    public const int maxKitLevel = 3;

    // XP required per level — index 0 = level 0→1, index 1 = level 1→2, index 2 = level 2→3
    public float[] xpPerLevel = { 100f, 250f, 500f };

    [Header("Kit Upgrade Costs (per tier: 1, 2, 3)")]
    public int[] kitUpgradeCosts = { 100, 200, 400 };

    // key: kit name, value: how many upgrades bought (0..3)
    private Dictionary<string, int> kitUpgradesBought = new Dictionary<string, int>();

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    // ─── Score ──────────────────────────────────────────────

    public void AddScore(int amount)
    {
        score += amount;
    }

    public bool SpendScore(int amount)
    {
        if (score < amount) return false;
        score -= amount;
        return true;
    }

    // ─── Stat Upgrades ──────────────────────────────────────

    public bool UpgradeHealth()
    {
        if (!SpendScore(healthUpgradeCost)) return false;
        playerStats.UpgradeMaxHealth(healthUpgradeAmount);
        return true;
    }

    public bool UpgradeMana()
    {
        if (!SpendScore(manaUpgradeCost)) return false;
        playerStats.UpgradeMaxMana(manaUpgradeAmount);
        return true;
    }

    public bool UpgradeEndurance()
    {
        if (!SpendScore(enduranceUpgradeCost)) return false;
        playerStats.UpgradeMaxEndurance(enduranceUpgradeAmount);
        return true;
    }

    // ─── Kit XP & Levels ────────────────────────────────────

    public void RegisterKit(string kitName)
    {
        if (!kitXP.ContainsKey(kitName))
        {
            kitXP[kitName] = 0f;
            kitLevels[kitName] = 0;
        }
    }

    public void AddKitXP(string kitName, float amount)
    {
        if (!kitXP.ContainsKey(kitName)) RegisterKit(kitName);
        if (kitLevels[kitName] >= maxKitLevel) return; // already max level

        kitXP[kitName] += amount;

        // check for level up
        float required = xpPerLevel[kitLevels[kitName]];
        if (kitXP[kitName] >= required)
        {
            kitXP[kitName] -= required;
            kitLevels[kitName]++;
            OnKitLevelUp(kitName, kitLevels[kitName]);
        }
    }

    public int GetKitLevel(string kitName)
    {
        if (!kitLevels.ContainsKey(kitName)) return 0;
        return kitLevels[kitName];
    }

    public float GetKitXPProgress(string kitName)
    {
        if (!kitXP.ContainsKey(kitName)) return 0f;
        if (kitLevels[kitName] >= maxKitLevel) return 1f;

        return kitXP[kitName] / xpPerLevel[kitLevels[kitName]];
    }

    private void OnKitLevelUp(string kitName, int newLevel)
    {
        // spells check GetKitLevel() themselves to unlock upgrades
        Debug.Log($"{kitName} reached level {newLevel}!");
    }

    // ─── Kit Upgrades ───────────────────────────────────────

    public int GetKitUpgradesBought(string kitName)
    {
        return kitUpgradesBought.TryGetValue(kitName, out int v) ? v : 0;
    }

    // can the player buy the NEXT upgrade for this kit right now?
    public bool CanBuyKitUpgrade(string kitName)
    {
        int bought = GetKitUpgradesBought(kitName);
        if (bought >= 3) return false;                          // all bought

        int requiredLevel = bought + 1;                         // upgrade 1 needs level 1, etc.
        if (GetKitLevel(kitName) < requiredLevel) return false; // not high enough level

        int cost = kitUpgradeCosts[bought];
        return score >= cost;                                   // can afford
    }

    // returns true if the purchase succeeded
    public bool BuyKitUpgrade(string kitName)
    {
        if (!CanBuyKitUpgrade(kitName)) return false;

        int bought = GetKitUpgradesBought(kitName);
        int cost = kitUpgradeCosts[bought];

        if (!SpendScore(cost)) return false;

        kitUpgradesBought[kitName] = bought + 1;
        Debug.Log($"Bought upgrade {bought + 1} for {kitName} (cost {cost}).");
        return true;
    }

    // cost of the NEXT upgrade (for UI display); -1 if none left
    public int GetNextKitUpgradeCost(string kitName)
    {
        int bought = GetKitUpgradesBought(kitName);
        if (bought >= 3) return -1;
        return kitUpgradeCosts[bought];
    }
}