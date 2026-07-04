using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class StatUpgradesUI : MonoBehaviour
{
    [Header("Health")]
    public TextMeshProUGUI healthValueText;   // "Max HP: 100"
    public TextMeshProUGUI healthCostText;    // "Cost: 50"
    public Button healthBuyButton;

    [Header("Mana")]
    public TextMeshProUGUI manaValueText;
    public TextMeshProUGUI manaCostText;
    public Button manaBuyButton;

    [Header("Endurance")]
    public TextMeshProUGUI enduranceValueText;
    public TextMeshProUGUI enduranceCostText;
    public Button enduranceBuyButton;

    [Header("Score (optional)")]
    public TextMeshProUGUI scoreText;

    [Header("Testing (optional) — temporary button to add score")]
    public Button addTestScoreButton;

    private PlayerStats playerStats;
    private PlayerProgression progression;
    private bool wired = false;

    private void Update()
    {
        if (playerStats == null || progression == null)
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

        foreach (PlayerStats ps in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            NetworkObject no = ps.GetComponent<NetworkObject>();
            if (no != null && no.IsOwner)
            {
                playerStats = ps;
                progression = ps.GetComponent<PlayerProgression>();
                break;
            }
        }
    }

    private void WireButtons()
    {
        if (healthBuyButton != null)
            healthBuyButton.onClick.AddListener(() => { progression.UpgradeHealth(); Refresh(); });

        if (manaBuyButton != null)
            manaBuyButton.onClick.AddListener(() => { progression.UpgradeMana(); Refresh(); });

        if (enduranceBuyButton != null)
            enduranceBuyButton.onClick.AddListener(() => { progression.UpgradeEndurance(); Refresh(); });

        if (addTestScoreButton != null)
            addTestScoreButton.onClick.AddListener(() => { progression.AddScore(100); Refresh(); });

        wired = true;
    }

    private void Refresh()
    {
        if (playerStats == null || progression == null) return;

        if (healthValueText != null)
            healthValueText.text = $"Max HP: {Mathf.CeilToInt(playerStats.maxHealth)}";
        if (manaValueText != null)
            manaValueText.text = $"Max MP: {Mathf.CeilToInt(playerStats.maxMana)}";
        if (enduranceValueText != null)
            enduranceValueText.text = $"Max END: {Mathf.CeilToInt(playerStats.maxEndurance)}";

        if (healthCostText != null)
            healthCostText.text = $"Cost: {progression.healthUpgradeCost}";
        if (manaCostText != null)
            manaCostText.text = $"Cost: {progression.manaUpgradeCost}";
        if (enduranceCostText != null)
            enduranceCostText.text = $"Cost: {progression.enduranceUpgradeCost}";

        if (scoreText != null)
            scoreText.text = $"Score: {progression.score}";

        // grey out what you can't afford
        if (healthBuyButton != null)
            healthBuyButton.interactable = progression.score >= progression.healthUpgradeCost;
        if (manaBuyButton != null)
            manaBuyButton.interactable = progression.score >= progression.manaUpgradeCost;
        if (enduranceBuyButton != null)
            enduranceBuyButton.interactable = progression.score >= progression.enduranceUpgradeCost;
    }
}