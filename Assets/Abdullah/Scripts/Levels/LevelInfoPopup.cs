using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class LevelInfoPopup : MonoBehaviour
{
    public static bool AnyPopupOpen = false;

    [Header("Panel")]
    public GameObject panel;

    [Header("Text Fields")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI kitNeededText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI statusText;

    [Header("Buttons")]
    public Button takeContractButton;       // host-only
    public TextMeshProUGUI takeContractLabel;
    public Button closeButton;

    public bool IsOpen { get; private set; }

    private LevelDefinition currentLevel;
    private int currentIndex;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (takeContractButton != null) takeContractButton.onClick.AddListener(TakeContract);
    }

    public void Open(LevelDefinition level, int levelIndex)
    {
        currentLevel = level;
        currentIndex = levelIndex;
        IsOpen = true;
        AnyPopupOpen = true;

        if (panel != null) panel.SetActive(true);

        if (nameText != null) nameText.text = level.levelName;
        if (descriptionText != null) descriptionText.text = level.description;
        if (kitNeededText != null) kitNeededText.text = $"Kit Needed: {level.kitNeeded}";
        if (rewardText != null) rewardText.text = $"Reward: {level.scoreReward}\n{level.rewardDescription}";

        bool unlocked = LevelProgressManager.Instance == null
            || LevelProgressManager.Instance.IsUnlocked(level);
        bool completed = LevelProgressManager.Instance != null
            && LevelProgressManager.Instance.IsCompleted(level.sceneName);

        if (statusText != null)
        {
            if (!unlocked && level.requiredLevel != null)
                statusText.text = $"LOCKED — complete {level.requiredLevel.levelName} first";
            else if (completed)
                statusText.text = "COMPLETED — replayable for the same reward";
            else
                statusText.text = "AVAILABLE";
        }

        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        if (takeContractButton != null)
            takeContractButton.gameObject.SetActive(isHost && unlocked);

        UpdateContractLabel();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateContractLabel()
    {
        if (takeContractLabel == null) return;
        bool isSelected = LevelSelectionManager.Instance != null
            && LevelSelectionManager.Instance.selectedLevel.Value == currentIndex;
        takeContractLabel.text = isSelected ? "Contract Taken" : "Take Contract";
    }

    private void TakeContract()
    {
        if (LevelSelectionManager.Instance == null) return;
        LevelSelectionManager.Instance.SelectLevel(currentIndex);
        UpdateContractLabel();
    }

    public void Close()
    {
        IsOpen = false;
        AnyPopupOpen = false;
        if (panel != null) panel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}