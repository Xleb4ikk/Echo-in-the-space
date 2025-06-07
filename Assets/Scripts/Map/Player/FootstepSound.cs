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

    // Ссылки на системы ввода
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Start()
    {
        // Создаем два отдельных источника звука для ходьбы и бега
        CreateAudioSources();
        
        if (forceDebugMode)
        {
            Debug.Log("FootstepSound: Инициализация завершена");
            Debug.Log($"Скорость звука при ходьбе: {walkPitch}, при беге: {sprintPitch}");
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
        // Проверяем нажатия клавиш
        CheckInput();
        
        // Управляем звуком шагов
        ManageFootsteps();
    }

    private void CheckInput()
    {
        // Считываем ввод из новой системы ввода
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        bool shiftPressed = inputActions.Player.Sprint.ReadValue<float>() > 0.5f;
        
        // Определяем, движется ли игрок
        isMoving = moveInput.magnitude > 0.1f;
        
        // Определяем, спринтует ли игрок
        isSprinting = isMoving && shiftPressed;
        
        // Выводим отладочные сообщения
        if (forceDebugMode && isMoving)
        {
            Debug.Log($"Движение: {isMoving}, Спринт: {isSprinting}, Input: {moveInput}");
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