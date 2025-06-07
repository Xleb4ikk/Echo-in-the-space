using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioManager ambientAudioManager; // Ссылка на AudioManager
    [SerializeField] private AudioClip newMusicClip; // Новая музыка
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f; // Громкость
    
    private AudioSource audioSource;
    private BoxCollider triggerBox;
    private bool isTriggered = false;
    
    private void Start()
    {
        // Получаем или создаем Box Collider
        triggerBox = GetComponent<BoxCollider>();
        if (triggerBox == null)
        {
            triggerBox = gameObject.AddComponent<BoxCollider>();
        }
        triggerBox.isTrigger = true;
        
        // Получаем или создаем Audio Source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Настраиваем Audio Source
        audioSource.clip = newMusicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            // Полностью выключаем эмбиент музыку
            if (ambientAudioManager != null)
            {
                // Находим и останавливаем все AudioSource у AudioManager
                AudioSource[] audioSources = ambientAudioManager.GetComponents<AudioSource>();
                foreach (AudioSource source in audioSources)
                {
                    source.Stop();
                }
                
                // Также применяем ToggleMute для установки флага mute
                ambientAudioManager.ToggleMute();
                
                // Устанавливаем громкость на 0 через AudioManager
                ambientAudioManager.SetVolume(0);
            }
            
            // Включаем новую музыку
            if (newMusicClip != null)
            {
                audioSource.Play();
            }
            
            isTriggered = true;
        }
    }
    
    // Метод для изменения громкости
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
} 