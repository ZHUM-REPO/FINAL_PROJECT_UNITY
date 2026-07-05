using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Levels/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Identity")]
    public string levelName = "Level 1";
    public string sceneName = "Level-1";   // EXACT scene name in Build Settings
    [TextArea(3, 6)] public string description;

    [Header("Requirements")]
    public string kitNeeded = "Any";
    public LevelDefinition requiredLevel;   // must be completed first (null = none)

    [Header("Rewards")]
    public int scoreReward = 100;
    [TextArea(2, 4)] public string rewardDescription;
}