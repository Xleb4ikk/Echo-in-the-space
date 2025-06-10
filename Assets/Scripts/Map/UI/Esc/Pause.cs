using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    
    // Аудио
    [SerializeField] private AudioClip pauseMusicClip;
    [SerializeField] private AudioClip buttonHoverSound;  // Звук при наведении
    [SerializeField] private AudioClip buttonClickSound;  // Звук при клике
    
    [SerializeField] private float buttonSoundVolume = 0.5f;
    
    private AudioSource pauseMusicSource;
    private AudioSource buttonSoundSource;  // Отдельный источник для звуков кнопок
    
    // Добавим ссылку на компоненты управления камерой
    [SerializeField] private MonoBehaviour[] cameraControlScripts;
    
    [SerializeField] private GameObject blurPanel;
    
    // Добавьте это поле для эффекта расфокуса
    [SerializeField] private Volume depthOfFieldVolume;
    
    private bool isSettingsOpen = false;
    
    private void Awake()
    {
        // Создаем источники звука
        pauseMusicSource = gameObject.AddComponent<AudioSource>();
        pauseMusicSource.clip = pauseMusicClip;
        pauseMusicSource.loop = true;
        pauseMusicSource.playOnAwake = false;
        
        // Источник для звуков кнопок
        buttonSoundSource = gameObject.AddComponent<AudioSource>();
        buttonSoundSource.playOnAwake = false;
    }
    
    private void Start()
    {
        // Скрываем ОБЕ панели при старте игры
        if (blurPanel != null)
            blurPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Настройка обработчиков событий
        SetupButtonSounds();
        
        continueButton.onClick.AddListener(CloseSettings);
        exitButton.onClick.AddListener(ReturnToMainMenu);
        
        // Если не заданы скрипты управления камерой, попробуем найти их автоматически
        if (cameraControlScripts == null || cameraControlScripts.Length == 0)
        {
            // Попытка найти типичные скрипты управления камерой
            cameraControlScripts = FindObjectsOfType<MonoBehaviour>();
        }
    }
    
    private void SetupButtonSounds()
    {
        // Находим все кнопки в меню паузы
        Button[] allButtons = settingsPanel.GetComponentsInChildren<Button>(true);
        
        foreach (Button button in allButtons)
        {
            // Добавляем обработчик для звука клика
            button.onClick.AddListener(() => PlayButtonClickSound());
            
            // Добавляем EventTrigger для звука наведения
            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            
            // Создаем новую запись для события наведения
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            
            // Добавляем обработчик события
            entry.callback.AddListener((data) => { PlayButtonHoverSound(); });
            
            // Добавляем запись в триггер
            eventTrigger.triggers.Add(entry);
        }
    }
    
    private void PlayButtonHoverSound()
    {
        if (buttonHoverSound != null && buttonSoundSource != null)
        {
            buttonSoundSource.PlayOneShot(buttonHoverSound, buttonSoundVolume);
        }
    }
    
    private void PlayButtonClickSound()
    {
        if (buttonClickSound != null && buttonSoundSource != null)
        {
            buttonSoundSource.PlayOneShot(buttonClickSound, buttonSoundVolume);
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
