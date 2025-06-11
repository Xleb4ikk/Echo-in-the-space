using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        ApplyVolume("MusicVolume", musicVolume);
        ApplyVolume("SFXVolume", sfxVolume);

        musicSlider.onValueChanged.AddListener((value) => {
            ApplyVolume("MusicVolume", value);
            PlayerPrefs.SetFloat("musicVolume", value);
        });

        sfxSlider.onValueChanged.AddListener((value) => {
            ApplyVolume("SFXVolume", value);
            PlayerPrefs.SetFloat("sfxVolume", value);
        });
    }

    private void ApplyVolume(string parameterName, float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat(parameterName, dB);
    }
}
