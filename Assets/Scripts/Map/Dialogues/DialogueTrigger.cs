using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;
    public string speakerName;
    public Color speakerNameColor = Color.yellow;
    public AudioClip typingSound; // Индивидуальный звук для каждой реплики
    [Range(0.1f, 2.0f)]
    public float typingSpeed = 1.0f; // Индивидуальная скорость печатания
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("Ссылки на UI элементы")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text speakerNameText;

    [Header("Настройки диалога")]
    public DialogueLine[] dialogueLines;
    public float defaultTypingDelay = 0.05f; // Базовая задержка печатания
    public float activationDelay = 0f;

    [Header("Параметры игрока")]
    public GameObject playerObject; // Ссылка на объект игрока
    public string playerTag = "Player"; // Тег игрока

    [Header("Настройки триггера")]
    public bool activateOnStart = false; // Флаг для активации при старте
    public bool activateOnTrigger = true; // Флаг для активации при входе в триггер
    public bool debugMode = true; // Режим отладки для вывода сообщений

    private AudioSource audioSource;
    private bool isDialogueActive = false;
    private int currentLine = 0;
    private string currentText = "";
    private bool isTyping = false;
    private bool hasTriggered = false; // Флаг, показывающий, был ли уже активирован триггер

    void Awake()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Awake: " + gameObject.name);
    }

    void Start()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Start: " + gameObject.name);

        // Проверка наличия необходимых компонентов
        if (dialoguePanel == null)
        {
            Debug.LogError("[DialogueTrigger] Отсутствует ссылка на панель диалога в " + gameObject.name);
            return;
        }
        if (dialogueText == null)
        {
            Debug.LogError("[DialogueTrigger] Отсутствует ссылка на текст диалога в " + gameObject.name);
            return;
        }
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogError("[DialogueTrigger] Отсутствуют строки диалога в " + gameObject.name);
            return;
        }

        // Получаем или добавляем AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;
        }

        // Скрываем панель диалога при старте
        if (dialoguePanel.activeSelf)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Скрываем панель диалога при старте");
            dialoguePanel.SetActive(false);
        }
        
        // Активируем диалог при старте только если установлен флаг activateOnStart
        if (activateOnStart)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Запланирована активация диалога через " + activationDelay + " секунд");
            Invoke("ActivateDialogue", activationDelay);
        }

        // Проверяем настройки триггера для 3D
        if (activateOnTrigger)
        {
            Collider collider = GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogWarning("[DialogueTrigger] Активация по триггеру включена, но отсутствует компонент Collider на " + gameObject.name);
            }
            else if (!collider.isTrigger)
            {
                Debug.LogWarning("[DialogueTrigger] Активация по триггеру включена, но у коллайдера не установлен флаг isTrigger на " + gameObject.name);
            }
        }
    }

    // Обработка столкновений - OnTriggerEnter для 3D
    void OnTriggerEnter(Collider other)
    {
        if (debugMode) Debug.Log("[DialogueTrigger] OnTriggerEnter с объектом: " + other.gameObject.name);
        
        CheckAndActivate(other.gameObject);
    }

    // Дополнительная обработка - OnCollisionEnter для 3D
    void OnCollisionEnter(Collision collision)
    {
        if (debugMode) Debug.Log("[DialogueTrigger] OnCollisionEnter с объектом: " + collision.gameObject.name);
        
        CheckAndActivate(collision.gameObject);
    }

    // Обработка ввода для переключения диалога
    void Update()
    {
        if (!isDialogueActive) return;
        
        // Проверка нажатия пробела с использованием нового Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Нажат пробел");
            NextLine();
        }
        
        // Проверка нажатия левой кнопки мыши с использованием нового Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Нажата левая кнопка мыши");
            NextLine();
        }
    }

    // Проверка объекта и активация диалога
    private void CheckAndActivate(GameObject obj)
    {
        // Проверяем, что активация по триггеру включена
        if (!activateOnTrigger) 
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Активация по триггеру отключена");
            return;
        }
        
        // Если триггер уже сработал, не активируем повторно
        if (hasTriggered)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Триггер уже был активирован ранее");
            return;
        }
        
        // Если playerObject не назначен, проверяем по тегу Player
        if (playerObject == null)
        {
            if (obj.CompareTag(playerTag))
            {
                if (debugMode) Debug.Log("[DialogueTrigger] Игрок (по тегу) вошел в триггер");
                hasTriggered = true;
                ActivateDialogue();
            }
        }
        else if (obj == playerObject)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Игрок (по объекту) вошел в триггер");
            hasTriggered = true;
            ActivateDialogue();
        }
    }

    // Публичный метод для активации диалога извне
    public void ActivateDialogue()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Вызван метод ActivateDialogue()");
        
        if (isDialogueActive)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Диалог уже активен");
            return;
        }
        
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogError("[DialogueTrigger] Нет строк диалога для отображения");
            return;
        }

        if (debugMode) Debug.Log("[DialogueTrigger] Активация диалога");
        
        isDialogueActive = true;
        currentLine = 0;
        
        // Активируем панель диалога
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            if (debugMode) Debug.Log("[DialogueTrigger] Панель диалога активирована");
        }
        else
        {
            Debug.LogError("[DialogueTrigger] Отсутствует ссылка на панель диалога");
            return;
        }

        ShowCurrentDialogueLine();
    }

    private void ShowCurrentDialogueLine()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Показываем строку диалога #" + currentLine);
        
        DialogueLine line = dialogueLines[currentLine];
        
        // Устанавливаем имя говорящего, если оно указано
        if (speakerNameText != null && !string.IsNullOrEmpty(line.speakerName))
        {
            speakerNameText.text = line.speakerName;
            speakerNameText.color = line.speakerNameColor;
            if (debugMode) Debug.Log("[DialogueTrigger] Установлено имя говорящего: " + line.speakerName);
        }
        
        StartDialogue(line);
    }

    private void StartDialogue(DialogueLine line)
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Запуск анимации печатания текста");
        
        StopAllCoroutines();
        StartCoroutine(TypeText(line));
    }

    private IEnumerator TypeText(DialogueLine line)
    {
        isTyping = true;
        dialogueText.text = "";
        currentText = "";
        
        float actualDelay = defaultTypingDelay / line.typingSpeed;
        
        if (debugMode) Debug.Log("[DialogueTrigger] Начало печатания текста: " + line.text);
        
        foreach (char letter in line.text.ToCharArray())
        {
            currentText += letter;
            dialogueText.text = currentText;
            
            // Воспроизводим звук печатания, если он есть и буква не пробел
            if (audioSource != null && line.typingSound != null && letter != ' ')
            {
                audioSource.PlayOneShot(line.typingSound);
            }
            
            yield return new WaitForSeconds(actualDelay);
        }
        
        if (debugMode) Debug.Log("[DialogueTrigger] Завершено печатание текста");
        isTyping = false;
    }

    public void NextLine()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Вызван метод NextLine()");
        
        // Если текст еще печатается, показываем его сразу полностью
        if (isTyping)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Текст еще печатается, показываем полностью");
            
            StopAllCoroutines();
            dialogueText.text = dialogueLines[currentLine].text;
            isTyping = false;
            return;
        }

        currentLine++;
        
        if (currentLine < dialogueLines.Length)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Переход к следующей строке: " + currentLine);
            ShowCurrentDialogueLine();
        }
        else
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Достигнут конец диалога");
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Завершение диалога");
        
        isDialogueActive = false;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
} 