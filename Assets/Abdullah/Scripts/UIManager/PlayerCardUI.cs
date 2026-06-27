using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PlayerCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI kitNameText;
    public TextMeshProUGUI spellNameText;
    public Image healthBar;
    public Image manaBar;
    public GameObject deadOverlay;  // dark overlay shown when player is dead

    private ulong clientId;
    private PlayerStats playerStats;
    private KitManager kitManager;
    private NetworkPlayerData networkData;
    private PlayerDeath playerDeath;

    public void Setup(ulong id, PlayerStats stats, KitManager kit,
                      NetworkPlayerData netData, PlayerDeath death)
    {
        clientId = id;
        playerStats = stats;
        kitManager = kit;
        networkData = netData;
        playerDeath = death;

        // set player name
        playerNameText.text = $"Player {id + 1}";

        if (deadOverlay != null)
            deadOverlay.SetActive(false);
    }

    private void Update()
    {
        if (playerStats == null) return;

        UpdateBars();
        UpdateKitInfo();
        UpdateDeathState();
    }

    private void UpdateBars()
    {
        // use network data if available (more accurate for remote players)
        if (networkData != null)
        {
            healthBar.fillAmount = networkData.networkHealth.Value
                                 / playerStats.maxHealth;
            manaBar.fillAmount = networkData.networkMana.Value
                               / playerStats.maxMana;
        }
        else
        {
            healthBar.fillAmount = playerStats.myHealth / playerStats.maxHealth;
            manaBar.fillAmount = playerStats.myMana / playerStats.maxMana;
        }
    }

    private void UpdateKitInfo()
    {
        if (kitManager == null) return;

        if (kitManager.equippedKit != null)
            kitNameText.text = kitManager.equippedKit.kitName;

        spellNameText.text = kitManager.GetActiveSpellName();
    }

    private void UpdateDeathState()
    {
        if (playerDeath == null || deadOverlay == null) return;

        deadOverlay.SetActive(playerDeath.IsDead());
    }
}