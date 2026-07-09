using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerProgression : NetworkBehaviour
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

    public Dictionary<string, float> kitXP = new Dictionary<string, float>();
    public Dictionary<string, int> kitLevels = new Dictionary<string, int>();

    public const int maxKitLevel = 3;
    public float[] xpPerLevel = { 100f, 250f, 500f };

    [Header("Kit Upgrade Costs (per tier: 1, 2, 3)")]
    public int[] kitUpgradeCosts = { 100, 200, 400 };

    private Dictionary<string, int> kitUpgradesBought = new Dictionary<string, int>();

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        LoadFromStore();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        SaveToStore();
    }

    // ─── Persistence ────────────────────────────────────────

    private void LoadFromStore()
    {
        if (ProgressionStore.Instance == null) return;

        ProgressionData data = ProgressionStore.Instance.GetData(OwnerClientId);

        score = data.score;
        kitXP = new Dictionary<string, float>(data.kitXP);
        kitLevels = new Dictionary<string, int>(data.kitLevels);
        kitUpgradesBought = new Dictionary<string, int>(data.kitUpgradesBought);

        // re-apply persisted stat upgrades to this fresh player
        if (playerStats != null)
        {
            playerStats.maxHealth += data.bonusMaxHealth;
            playerStats.maxMana += data.bonusMaxMana;
            playerStats.maxEndurance += data.bonusMaxEndurance;
            playerStats.myHealth = playerStats.maxHealth;
            playerStats.myMana = playerStats.maxMana;
            playerStats.myEndurance = playerStats.maxEndurance;
        }
    }

    private void SaveToStore()
    {
        if (ProgressionStore.Instance == null) return;

        ProgressionData data = ProgressionStore.Instance.GetData(OwnerClientId);

        data.score = score;
        data.kitXP = new Dictionary<string, float>(kitXP);
        data.kitLevels = new Dictionary<string, int>(kitLevels);
        data.kitUpgradesBought = new Dictionary<string, int>(kitUpgradesBought);

        ProgressionStore.Instance.SaveData(OwnerClientId, data);
    }

    // ─── Score ──────────────────────────────────────────────

    public void AddScore(int amount)
    {
        score += amount;
        SaveToStore();   // persist immediately so rewards aren't lost
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
        RecordStatBonus(healthUpgradeAmount, 0f, 0f);
        SaveToStore();
        return true;
    }

    public bool UpgradeMana()
    {
        if (!SpendScore(manaUpgradeCost)) return false;
        playerStats.UpgradeMaxMana(manaUpgradeAmount);
        RecordStatBonus(0f, manaUpgradeAmount, 0f);
        SaveToStore();
        return true;
    }

    public bool UpgradeEndurance()
    {
        if (!SpendScore(enduranceUpgradeCost)) return false;
        playerStats.UpgradeMaxEndurance(enduranceUpgradeAmount);
        RecordStatBonus(0f, 0f, enduranceUpgradeAmount);
        SaveToStore();
        return true;
    }

    // remember stat upgrades so they persist across scenes
    private void RecordStatBonus(float hp, float mp, float end)
    {
        if (ProgressionStore.Instance == null) return;
        ProgressionData data = ProgressionStore.Instance.GetData(OwnerClientId);
        data.bonusMaxHealth += hp;
        data.bonusMaxMana += mp;
        data.bonusMaxEndurance += end;
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
        if (kitLevels[kitName] >= maxKitLevel) return;

        kitXP[kitName] += amount;

        float required = xpPerLevel[kitLevels[kitName]];
        if (kitXP[kitName] >= required)
        {
            kitXP[kitName] -= required;
            kitLevels[kitName]++;
            Debug.Log($"{kitName} reached level {kitLevels[kitName]}!");
        }

        SaveToStore();
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

    // ─── Kit Upgrades ───────────────────────────────────────

    public int GetKitUpgradesBought(string kitName)
    {
        return kitUpgradesBought.TryGetValue(kitName, out int v) ? v : 0;
    }

    public bool CanBuyKitUpgrade(string kitName)
    {
        int bought = GetKitUpgradesBought(kitName);
        if (bought >= 3) return false;

        int requiredLevel = bought + 1;
        if (GetKitLevel(kitName) < requiredLevel) return false;

        int cost = kitUpgradeCosts[bought];
        return score >= cost;
    }

    public bool BuyKitUpgrade(string kitName)
    {
        if (!CanBuyKitUpgrade(kitName)) return false;

        int bought = GetKitUpgradesBought(kitName);
        int cost = kitUpgradeCosts[bought];

        if (!SpendScore(cost)) return false;

        kitUpgradesBought[kitName] = bought + 1;
        SaveToStore();
        Debug.Log($"Bought upgrade {bought + 1} for {kitName} (cost {cost}).");
        return true;
    }

    public int GetNextKitUpgradeCost(string kitName)
    {
        int bought = GetKitUpgradesBought(kitName);
        if (bought >= 3) return -1;
        return kitUpgradeCosts[bought];
    }
}