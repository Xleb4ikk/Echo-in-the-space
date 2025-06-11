using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("Звуки")]
    [SerializeField] private AudioClip footstepSound; // Звук шага
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f; // Громкость

    [Header("Настройки")]
    [SerializeField] private float walkPitch = 1.0f;
    [SerializeField] private float sprintPitch = 1.7f;
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private bool forceDebugMode = true;

    [Header("Аудиоисточники (назначьте вручную)")]
    [SerializeField] private AudioSource walkAudioSource;
    [SerializeField] private AudioSource sprintAudioSource;

    // Внутренние состояния
    private Player playerScript;
    private bool isMoving = false;
    private bool isSprinting = false;
    private float stepTimer = 0f;

    private void Start()
    {
        playerScript = GetComponent<Player>() ?? GetComponentInParent<Player>() ?? GetComponentInChildren<Player>();

        if (forceDebugMode)
        {
            Debug.Log("FootstepSound: Инициализация завершена (ручной режим AudioSource)");
            if (playerScript == null)
                Debug.LogWarning("Не найден компонент Player");
        }

        // Настройка аудиоисточников
        SetupAudioSource(walkAudioSource, walkPitch);
        SetupAudioSource(sprintAudioSource, sprintPitch);
    }

    private void SetupAudioSource(AudioSource source, float pitch)
    {
        if (source != null)
        {
            source.clip = footstepSound;
            source.loop = false;
            source.playOnAwake = false;
            source.volume = volume;
            source.pitch = pitch;
        }
        else
        {
            Debug.LogWarning("Один из источников AudioSource не назначен!");
        }
    }

    private void Update()
    {
        if (playerScript == null) return;

        isMoving = playerScript.MoveInput.magnitude > 0.1f;
        isSprinting = playerScript.IsSprinting;

        if (!isMoving)
        {
            StopAllFootsteps();
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;
        float currentInterval = isSprinting ? stepInterval / (sprintPitch / walkPitch) : stepInterval;

        if (stepTimer >= currentInterval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (isSprinting && sprintAudioSource != null)
        {
            sprintAudioSource.pitch = sprintPitch;
            sprintAudioSource.PlayOneShot(footstepSound, volume);
            if (forceDebugMode) Debug.Log("Бег - шаг");
        }
        else if (walkAudioSource != null)
        {
            walkAudioSource.pitch = walkPitch;
            walkAudioSource.PlayOneShot(footstepSound, volume);
            if (forceDebugMode) Debug.Log("Ходьба - шаг");
        }
    }

    private void StopAllFootsteps()
    {
        if (walkAudioSource && walkAudioSource.isPlaying)
            walkAudioSource.Stop();

        if (sprintAudioSource && sprintAudioSource.isPlaying)
            sprintAudioSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (walkAudioSource != null) walkAudioSource.volume = volume;
        if (sprintAudioSource != null) sprintAudioSource.volume = volume;
    }

    private void OnDestroy()
    {
        StopAllFootsteps();
    }
}
