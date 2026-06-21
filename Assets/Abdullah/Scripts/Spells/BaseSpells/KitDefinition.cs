using UnityEngine;

[CreateAssetMenu(fileName = "NewKit", menuName = "Spells/Kit Definition")]
public class KitDefinition : ScriptableObject
{
    [Header("Kit Info")]
    public string kitName;
    public Sprite kitIcon;

    [Header("Spells")]
    // these are the prefabs that have SpellBase components on them
    public GameObject spellOnePrefab;
    public GameObject spellTwoPrefab;

    [Header("Upgrades — unlocked at kit levels 1, 2, 3")]
    public string upgradeOneDescription;
    public string upgradeTwoDescription;
    public string upgradeThreeDescription;
}