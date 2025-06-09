using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    
    // Аудио
    [SerializeField] private AudioClip pauseMusicClip;
    private AudioSource pauseMusicSource;
    
    // Добавим ссылку на компоненты управления камерой
    [SerializeField] private MonoBehaviour[] cameraControlScripts;
    
    [SerializeField] private GameObject blurPanel;
    
    // Добавьте это поле для эффекта расфокуса
    [SerializeField] private Volume depthOfFieldVolume;
    
    private bool isSettingsOpen = false;
    
    private void Awake()
    {
        // Создаем источник звука для паузы
        pauseMusicSource = gameObject.AddComponent<AudioSource>();
        pauseMusicSource.clip = pauseMusicClip;
        pauseMusicSource.loop = true;
        pauseMusicSource.playOnAwake = false;
    }
    
    private void Start()
    {
        // Скрываем ОБЕ панели при старте игры
        if (blurPanel != null)
            blurPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Настройка обработчиков событий
        continueButton.onClick.AddListener(CloseSettings);
        exitButton.onClick.AddListener(ReturnToMainMenu);
        
        // Если не заданы скрипты управления камерой, попробуем найти их автоматически
        if (cameraControlScripts == null || cameraControlScripts.Length == 0)
        {
            // Попытка найти типичные скрипты управления камерой
            cameraControlScripts = FindObjectsOfType<MonoBehaviour>();
        }
    }
    
    private void Update()
    {
        // Проверка нажатия клавиши Esc
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (isSettingsOpen)
                CloseSettings();
            else
                OpenSettings();
        }
    }
    
    public void OpenSettings()
    {
        // Активируем ОБЕ панели
        if (blurPanel != null)
            blurPanel.SetActive(true);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        
        isSettingsOpen = true;
        
        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Останавливаем время
        Time.timeScale = 0f;
        
        // Отключаем скрипты управления камерой
        DisableCameraControls();
        
        // Останавливаем все аудио кроме музыки паузы
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in allAudio)
        {
            if (audio != pauseMusicSource && audio.isPlaying)
                audio.Pause();
        }
        
        // Запускаем музыку паузы
        if (pauseMusicClip != null)
            pauseMusicSource.Play();
        
        // Включаем эффект расфокуса
        if (depthOfFieldVolume != null)
        {
            depthOfFieldVolume.weight = 1.0f;
        }
    }
    
    public void CloseSettings()
    {
        // Деактивируем ОБЕ панели
        if (blurPanel != null)
            blurPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        isSettingsOpen = false;
        
        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Восстанавливаем время
        Time.timeScale = 1f;
        
        // Включаем скрипты управления камерой
        EnableCameraControls();
        
        // Останавливаем музыку паузы
        pauseMusicSource.Stop();
        
        // Возобновляем остальное аудио
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in allAudio)
        {
            if (audio != pauseMusicSource && !audio.isPlaying)
                audio.UnPause();
        }
        
        // Отключаем эффект расфокуса
        if (depthOfFieldVolume != null)
        {
            depthOfFieldVolume.weight = 0f;
        }
    }
    
    private void DisableCameraControls()
    {
        // Просто пытаемся отключить все скрипты, которые могут управлять камерой
        foreach (var script in cameraControlScripts)
        {
            if (script != null && script.enabled)
            {
                script.enabled = false;
            }
        }
    }
    
    private void EnableCameraControls()
    {
        // Включаем все скрипты управления камерой
        foreach (var script in cameraControlScripts)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }
    }
    
    public void ReturnToMainMenu()
    {
        // Убедитесь, что timeScale сброшен перед загрузкой новой сцены
        Time.timeScale = 1f;
        // Загрузка сцены главного меню
        SceneManager.LoadScene("MainMenu");
    }
}