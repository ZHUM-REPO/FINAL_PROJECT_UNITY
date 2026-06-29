using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainButtonsPanel;
    public GameObject multiplayerPanel;
    public GameObject settingsPanel;

    [Header("Multiplayer UI")]
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;
    public TMP_Text sessionCodeText;

    private void Start()
    {
        mainButtonsPanel.SetActive(true);
        multiplayerPanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (sessionCodeText != null)
            sessionCodeText.gameObject.SetActive(false);
    }

    // ─── Main Buttons ─────────────────────────────────────

    public void PlaySolo()
    {
        MultiplayerManager.Instance.StartSolo();
        SceneManager.LoadScene("TheOffice");
    }

    public void OpenMultiplayer()
    {
        mainButtonsPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
    }

    public void CloseMultiplayer()
    {
        mainButtonsPanel.SetActive(true);
        multiplayerPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Pressed");
        Application.Quit();
    }

    // ─── Multiplayer ──────────────────────────────────────

    public async void HostGame()
    {
        if (statusText != null) statusText.text = "Creating session...";

        bool success = await MultiplayerManager.Instance.Host();

        if (success)
        {
            string code = MultiplayerManager.Instance.GetSessionCode();

            // show code on screen
            if (sessionCodeText != null)
            {
                sessionCodeText.gameObject.SetActive(true);
                sessionCodeText.text = $"Code: {code}";
            }

            // copy to clipboard automatically
            GUIUtility.systemCopyBuffer = code;

            if (statusText != null) statusText.text = "Session created!";

            // load office — synced to all clients via NetworkManager
            MultiplayerManager.Instance.LoadOfficeScene();
        }
        else
        {
            if (statusText != null) statusText.text = "Failed to create session.";
        }
    }

    public async void JoinGame()
    {
        if (joinCodeInput == null || string.IsNullOrEmpty(joinCodeInput.text))
        {
            if (statusText != null) statusText.text = "Enter a code first.";
            return;
        }

        if (statusText != null) statusText.text = "Joining...";

        bool success = await MultiplayerManager.Instance.Join(joinCodeInput.text.Trim());

        if (success)
        {
            if (statusText != null) statusText.text = "Joined!";
            // client will be brought to office by host's LoadOfficeScene call
        }
        else
        {
            if (statusText != null) statusText.text = "Failed to join.";
        }
    }
}