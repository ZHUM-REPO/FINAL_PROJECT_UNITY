using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public Slider sensitivitySlider;

    void Start()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0);
        float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 3);

        masterSlider.value = master;
        musicSlider.value = music;
        sfxSlider.value = sfx;
        sensitivitySlider.value = sensitivity;

        audioMixer.SetFloat("MasterVolume", master);
        audioMixer.SetFloat("MusicVolume", music);
        audioMixer.SetFloat("SFXVolume", sfx);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }
}