using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class HUDManager : MonoBehaviour
{
    [Header("Stat Bars")]
    public Image healthBar;
    public Image manaBar;

    [Header("Spell Info")]
    public TextMeshProUGUI kitNameText;
    public TextMeshProUGUI spellNameText;

    private PlayerStats playerStats;
    private KitManager kitManager;
    private bool isSearching = false;

    private void Update()
    {
        if (playerStats == null || kitManager == null)
        {
            if (!isSearching)
                FindLocalPlayer();
            return;
        }

        UpdateStatBars();
        UpdateSpellInfo();
    }

    private void FindLocalPlayer()
    {
        // safety check — network must be running
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsListening) return;

        isSearching = true;

        // find all PlayerStats in scene and get the one owned by local client
        PlayerStats[] allPlayers = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        foreach (PlayerStats ps in allPlayers)
        {
            NetworkObject netObj = ps.GetComponent<NetworkObject>();
            if (netObj == null) continue;
            if (!netObj.IsOwner) continue;

            playerStats = ps;
            kitManager = ps.GetComponent<KitManager>();

            Debug.Log("HUD found local player.");
            break;
        }

        isSearching = false;
    }

    private void UpdateStatBars()
    {
        if (playerStats == null) return;

        healthBar.fillAmount = playerStats.myHealth / playerStats.maxHealth;
        manaBar.fillAmount = playerStats.myMana / playerStats.maxMana;
    }

    private void UpdateSpellInfo()
    {
        if (kitManager == null) return;

        if (kitManager.equippedKit != null)
            kitNameText.text = $"Kit: {kitManager.equippedKit.kitName}";

        spellNameText.text = $"Spell: {kitManager.GetActiveSpellName()}";
    }
}