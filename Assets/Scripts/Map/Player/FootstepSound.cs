using UnityEngine;
using UnityEngine.InputSystem;

public class FootstepSound : MonoBehaviour
{
    [Header("Звуки")]
    [SerializeField] private AudioClip footstepSound; // Звук шага
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f; // Громкость

    [Header("Настройки")]
    [SerializeField] private float walkPitch = 1.0f; // Скорость звука при ходьбе
    [SerializeField] private float sprintPitch = 1.7f; // Скорость звука при беге (на 70% быстрее)
    [SerializeField] private float stepInterval = 0.5f; // Интервал между шагами при ходьбе
    [SerializeField] private bool forceDebugMode = true; // Режим отладки принудительно включен

    // Ссылки на компоненты
    private AudioSource walkAudioSource; // Для звуков при ходьбе
    private AudioSource sprintAudioSource; // Для звуков при беге
    private bool isSprinting = false;
    private bool isMoving = false;
    private float stepTimer = 0f;

    // Ссылка на скрипт Player
    private Player playerScript;

    private void Start()
    {
        // Создаем два отдельных источника звука для ходьбы и бега
        CreateAudioSources();
        
        // Получаем ссылку на компонент Player
        playerScript = GetComponent<Player>();
        if (playerScript == null)
        {
            // Если компонент не на этом объекте, ищем его в родителе или верхнем объекте
            playerScript = GetComponentInParent<Player>();
            if (playerScript == null && transform.root != null)
            {
                playerScript = transform.root.GetComponentInChildren<Player>();
            }
        }
        
        if (forceDebugMode)
        {
            Debug.Log("FootstepSound: Инициализация завершена");
            Debug.Log($"Скорость звука при ходьбе: {walkPitch}, при беге: {sprintPitch}");
            if (playerScript == null)
                Debug.LogWarning("FootstepSound: Не найден компонент Player");
        }
    }

    private void CreateAudioSources()
    {
        // Создаем GameObject для звуков ходьбы
        GameObject walkAudioObj = new GameObject("WalkAudio");
        walkAudioObj.transform.parent = transform;
        walkAudioObj.transform.localPosition = Vector3.zero;
        walkAudioSource = walkAudioObj.AddComponent<AudioSource>();
        SetupAudioSource(walkAudioSource, walkPitch);

        // Создаем GameObject для звуков бега
        GameObject sprintAudioObj = new GameObject("SprintAudio");
        sprintAudioObj.transform.parent = transform;
        sprintAudioObj.transform.localPosition = Vector3.zero;
        sprintAudioSource = sprintAudioObj.AddComponent<AudioSource>();
        SetupAudioSource(sprintAudioSource, sprintPitch);
        
        if (forceDebugMode)
        {
            Debug.Log($"Созданы аудио-источники: walkPitch={walkPitch}, sprintPitch={sprintPitch}");
        }
    }

    private void SetupAudioSource(AudioSource source, float pitch)
    {
        source.clip = footstepSound;
        source.loop = false;
        source.playOnAwake = false;
        source.volume = volume;
        source.pitch = pitch;
    }

    private void Update()
    {
        // Проверяем состояние движения и спринта
        CheckMovementState();
        
        // Управляем звуком шагов
        ManageFootsteps();
    }

    private void CheckMovementState()
    {
        // Если не удалось найти скрипт Player, выходим
        if (playerScript == null)
        {
            isMoving = false;
            isSprinting = false;
            return;
        }
        
        // Получаем состояние движения напрямую из скрипта Player
        isMoving = playerScript.MoveInput.magnitude > 0.1f;
        
        // Получаем состояние спринта напрямую из скрипта Player
        // В скрипте Player уже учтена проверка стамины
        isSprinting = playerScript.IsSprinting;
        
        // Выводим отладочные сообщения
        if (forceDebugMode && isMoving)
        {
            Debug.Log($"Движение: {isMoving}, Спринт: {isSprinting}, Input: {playerScript.MoveInput}");
        }
    }

    private void ManageFootsteps()
    {
        // Если игрок не движется, останавливаем все звуки
        if (!isMoving)
        {
            StopAllFootsteps();
            stepTimer = 0f;
            return;
        }
        
        // Увеличиваем таймер
        stepTimer += Time.deltaTime;
        
        // Определяем интервал между шагами в зависимости от спринта
        float currentInterval = isSprinting ? stepInterval / (sprintPitch / walkPitch) : stepInterval;
        
        // Если прошло достаточно времени для следующего шага
        if (stepTimer >= currentInterval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        // Останавливаем предыдущие звуки
        walkAudioSource.Stop();
        sprintAudioSource.Stop();
        
        // Воспроизводим соответствующий звук в зависимости от спринта
        if (isSprinting)
        {
            sprintAudioSource.PlayOneShot(footstepSound);
            if (forceDebugMode) Debug.Log($"Воспроизведение звука бега, pitch: {sprintPitch}");
        }
        else
        {
            walkAudioSource.PlayOneShot(footstepSound);
            if (forceDebugMode) Debug.Log($"Воспроизведение звука ходьбы, pitch: {walkPitch}");
        }
    }

    private void StopAllFootsteps()
    {
        if (walkAudioSource && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
        
        if (sprintAudioSource && sprintAudioSource.isPlaying)
        {
            sprintAudioSource.Stop();
        }
    }

    // Публичный метод для изменения громкости
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        
        if (walkAudioSource)
        {
            walkAudioSource.volume = volume;
        }
        
        if (sprintAudioSource)
        {
            sprintAudioSource.volume = volume;
        }
    }

    // При уничтожении объекта
    private void OnDestroy()
    {
        StopAllFootsteps();
    }
} 