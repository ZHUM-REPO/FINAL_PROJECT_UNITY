using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class KitUpgradesUI : MonoBehaviour
{
    [Header("Kit Selector Buttons (order: Fire, Ice, Heal, Gravity)")]
    public Button[] kitSelectButtons;
    public string[] kitNames = { "Fire", "Ice", "Heal", "Gravity" };

    [Header("Selected Kit Header")]
    public TextMeshProUGUI selectedKitNameText;
    public TextMeshProUGUI selectedKitLevelText;

    [Header("Upgrade Slots (3, in tier order)")]
    public UpgradeSlot[] upgradeSlots;   // size 3

    [Header("Buy")]
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    [Header("Score")]
    public TextMeshProUGUI scoreText;

    private PlayerProgression progression;
    private KitManager kitManager;
    private KitDefinition[] allKits;      // pulled from KitSelectionManager
    private int selectedKitIndex = 0;
    private bool wired = false;

    private void Update()
    {
        if (progression == null || kitManager == null)
        {
            FindLocalPlayer();
            return;
        }

        if (!wired) WireButtons();
        Refresh();
    }

    private void FindLocalPlayer()
    {
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsListening) return;

        foreach (PlayerProgression pp in FindObjectsByType<PlayerProgression>(FindObjectsSortMode.None))
        {
            NetworkObject no = pp.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner)
            {
                progression = pp;
                kitManager = pp.GetComponent<KitManager>();
                break;
            }
        }

        // grab the kit definitions from the selection manager (for descriptions)
        if (KitSelectionManager.Instance != null)
            allKits = KitSelectionManager.Instance.allKits;
    }

    private void WireButtons()
    {
        for (int i = 0; i < kitSelectButtons.Length; i++)
        {
            int index = i;
            kitSelectButtons[i].onClick.AddListener(() => SelectKit(index));
        }

        if (buyButton != null)
            buyButton.onClick.AddListener(BuySelectedUpgrade);

        wired = true;
    }

    private void SelectKit(int index)
    {
        selectedKitIndex = index;
        Refresh();
    }

    private void BuySelectedUpgrade()
    {
        string kit = kitNames[selectedKitIndex];

        if (progression.BuyKitUpgrade(kit))
        {
            // if this is the kit we currently have equipped, apply it live
            if (kitManager.equippedKit != null && kitManager.equippedKit.kitName == kit)
                kitManager.ReapplyUpgrades();
        }

        Refresh();
    }

    private void Refresh()
    {
        if (progression == null) return;

        string kit = kitNames[selectedKitIndex];
        int level = progression.GetKitLevel(kit);
        int bought = progression.GetKitUpgradesBought(kit);

        // header
        if (selectedKitNameText != null)
            selectedKitNameText.text = kit;
        if (selectedKitLevelText != null)
            selectedKitLevelText.text = level >= PlayerProgression.maxKitLevel
                ? "MAX LEVEL" : $"Level {level}";

        // score
        if (scoreText != null)
            scoreText.text = $"Score: {progression.score}";

        // find this kit's descriptions (if we have them)
        KitDefinition def = GetKitDef(kit);

        // update the three upgrade slots
        for (int tier = 0; tier < upgradeSlots.Length; tier++)
        {
            UpgradeSlot slot = upgradeSlots[tier];
            if (slot == null) continue;

            string desc = GetUpgradeDescription(def, tier);
            int cost = progression.kitUpgradeCosts[tier];

            // determine this tier's state
            UpgradeSlot.SlotState state;
            if (bought > tier)
                state = UpgradeSlot.SlotState.Owned;              // already purchased
            else if (level < tier + 1)
                state = UpgradeSlot.SlotState.LockedByLevel;      // need higher kit level
            else if (bought == tier)
                state = UpgradeSlot.SlotState.Available;          // next one you can buy
            else
                state = UpgradeSlot.SlotState.LockedByOrder;      // must buy earlier tier first

            slot.SetState(state, tier + 1, desc, cost);
        }

        // buy button — buys the NEXT upgrade
        bool canBuy = progression.CanBuyKitUpgrade(kit);
        int nextCost = progression.GetNextKitUpgradeCost(kit);

        if (buyButton != null)
            buyButton.interactable = canBuy;

        if (buyButtonText != null)
        {
            if (bought >= 3)
                buyButtonText.text = "All Upgrades Owned";
            else if (level < bought + 1)
                buyButtonText.text = $"Reach Level {bought + 1}";
            else if (!canBuy)
                buyButtonText.text = $"Need {nextCost} Score";
            else
                buyButtonText.text = $"Buy Upgrade {bought + 1} ({nextCost})";
        }
    }

    private KitDefinition GetKitDef(string kitName)
    {
        if (allKits == null) return null;
        foreach (KitDefinition k in allKits)
            if (k != null && k.kitName == kitName) return k;
        return null;
    }

    private string GetUpgradeDescription(KitDefinition def, int tier)
    {
        if (def == null) return $"Upgrade {tier + 1}";
        switch (tier)
        {
            case 0: return string.IsNullOrEmpty(def.upgradeOneDescription) ? "Upgrade 1" : def.upgradeOneDescription;
            case 1: return string.IsNullOrEmpty(def.upgradeTwoDescription) ? "Upgrade 2" : def.upgradeTwoDescription;
            case 2: return string.IsNullOrEmpty(def.upgradeThreeDescription) ? "Upgrade 3" : def.upgradeThreeDescription;
            default: return "";
        }
    }
}