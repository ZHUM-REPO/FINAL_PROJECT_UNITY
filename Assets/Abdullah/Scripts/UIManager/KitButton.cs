using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KitButton : MonoBehaviour
{
    public enum State { Available, Mine, Taken }

    [Header("References")]
    public Button button;
    public TextMeshProUGUI label;        // kit name / status
    public Image background;

    [Header("Kit Info")]
    public string kitName = "Fire";

    [Header("State Colors")]
    public Color availableColor = new Color(0.2f, 0.3f, 0.4f);
    public Color mineColor = new Color(0.2f, 0.6f, 0.3f);
    public Color takenColor = new Color(0.3f, 0.3f, 0.3f);

    public void SetState(State state)
    {
        switch (state)
        {
            case State.Available:
                background.color = availableColor;
                label.text = kitName + "\n(Select)";
                button.interactable = true;
                break;

            case State.Mine:
                background.color = mineColor;
                label.text = kitName + "\n(Equipped)";
                button.interactable = true;   // clickable to unequip
                break;

            case State.Taken:
                background.color = takenColor;
                label.text = kitName + "\n(Taken)";
                button.interactable = false;  // locked
                break;
        }
    }
}