using System.Collections.Generic;

public class ProgressionData
{
    public int score = 0;
    public Dictionary<string, float> kitXP = new Dictionary<string, float>();
    public Dictionary<string, int> kitLevels = new Dictionary<string, int>();
    public Dictionary<string, int> kitUpgradesBought = new Dictionary<string, int>();

    public float bonusMaxHealth = 0f;
    public float bonusMaxMana = 0f;
    public float bonusMaxEndurance = 0f;

    // the kit names this player chose in the office (carries into levels)
    public List<string> chosenKits = new List<string>();
}