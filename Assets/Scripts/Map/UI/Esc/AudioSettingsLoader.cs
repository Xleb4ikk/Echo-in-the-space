using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsLoader : MonoBehaviour
{
    public AudioMixer audioMixer;

    void Start()
    {
        LoadAndApplyVolume("musicVolume", "MusicVolume");
        LoadAndApplyVolume("sfxVolume", "SFXVolume");
    }

    private void LoadAndApplyVolume(string playerPrefKey, string mixerParameter)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioSettingsLoader: AudioMixer не присвоен");
            return;
        }

        float volume = PlayerPrefs.GetFloat(playerPrefKey, 1f);
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat(mixerParameter, dB);
    }
}
