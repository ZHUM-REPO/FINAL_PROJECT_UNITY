using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

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

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);

        masterSlider.value = savedMaster;
        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;
        sensitivitySlider.value = savedSensitivity;

        SetMasterVolume(savedMaster);
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);
        SetSensitivity(savedSensitivity);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

    public void OpenPauseMenu()
    {
        isPaused = true;

        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void SetMasterVolume(float value)
    {
        if (value < 0.0001f)
        {
            value = 0.0001f;
        }

        PlayerPrefs.SetFloat("MasterVolume", value);

        float volumeDb = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("MasterVolume", volumeDb);

        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        if (value < 0.0001f)
        {
            value = 0.0001f;
        }

        PlayerPrefs.SetFloat("MusicVolume", value);

        float volumeDb = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("MusicVolume", volumeDb);

        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        if (value < 0.0001f)
        {
            value = 0.0001f;
        }

        PlayerPrefs.SetFloat("SFXVolume", value);

        float volumeDb = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("SFXVolume", volumeDb);

        PlayerPrefs.Save();
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}