using UnityEngine;

public class OfficeComputerScreens : MonoBehaviour
{
    [Header("Big Screen Panels")]
    public GameObject kitSelectionPanel;   // shown when Kit Selection is pressed
    public GameObject upgradesPanel;       // shown when Upgrades is pressed

    private void Start()
    {
        // start on kit selection by default
        ShowKitSelection();
    }

    public void ShowKitSelection()
    {
        if (kitSelectionPanel != null) kitSelectionPanel.SetActive(true);
        if (upgradesPanel != null) upgradesPanel.SetActive(false);
    }

    public void ShowUpgrades()
    {
        if (kitSelectionPanel != null) kitSelectionPanel.SetActive(false);
        if (upgradesPanel != null) upgradesPanel.SetActive(true);
    }
}