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
    public float defaultTypingDelay = 0.05f;
    public float activationDelay = 0f;

    [Header("Аудио")]
    public AudioSource audioSource; // <- вручную настраиваемый источник звука

    [Header("Параметры игрока")]
    public GameObject playerObject;
    public string playerTag = "Player";

    [Header("Настройки триггера")]
    public bool activateOnStart = false;
    public bool activateOnTrigger = true;
    public bool debugMode = true;

    private bool isDialogueActive = false;
    private int currentLine = 0;
    private string currentText = "";
    private bool isTyping = false;
    private bool hasTriggered = false;

    void Awake()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Awake: " + gameObject.name);
    }

    void Start()
    {
        if (debugMode) Debug.Log("[DialogueTrigger] Start: " + gameObject.name);

        if (dialoguePanel == null)
        {
            Debug.LogError("[DialogueTrigger] Отсутствует ссылка на панель диалога");
            return;
        }

        if (dialogueText == null)
        {
            Debug.LogError("[DialogueTrigger] Отсутствует ссылка на текст диалога");
            return;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogError("[DialogueTrigger] Нет строк диалога");
            return;
        }

        // Попробуем найти AudioSource, если не установлен вручную
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("[DialogueTrigger] AudioSource не найден и не установлен вручную.");
            }
        }

        if (dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(false);
        }

        if (activateOnStart)
        {
            Invoke("ActivateDialogue", activationDelay);
        }

        if (activateOnTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogWarning("[DialogueTrigger] Нет Collider для триггера");
            }
            else if (!col.isTrigger)
            {
                Debug.LogWarning("[DialogueTrigger] Collider не помечен как триггер");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (debugMode) Debug.Log("[DialogueTrigger] OnTriggerEnter: " + other.gameObject.name);
        CheckAndActivate(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (debugMode) Debug.Log("[DialogueTrigger] OnCollisionEnter: " + collision.gameObject.name);
        CheckAndActivate(collision.gameObject);
    }

    void Update()
    {
        if (!isDialogueActive) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Нажат пробел");
            NextLine();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Нажата левая кнопка мыши");
            NextLine();
        }
    }

    private void CheckAndActivate(GameObject obj)
    {
        if (!activateOnTrigger || hasTriggered) return;

        if (playerObject == null && obj.CompareTag(playerTag))
        {
            hasTriggered = true;
            ActivateDialogue();
        }
        else if (obj == playerObject)
        {
            hasTriggered = true;
            ActivateDialogue();
        }
    }

    public void ActivateDialogue()
    {
        if (isDialogueActive) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        isDialogueActive = true;
        currentLine = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowCurrentDialogueLine();
    }

    private void ShowCurrentDialogueLine()
    {
        DialogueLine line = dialogueLines[currentLine];

        if (speakerNameText != null && !string.IsNullOrEmpty(line.speakerName))
        {
            speakerNameText.text = line.speakerName;
            speakerNameText.color = line.speakerNameColor;
        }

        StartDialogue(line);
    }

    private void StartDialogue(DialogueLine line)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(line));
    }

    private IEnumerator TypeText(DialogueLine line)
    {
        isTyping = true;
        dialogueText.text = "";
        currentText = "";

        float delay = defaultTypingDelay / line.typingSpeed;

        foreach (char letter in line.text)
        {
            currentText += letter;
            dialogueText.text = currentText;

            if (audioSource != null && line.typingSound != null && letter != ' ')
            {
                audioSource.PlayOneShot(line.typingSound);
            }

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
    }

    public void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = dialogueLines[currentLine].text;
            isTyping = false;
            return;
        }

        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            ShowCurrentDialogueLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}
