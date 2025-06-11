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

    private void Start()
    {
        // Добавляем AudioSource, если его нет
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        // Настройка источника
        musicSource.clip = newMusicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f; // Начинаем с нулевой громкости
    }

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
            musicSource.Play();
            musicSource.volume = newMusicVolume;
        }

        isTriggered = true;
    }

    // Метод для обновления громкости вручную, если нужно
    public void SetVolume(float volume)
    {
        newMusicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = newMusicVolume;
    }
}
