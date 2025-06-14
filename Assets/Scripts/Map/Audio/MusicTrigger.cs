using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [Header("Ambient Settings")]
    [SerializeField] private AudioSource ambientSource; // Сюда передай эмбиентный AudioSource

    [Header("New Music Settings")]
    [SerializeField] private AudioClip newMusicClip;
    [SerializeField, Range(0f, 1f)] private float newMusicVolume = 0.8f;

    private AudioSource musicSource;
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered || !other.CompareTag("Player")) return;

        // Останавливаем эмбиент и обнуляем громкость
        if (ambientSource != null)
        {
            ambientSource.Stop();
            ambientSource.volume = 0f;
        }

        // Включаем новую музыку
        if (newMusicClip != null)
        {
            ambientSource.volume = newMusicVolume;
            ambientSource.clip = newMusicClip;
            ambientSource.Play();
        }

        isTriggered = true;
    }

    public void SetVolume(float volume)
    {
        newMusicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = newMusicVolume;
    }
}
