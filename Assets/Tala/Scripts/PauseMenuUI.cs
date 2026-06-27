using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider sensitivitySlider;

    public static bool isPaused = false;

    private void OnEnable()
    {
        PlayerInputs.OnPauseInput += TogglePause;
    }

    private void OnDisable()
    {
        PlayerInputs.OnPauseInput -= TogglePause;
    }

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        LoadSettings();
        LockCursor();
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            OpenPauseMenu();
    }

    public void OpenPauseMenu()
    {
        isPaused = true;

        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ResumeGame()
    {
        isPaused = false;

        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        LockCursor();
    }

    public void OpenSettings()
    {
        isPaused = true;

        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void BackToPauseMenu()
    {
        isPaused = true;

        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("Main-Menu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void LoadSettings()
    {
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);

        if (masterSlider != null) masterSlider.value = savedMaster;
        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
        if (sensitivitySlider != null) sensitivitySlider.value = savedSensitivity;

        ApplyMasterVolume(savedMaster);
        ApplyMusicVolume(savedMusic);
        ApplySFXVolume(savedSFX);
    }

    public void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        ApplyMasterVolume(value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        ApplyMusicVolume(value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        ApplySFXVolume(value);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }

    void ApplyMasterVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", ConvertToDecibel(value));
    }

    void ApplyMusicVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", ConvertToDecibel(value));
    }

    void ApplySFXVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", ConvertToDecibel(value));
    }

    float ConvertToDecibel(float value)
    {
        if (value <= 0.0001f)
            value = 0.0001f;

        return Mathf.Log10(value) * 20f;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}