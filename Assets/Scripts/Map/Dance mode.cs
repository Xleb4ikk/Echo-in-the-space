using UnityEngine;
using UnityEngine.InputSystem;

public class Dancemode : MonoBehaviour
{
    [Header("Материалы для перелива эмиссии")]
    public Material[] materials;

    [Range(0.1f, 10f)]
    public float colorCycleSpeed = 1f;

    [Header("Твой основной AudioSource (не заглушать)")]
    public AudioSource musicSource;

    [Header("Объекты для включения/выключения")]
    public GameObject[] objectsToToggle;

    private bool flexModeActive = false;
    private bool autoActivated = false; // Чтобы один раз включить

    private void Start()
    {
        // Изначально выключаем эмиссию у материалов
        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty("_EmissionColor"))
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
        }

        // Заглушаем все остальные аудио сразу
        MuteAllOtherAudioSources(true);

        // Запускаем музыку сразу, чтобы можно было отсчитывать время
        if (musicSource != null)
            musicSource.Play();

        // Объекты выключены
        SetObjectsActive(false);
    }

    private void Update()
    {
        Debug.Log(musicSource.time);
        // Автоматическое включение через 45 секунд от начала музыки
        if (!autoActivated && musicSource != null && musicSource.isPlaying)
        {
            
            if (musicSource.time >= 60f)
            {
                
                autoActivated = true;
                flexModeActive = true;
                OnFlexModeChanged(true);
            }
        }

        // Включение/выключение вручную по F (опционально)
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            flexModeActive = !flexModeActive;
            OnFlexModeChanged(flexModeActive);
        }

        if (flexModeActive)
        {
            UpdateRainbowEmission();
        }
    }

    private void OnFlexModeChanged(bool active)
    {
        if (active)
        {
            // Включаем эмиссию
            foreach (Material mat in materials)
            {
                if (mat != null && mat.HasProperty("_EmissionColor"))
                    mat.EnableKeyword("_EMISSION");
            }

            // Включаем музыку (если не играет)
            if (musicSource != null && !musicSource.isPlaying)
                musicSource.Play();

            // Включаем объекты
            SetObjectsActive(true);

            // Включаем звук у других AudioSource
            //MuteAllOtherAudioSources(false);
        }
        else
        {
            // Выключаем эмиссию (черный цвет)
            foreach (Material mat in materials)
            {
                if (mat != null && mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                    mat.DisableKeyword("_EMISSION");
                }
            }

            // Пауза музыки
            if (musicSource != null)
                musicSource.Pause();

            // Выключаем объекты
            SetObjectsActive(false);

            // Заглушаем другие AudioSource
            MuteAllOtherAudioSources(true);
        }
    }

    private void UpdateRainbowEmission()
    {
        float hue = Mathf.Repeat(Time.time * colorCycleSpeed, 1f);
        Color emissionColor = Color.HSVToRGB(hue, 1f, 1f);

        foreach (Material mat in materials)
        {
            if (mat == null) continue;
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", emissionColor);
        }
    }

    private void SetObjectsActive(bool active)
    {
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void MuteAllOtherAudioSources(bool mute)
    {
        AudioSource[] allSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource source in allSources)
        {
            if (source != musicSource)
            {
                source.volume = mute ? 0f : 1f;
            }
        }
    }
}
