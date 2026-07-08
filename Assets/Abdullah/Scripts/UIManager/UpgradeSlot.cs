using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeSlot : MonoBehaviour
{
    public enum SlotState { Owned, Available, LockedByLevel, LockedByOrder }

    [Header("References")]
    public TextMeshProUGUI titleText;      // "Upgrade 1"
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Image background;

    [Header("State Colors")]
    public Color ownedColor = new Color(0.2f, 0.6f, 0.3f);
    public Color availableColor = new Color(0.2f, 0.4f, 0.6f);
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f);

    public void SetState(SlotState state, int tierNumber, string description, int cost)
    {
        if (titleText != null) titleText.text = $"Upgrade {tierNumber}";
        if (descriptionText != null) descriptionText.text = description;

        switch (state)
        {
            case SlotState.Owned:
                if (background != null) background.color = ownedColor;
                if (costText != null) costText.text = "OWNED";
                break;

            case SlotState.Available:
                if (background != null) background.color = availableColor;
                if (costText != null) costText.text = $"Cost: {cost}";
                break;

            case SlotState.LockedByLevel:
                if (background != null) background.color = lockedColor;
                if (costText != null) costText.text = $"Needs Level {tierNumber}";
                break;

            case SlotState.LockedByOrder:
                if (background != null) background.color = lockedColor;
                if (costText != null) costText.text = "Locked";
                break;
        }
    }
}