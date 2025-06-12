using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text dialogueText;  // Ссылка на текст диалога
    public float delay = 0.05f;    // Скорость появления букв

    private string fullText;       // Полный текст
    private string currentText = ""; // Текущий отображаемый текст
    private int currentLine = 0;   // Текущая реплика
    public string[] dialogueLines; // Массив диалогов

    void Update()
    {
        // Проверяем левый клик мыши с помощью Input System
        if (Mouse.current.leftButton.wasPressedThisFrame && !IsTyping())
        {
            NextDialogue();
        }
    }

    public void Start() // Убедимся, что метод публичный
    {
        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            StartDialogue(dialogueLines[0]); // Запускаем первый диалог
        }
    }

    public void StartDialogue(string text)
    {
        fullText = text;
        currentText = "";
        StartCoroutine(ShowText());
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
        }
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
        // Проверяем, идет ли анимация текста
        return !string.IsNullOrEmpty(fullText) && currentText != fullText;
    }
}