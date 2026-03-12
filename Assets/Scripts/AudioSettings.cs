using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    // UI Slider for volume channel
    public AudioMixer mainMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider SfxSlider;
    // Panel references for switching between menus
    public GameObject settingsPanel;
    public GameObject mainMenuPanel;

    // Load saved volume settings when the menu starts
    void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        SfxSlider.value = PlayerPrefs.GetFloat("SfxVolume", 0.75f);
        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSfxVolume(SfxSlider.value);
    }

    public void SetMasterVolume(float value)
    {
        float db = value <= 0.001f ? -80f : Mathf.Log10(value) * 20f;
        mainMixer.SetFloat("MasterVolume", db);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        float db = value <= 0.001f ? -80f : Mathf.Log10(value) * 20f;
        mainMixer.SetFloat("MusicVolume", db);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSfxVolume(float value)
    {
        float db = value <= 0.001f ? -80f : Mathf.Log10(value) * 20f;
        mainMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat("SfxVolume", value);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

}