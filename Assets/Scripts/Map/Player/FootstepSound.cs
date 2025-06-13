using UnityEngine;


public class FootstepSound : MonoBehaviour
{
    [Header("Звуки")]
    [SerializeField] private AudioClip defaultFootstepSound;
    [SerializeField] private AudioClip stairsFootstepSound;

    [Header("Громкость шагов")]
    [SerializeField, Range(0f, 1f)] private float defaultFootstepVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float stairsFootstepVolume = 0.5f;

    [Header("Настройки")]
    [SerializeField] private float walkPitch = 1.0f;
    [SerializeField] private float sprintPitch = 1.7f;
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private bool forceDebugMode = true;

    [Header("Аудиоисточники")]
    [SerializeField] private AudioSource walkAudioSource;
    [SerializeField] private AudioSource sprintAudioSource;

    // Состояния
    private Player playerScript;
    private bool isMoving = false;
    private bool isSprinting = false;
    private float stepTimer = 0f;
    private bool onStairs = false;

    private void Start()
    {
        playerScript = GetComponent<Player>() ?? GetComponentInParent<Player>() ?? GetComponentInChildren<Player>();

        if (forceDebugMode)
        {
            Debug.Log("FootstepSound: Инициализация завершена (ручной режим AudioSource)");
            if (playerScript == null)
                Debug.LogWarning("Не найден компонент Player");
        }

        SetupAudioSource(walkAudioSource, walkPitch);
        SetupAudioSource(sprintAudioSource, sprintPitch);
    }

    private void SetupAudioSource(AudioSource source, float pitch)
    {
        if (source != null)
        {
            source.clip = defaultFootstepSound;
            source.loop = false;
            source.playOnAwake = false;
            source.pitch = pitch;
            // Громкость задаётся в момент воспроизведения в зависимости от поверхности
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
        AudioClip currentClip = onStairs ? stairsFootstepSound : defaultFootstepSound;
        float currentVolume = onStairs ? stairsFootstepVolume : defaultFootstepVolume;

        if (isSprinting && sprintAudioSource != null)
        {
            sprintAudioSource.pitch = sprintPitch;
            sprintAudioSource.PlayOneShot(currentClip, currentVolume);
            if (forceDebugMode) Debug.Log(onStairs ? "Бег - лестница" : "Бег - обычный шаг");
        }
        else if (walkAudioSource != null)
        {
            walkAudioSource.pitch = walkPitch;
            walkAudioSource.PlayOneShot(currentClip, currentVolume);
            if (forceDebugMode) Debug.Log(onStairs ? "Ходьба - лестница" : "Ходьба - обычный шаг");
        }
    }

    private void StopAllFootsteps()
    {
        if (walkAudioSource && walkAudioSource.isPlaying)
            walkAudioSource.Stop();

        if (sprintAudioSource && sprintAudioSource.isPlaying)
            sprintAudioSource.Stop();
    }

    public void SetDefaultVolume(float newVolume)
    {
        defaultFootstepVolume = Mathf.Clamp01(newVolume);
    }

    public void SetStairsVolume(float newVolume)
    {
        stairsFootstepVolume = Mathf.Clamp01(newVolume);
    }

    private void OnDestroy()
    {
        StopAllFootsteps();
    }

    // ====== Лестница ======
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stairs"))
        {
            onStairs = true;
            if (forceDebugMode) Debug.Log("Игрок вошёл на лестницу");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Stairs"))
        {
            onStairs = false;

            if (forceDebugMode) Debug.Log("Игрок покинул лестницу");

            if (isMoving)
            {
                StopAllFootsteps();
                PlayFootstep();
                stepTimer = 0f;
            }
        }
    }
}
