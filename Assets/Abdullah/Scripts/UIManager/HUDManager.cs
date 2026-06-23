using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Stat Bars")]
    public Image healthBar;
    public Image manaBar;

    [Header("Spell Info")]
    public TextMeshProUGUI kitNameText;
    public TextMeshProUGUI spellNameText;

    [Header("References")]
    public PlayerStats playerStats;
    public KitManager kitManager;

    private void Update()
    {
        UpdateStatBars();
        UpdateSpellInfo();
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