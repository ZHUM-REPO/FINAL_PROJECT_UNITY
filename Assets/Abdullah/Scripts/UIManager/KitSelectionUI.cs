using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class KitSelectionUI : MonoBehaviour
{
    [Header("Kit Buttons (one per kit, same order as manager)")]
    public KitButton[] kitButtons;

    private void OnEnable()
    {
        // refresh whenever ownership changes
        if (KitSelectionManager.Instance != null)
            KitSelectionManager.Instance.OnKitOwnershipChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (KitSelectionManager.Instance != null)
            KitSelectionManager.Instance.OnKitOwnershipChanged -= Refresh;
    }

    private void Start()
    {
        // hook up each button to send its index to the manager
        for (int i = 0; i < kitButtons.Length; i++)
        {
            int index = i; // capture for the lambda
            kitButtons[i].button.onClick.AddListener(() => OnKitClicked(index));
        }
        Refresh();
    }

    private void OnKitClicked(int kitIndex)
    {
        if (KitSelectionManager.Instance == null) return;
        KitSelectionManager.Instance.RequestToggleKit(kitIndex);
    }

    private void Refresh()
    {
        var manager = KitSelectionManager.Instance;
        if (manager == null) return;

        for (int i = 0; i < kitButtons.Length; i++)
        {
            KitButton kb = kitButtons[i];

            if (manager.IsOwnedByLocalPlayer(i))
            {
                kb.SetState(KitButton.State.Mine);       // green / "Equipped"
            }
            else if (manager.IsFree(i))
            {
                kb.SetState(KitButton.State.Available);   // normal / "Select"
            }
            else
            {
                kb.SetState(KitButton.State.Taken);        // locked / greyed out
            }
        }
    }
}