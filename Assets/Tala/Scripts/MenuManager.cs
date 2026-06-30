using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject multiplayerPanel;
    public GameObject settingsPanel;

    [Header("Multiplayer UI")]
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;
    public TMP_Text sessionCodeText;

    private void Start()
    {
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (sessionCodeText != null)
            sessionCodeText.gameObject.SetActive(false);

        Debug.Log("MenuManager Started");
    }

    // زر Play
    public void PlaySolo()
    {
        Debug.Log("PlaySolo Pressed");

        MultiplayerManager.Instance.StartSolo();
        SceneManager.LoadScene("Office-Level");
    }

    // فتح قائمة Multiplayer
    public void OpenMultiplayer()
    {
        Debug.Log("OpenMultiplayer Pressed");

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(true);
    }

    // إغلاق قائمة Multiplayer
    public void CloseMultiplayer()
    {
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
    }

    // فتح الإعدادات
    public void OpenSettings()
    {
        Debug.Log("OpenSettings Pressed");

        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // إغلاق الإعدادات
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // خروج
    public void QuitGame()
    {
        Debug.Log("Quit Pressed");
        Application.Quit();
    }

    // إنشاء Session
    public async void HostGame()
    {
        if (statusText != null)
            statusText.text = "Creating session...";

        bool success = await MultiplayerManager.Instance.Host();

        if (success)
        {
            string code = MultiplayerManager.Instance.GetSessionCode();

            if (sessionCodeText != null)
            {
                sessionCodeText.gameObject.SetActive(true);
                sessionCodeText.text = "Code: " + code;
            }

            GUIUtility.systemCopyBuffer = code;

            if (statusText != null)
                statusText.text = "Session created! Code copied.";

            MultiplayerManager.Instance.LoadOfficeScene();
        }
        else
        {
            if (statusText != null)
                statusText.text = "Failed to create session.";
        }
    }

    // الانضمام إلى Session
    public async void JoinGame()
    {
        if (joinCodeInput == null || string.IsNullOrWhiteSpace(joinCodeInput.text))
        {
            if (statusText != null)
                statusText.text = "Enter a code first.";

            return;
        }

        if (statusText != null)
            statusText.text = "Joining...";

        bool success = await MultiplayerManager.Instance.Join(joinCodeInput.text.Trim());

        if (success)
        {
            if (statusText != null)
                statusText.text = "Joined!";
        }
        else
        {
            if (statusText != null)
                statusText.text = "Failed to join.";
        }
    }
}