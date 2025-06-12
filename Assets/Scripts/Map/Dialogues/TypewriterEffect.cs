using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text dialogueText;  // Ссылка на текст диалога
    public float delay = 0.05f;    // Скорость появления букв
    public GameObject dialoguePanel; // Панель диалогового окна

    private string fullText;       // Полный текст
    private string currentText = ""; // Текущий отображаемый текст
    private int currentLine = 0;   // Текущая реплика
    public string[] dialogueLines; // Массив диалогов
    private bool isActive = false; // Флаг активности диалога
    private Coroutine typingCoroutine; // Ссылка на корутину печатания

    void Update()
    {
        // Проверяем левый клик мыши с помощью Input System
        if (isActive && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsTyping())
            {
                // Если текст печатается, показываем его полностью при нажатии
                SkipTyping();
            }
            else
            {
                // Если печатание завершено, переходим к следующему диалогу
                NextDialogue();
            }
        }
    }

    public void Start()
    {
        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            dialogueText.text = ""; // Очищаем текст при старте
        }
        
        // Если не задана ссылка на диалоговую панель, пробуем найти её автоматически 
        // (предполагая, что скрипт находится на родительском объекте диалоговой панели)
        if (dialoguePanel == null)
        {
            dialoguePanel = gameObject;
        }
        
        // Скрываем диалоговую панель при старте
        HideDialoguePanel();
    }

    // Показать диалоговую панель
    private void ShowDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    // Скрыть диалоговую панель
    private void HideDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ActivateDialogue()
    {
        if (!isActive && dialogueLines != null && dialogueLines.Length > 0)
        {
            isActive = true;
            ShowDialoguePanel(); // Показываем диалоговую панель
            StartDialogue(dialogueLines[0]); // Запускаем первый диалог
        }
    }

    public void StartDialogue(string text)
    {
        fullText = text;
        currentText = "";
        
        // Останавливаем предыдущую корутину, если она еще активна
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(ShowText());
    }

    public void NextDialogue()
    {
        currentLine++;
        if (currentLine < dialogueLines.Length)
        {
            StartDialogue(dialogueLines[currentLine]);
        }
        else
        {
            dialogueText.text = ""; // Очищаем текст, если диалоги закончились
            isActive = false; // Деактивируем после последнего диалога
            HideDialoguePanel(); // Скрываем диалоговую панель
        }
    }

    // Метод для мгновенного отображения всего текста
    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        currentText = fullText;
        dialogueText.text = fullText;
    }

    IEnumerator ShowText()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            dialogueText.text = currentText;
            yield return new WaitForSeconds(delay);
        }
    }

    private bool IsTyping()
    {
        return !string.IsNullOrEmpty(fullText) && currentText != fullText;
    }
}