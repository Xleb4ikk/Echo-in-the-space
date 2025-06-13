using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableFlashlight : MonoBehaviour
{
    [SerializeField] private GameObject footstepGhostPrefab;
    [Header("Настройки взаимодействия")]
    [SerializeField] private float interactionDistance = 2.0f;
    [SerializeField] private string interactionPrompt = "Нажмите E чтобы подобрать фонарик";
    
    [Header("Визуальные эффекты")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightIntensity = 1.0f;

    [Header("Звуки")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Отладка")]
    [SerializeField] private bool showDebugMessages = true;
    [SerializeField] private float delayBeforeUsePrompt = 0.2f; // Задержка перед показом подсказки использования
    
    private PlayerFlashlight playerFlashlight;
    private GameObject player;
    private Camera mainCamera;
    private Light highlightLight;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] highlightMaterials;
    private bool playerInRange = false;
    
    private void Start()
    {
        // Находим игрока
        player = GameObject.FindGameObjectWithTag("Player");
        mainCamera = Camera.main;
        
        if (player != null)
        {
            playerFlashlight = player.GetComponentInChildren<PlayerFlashlight>();
            if (playerFlashlight == null)
            {
                DebugLog("PlayerFlashlight компонент не найден на игроке!");
            }
        }
        else
        {
            DebugLog("Игрок с тегом 'Player' не найден!");
        }
        
        // Настраиваем подсветку
        SetupHighlight();
        
        // Убедимся, что UI-система готова
        if (UIPromptManager.Instance == null)
        {
            DebugLog("ВНИМАНИЕ: UIPromptManager не найден в сцене! Создайте объект с этим компонентом.");
        }
    }
    
    private void SetupHighlight()
    {
        // Создаем свет для подсветки
        GameObject lightObj = new GameObject("HighlightLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        highlightLight = lightObj.AddComponent<Light>();
        highlightLight.type = LightType.Point;
        highlightLight.color = highlightColor;
        highlightLight.intensity = highlightIntensity;
        highlightLight.range = 0.5f;
        highlightLight.enabled = false;
        
        // Подготавливаем материалы для подсветки
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        highlightMaterials = new Material[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            // Сохраняем оригинальный материал
            originalMaterials[i] = renderers[i].material;
            
            // Создаем материал для подсветки
            highlightMaterials[i] = new Material(originalMaterials[i]);
            highlightMaterials[i].EnableKeyword("_EMISSION");
            highlightMaterials[i].SetColor("_EmissionColor", highlightColor * 0.5f);
        }
    }
    
    private void Update()
    {
        if (player == null || mainCamera == null) return;
        
        // Проверяем расстояние до игрока
        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool isInRange = distance <= interactionDistance;
        
        // Реагируем на изменение состояния
        if (isInRange != playerInRange)
        {
            playerInRange = isInRange;
            ShowHighlight(playerInRange);
            ShowPrompt(playerInRange);
            
            if (playerInRange)
            {
                DebugLog("Игрок вошел в зону взаимодействия с фонариком");
            }
            else
            {
                DebugLog("Игрок вышел из зоны взаимодействия с фонариком");
            }
        }
        
        // Обрабатываем подбор, если игрок в зоне и нажал E
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickupFlashlight();
        }
    }
    
    private void ShowHighlight(bool show)
    {
        // Включаем/выключаем свет
        if (highlightLight != null)
        {
            highlightLight.enabled = show;
        }
        
        // Меняем материалы
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = show ? highlightMaterials[i] : originalMaterials[i];
        }
    }
    
    private void ShowPrompt(bool show)
    {
        if (UIPromptManager.Instance != null)
        {
            if (show)
            {
                UIPromptManager.Instance.ShowPrompt(interactionPrompt);
                DebugLog("Показываю подсказку: " + interactionPrompt);
            }
            else
            {
                UIPromptManager.Instance.HidePrompt();
                DebugLog("Скрываю подсказку");
            }
        }
        else
        {
            DebugLog("ОШИБКА: UIPromptManager не найден! Текстовая подсказка не будет отображаться.");
            
            // Запасной вариант - выводим в консоль
            if (show)
            {
                DebugLog("Подсказка: " + interactionPrompt);
            }
        }
    }
    
        private void PickupFlashlight()
        {
            if (playerFlashlight != null)
            {
                // Убираем подсказку взаимодействия
                ShowPrompt(false);

                // Включаем возможность использовать фонарик
                playerFlashlight.HasFlashlight = true;

                // Воспроизводим звук подбора
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1.0f);
                }

                // Дополнительно вызываем подсказку с небольшой задержкой
                Invoke("ShowUseFlashlightPromptDelayed", delayBeforeUsePrompt);

                // Выводим сообщение в консоль для отладки
                DebugLog("Фонарик подобран!");

                // Движение шагов сзади игрока
                Vector3 startPos = new Vector3(4.5f, 1.5f, -97f);

                GameObject footstepGhost = Instantiate(
                    footstepGhostPrefab,
                    startPos,
                    Quaternion.identity
                );

                FootstepGhost script = footstepGhost.GetComponent<FootstepGhost>();

                // Задаём путь: сначала движение по X, потом по Z
                script.pathPoints = new Vector3[]
                {
                    new Vector3(14f, 1.5f, -97f),   // движение по X (с 4.5 до 14)
                    new Vector3(14f, 1.5f, -105f)   // движение по Z (с -97 до -105)
                };

                script.Initialize(player.transform);

                // Уничтожаем объект фонарика
                Destroy(gameObject);
            }
            else
            {
                DebugLog("ОШИБКА: Невозможно передать фонарик игроку: компонент PlayerFlashlight не найден");
            }
        
    }

    
    // Метод для задержанного показа подсказки использования
    private void ShowUseFlashlightPromptDelayed()
    {
        if (playerFlashlight != null)
        {
            // Принудительно показываем подсказку использования
            playerFlashlight.ForceShowPrompt();
            DebugLog("Вызываю отложенную подсказку использования фонарика");
        }
    }
    
    // Вспомогательный метод для отладочных сообщений
    private void DebugLog(string message)
    {
        if (showDebugMessages)
        {
            Debug.Log("[Фонарик] " + message);
        }
    }
    
    // Визуализация зоны взаимодействия в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
    
    // Визуализация путем триггера (чтобы лучше видеть зону взаимодействия)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DebugLog("Игрок вошел в триггерную зону фонарика");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DebugLog("Игрок вышел из триггерной зоны фонарика");
        }
    }
} 
