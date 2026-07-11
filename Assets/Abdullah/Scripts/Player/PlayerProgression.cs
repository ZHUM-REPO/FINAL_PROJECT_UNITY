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

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    // ─── Score ────────────────────────────────────────────

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

    // ─── Stat Upgrades ────────────────────────────────────

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

    // ─── Kit XP & Levels ─────────────────────────────────

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
}