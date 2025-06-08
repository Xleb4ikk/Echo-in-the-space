using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFlashlight : MonoBehaviour
{
    public Light flashlight; // Перетащите сюда Spot Light в инспекторе
    public KeyCode toggleKey = KeyCode.F;
    [SerializeField] private AudioClip flashlightSound; // Звук фонарика
    private AudioSource audioSource;
    
    [Header("Настройки подбираемого фонарика")]
    [SerializeField] private bool hasFlashlight = false; // По умолчанию у игрока нет фонарика
    [SerializeField] private string useFlashlightPrompt = "Нажмите F для включения фонарика";
    [SerializeField] private float promptDisplayTime = 6f; // Увеличено время отображения подсказки
    [SerializeField] private bool showDebugMessages = true; // Отладочные сообщения
    
    private bool promptShown = false;
    private float promptTimer = 0f;
    
    // Свойство для доступа к статусу фонарика из других скриптов
    public bool HasFlashlight 
    { 
        get { return hasFlashlight; }
        set 
        { 
            // Если фонарик только что подобран
            if (value == true && hasFlashlight == false)
            {
                hasFlashlight = value;
                ShowUseFlashlightPrompt();
                DebugLog("Фонарик подобран, показываю подсказку: " + useFlashlightPrompt);
            }
            else
            {
                hasFlashlight = value;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Получаем или добавляем компонент AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Выключаем фонарик в начале игры
        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
        
        // Проверяем наличие UIPromptManager
        if (UIPromptManager.Instance == null)
        {
            DebugLog("ОШИБКА: UIPromptManager не найден! Подсказки не будут отображаться.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Проверяем, есть ли у игрока фонарик
        if (hasFlashlight && Keyboard.current.fKey.wasPressedThisFrame)
        {
            flashlight.enabled = !flashlight.enabled;
            // Воспроизводим звук при переключении
            if (flashlightSound != null)
            {
                audioSource.PlayOneShot(flashlightSound);
            }
            
            DebugLog("Фонарик " + (flashlight.enabled ? "включен" : "выключен"));
        }
        
        // Управление отображением подсказки
        if (promptShown)
        {
            promptTimer += Time.deltaTime;
            if (promptTimer >= promptDisplayTime)
            {
                HideUseFlashlightPrompt();
                promptShown = false;
                promptTimer = 0f;
                DebugLog("Скрываю подсказку автоматически по таймеру");
            }
        }
    }
    
    // Принудительно показать подсказку
    public void ForceShowPrompt()
    {
        if (hasFlashlight)
        {
            ShowUseFlashlightPrompt();
            DebugLog("Принудительно показываю подсказку: " + useFlashlightPrompt);
        }
    }
    
    private void ShowUseFlashlightPrompt()
    {
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowPrompt(useFlashlightPrompt);
            promptShown = true;
            promptTimer = 0f;
        }
        else
        {
            DebugLog("ОШИБКА: UIPromptManager не найден! Не могу показать подсказку: " + useFlashlightPrompt);
        }
    }
    
    private void HideUseFlashlightPrompt()
    {
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.HidePrompt();
        }
    }
    
    // Вспомогательный метод для отладочных сообщений
    private void DebugLog(string message)
    {
        if (showDebugMessages)
        {
            Debug.Log("[PlayerFlashlight] " + message);
        }
    }
    
    // Для вызова из редактора или других скриптов
    public void ResetFlashlightState()
    {
        hasFlashlight = false;
        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
        HideUseFlashlightPrompt();
        DebugLog("Сброшено состояние фонарика");
    }
}
