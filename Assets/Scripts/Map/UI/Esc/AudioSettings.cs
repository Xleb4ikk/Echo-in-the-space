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
        // Проверяем и настраиваем каждый компонент отдельно
        InitMusicSlider();
        InitSFXSlider();
    }

    private void InitMusicSlider()
    {

        float musicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        musicSlider.value = musicVolume;
        ApplyVolume("MusicVolume", musicVolume);

        musicSlider.onValueChanged.AddListener((value) => {
            ApplyVolume("MusicVolume", value);
            PlayerPrefs.SetFloat("musicVolume", value);
        });
    }

    private void InitSFXSlider()
    {
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        sfxSlider.value = sfxVolume;
        ApplyVolume("SFXVolume", sfxVolume);

        sfxSlider.onValueChanged.AddListener((value) => {
            ApplyVolume("SFXVolume", value);
            PlayerPrefs.SetFloat("sfxVolume", value);
        });
    }

    private void ApplyVolume(string parameterName, float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioSettings: AudioMixer не присвоен");
            return;
        }
        
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat(parameterName, dB);
    }
}
