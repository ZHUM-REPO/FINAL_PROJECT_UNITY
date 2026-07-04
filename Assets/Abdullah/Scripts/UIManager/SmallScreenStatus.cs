using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class SmallScreenStatus : MonoBehaviour
{
    [Header("Kit Info")]
    public TextMeshProUGUI kitNameText;    // "Fire"
    public TextMeshProUGUI kitLevelText;   // "Level 2"
    public Image xpBarFill;                // fill image (Image type = Filled)
    public TextMeshProUGUI xpText;         // optional "60%" (can be left null)

    [Header("Stats (numbers)")]
    public TextMeshProUGUI healthText;     // "HP 80 / 100"
    public TextMeshProUGUI manaText;       // "MP 45 / 100"
    public TextMeshProUGUI enduranceText;  // "END 100 / 100"

    [Header("Score")]
    public TextMeshProUGUI scoreText;      // "Score: 250"

    private PlayerStats playerStats;
    private PlayerProgression playerProgression;
    private KitManager kitManager;
    private bool isSearching = false;

    private void Update()
    {
        // find the local player once it exists
        if (playerStats == null || kitManager == null || playerProgression == null)
        {
            if (!isSearching) FindLocalPlayer();
            return;
        }

        UpdateKitInfo();
        UpdateStats();
        UpdateScore();
    }

    private void FindLocalPlayer()
    {
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsListening) return;

        isSearching = true;

        PlayerStats[] all = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        foreach (PlayerStats ps in all)
        {
            NetworkObject netObj = ps.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                playerStats = ps;
                kitManager = ps.GetComponent<KitManager>();
                playerProgression = ps.GetComponent<PlayerProgression>();
                break;
            }
        }

        isSearching = false;
    }

    private void UpdateKitInfo()
    {
        // no kit equipped
        if (kitManager.equippedKit == null)
        {
            if (kitNameText != null) kitNameText.text = "No Kit";
            if (kitLevelText != null) kitLevelText.text = "";
            if (xpBarFill != null) xpBarFill.fillAmount = 0f;
            if (xpText != null) xpText.text = "";
            return;
        }

        string kitName = kitManager.equippedKit.kitName;

        if (kitNameText != null)
            kitNameText.text = kitName;

        int level = playerProgression.GetKitLevel(kitName);
        if (kitLevelText != null)
        {
            // show "MAX" once the kit hits the level cap
            kitLevelText.text = level >= PlayerProgression.maxKitLevel
                ? "MAX"
                : $"Level {level}";
        }

        float progress = playerProgression.GetKitXPProgress(kitName); // 0..1
        if (xpBarFill != null)
            xpBarFill.fillAmount = progress;

        if (xpText != null)
        {
            xpText.text = level >= PlayerProgression.maxKitLevel
                ? "MAX"
                : $"{Mathf.RoundToInt(progress * 100f)}%";
        }
    }

    private void UpdateStats()
    {
        if (healthText != null)
            healthText.text = $"HP {Mathf.CeilToInt(playerStats.myHealth)} / {Mathf.CeilToInt(playerStats.maxHealth)}";

        if (manaText != null)
            manaText.text = $"MP {Mathf.CeilToInt(playerStats.myMana)} / {Mathf.CeilToInt(playerStats.maxMana)}";

        if (enduranceText != null)
            enduranceText.text = $"END {Mathf.CeilToInt(playerStats.myEndurance)} / {Mathf.CeilToInt(playerStats.maxEndurance)}";
    }

    private void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {playerProgression.score}";
    }
}