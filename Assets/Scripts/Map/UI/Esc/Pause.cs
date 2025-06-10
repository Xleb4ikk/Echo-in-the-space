using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem.Controls;

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
    
    // Добавьте это поле для контроля скриптов игрока
    [Header("Компоненты управления")]
    [SerializeField] private MonoBehaviour[] playerControlScripts; // Скрипты движения
    [SerializeField] private MonoBehaviour cameraControlScript;    // Скрипт камеры (отдельно)
    [SerializeField] private GameObject playerObject;              // Корневой объект игрока (опционально)
    
    // Добавьте эти поля, если у вас есть доступ к действиям Input System
    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions; // Ваш Input Action Asset
    
    private bool isSettingsOpen = false;
    
    // Используем простой флаг для сброса движения
    private bool needToResetMovement = false;
    
    // Ссылки на компоненты игрока (добавьте эти поля)
    [SerializeField] private Player playerMovement;
    [SerializeField] private PlayerCamera playerCamera;
    
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
        
        // Если не указаны ссылки на игрока, найдем их автоматически
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Player>();
            
        if (playerCamera == null)
            playerCamera = FindObjectOfType<PlayerCamera>();
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
        
        // Сброс залипания движения после паузы
        if (needToResetMovement)
        {
            needToResetMovement = false;
            
            // Проверяем и сбрасываем залипшие клавиши движения
            if (keyboard != null)
            {
                // Ничего не делаем здесь, просто ждем, пока пользователь 
                // сам нажмет клавиши движения заново
                Debug.Log("Движение сброшено - нажмите клавиши заново");
            }
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
        
        // Явно отключаем ВСЕ контроллеры
        DisableAllControls();
        
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
        
        // Блокируем управление игроком
        if (playerMovement != null)
            playerMovement.canMove = false;
            
        if (playerCamera != null)
            playerCamera.canMove = false;
    }
    
    public void CloseSettings()
    {
        // Восстанавливаем время
        Time.timeScale = 1f;
        
        // Останавливаем музыку паузы
        if (pauseMusicSource != null)
            pauseMusicSource.Stop();
        
        // Восстанавливаем управление игроком напрямую вместо корутин
        EnableAllControls();
        
        // Возвращаем управление игроку
        if (playerMovement != null)
        {
            playerMovement.ResetInput();
            playerMovement.canMove = true;
        }
        
        if (playerCamera != null)
        {
            playerCamera.ResetInput();
            playerCamera.canMove = true;
        }
        
        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Скрываем панели в последнюю очередь
        if (blurPanel != null)
            blurPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        isSettingsOpen = false;
    }
    
    private void DisableAllControls()
    {
        // Отключаем все указанные скрипты движения
        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = false;
        }
        
        // Отдельно отключаем камеру
        if (cameraControlScript != null)
            cameraControlScript.enabled = false;
    }
    
    private void EnableAllControls()
    {
        // Включаем все указанные скрипты движения
        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = true;
        }
        
        // Отдельно включаем камеру
        if (cameraControlScript != null)
            cameraControlScript.enabled = true;
    }
    
    public void ReturnToMainMenu()
    {
        // Убедитесь, что timeScale сброшен перед загрузкой новой сцены
        Time.timeScale = 1f;
        // Загрузка сцены главного меню
        SceneManager.LoadScene("MainMenu");
    }
}
