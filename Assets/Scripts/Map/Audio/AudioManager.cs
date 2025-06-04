using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip ambientSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private bool isMuted = false;
    
    private AudioSource audioSource;
    private float previousVolume;

    private void Start()
    {
        // Получаем или добавляем компонент AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настраиваем параметры аудио
        audioSource.clip = ambientSound;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = volume;

        // Воспроизводим звук
        audioSource.Play();
    }

    // Метод для изменения громкости (можно вызывать из других скриптов)
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume); // Ограничиваем значение от 0 до 1
        if (!isMuted)
        {
            audioSource.volume = volume;
        }
    }

    // Метод для включения/выключения звука
    public void ToggleMute()
    {
        isMuted = !isMuted;
        if (isMuted)
        {
            previousVolume = audioSource.volume;
            audioSource.volume = 0f;
        }
        else
        {
            audioSource.volume = previousVolume;
        }
    }

    // Метод для плавного изменения громкости
    public void FadeVolume(float targetVolume, float duration)
    {
        StartCoroutine(FadeVolumeCoroutine(targetVolume, duration));
    }

    private System.Collections.IEnumerator FadeVolumeCoroutine(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            audioSource.volume = newVolume;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    // Метод для получения текущей громкости
    public float GetVolume()
    {
        return audioSource.volume;
    }
} 