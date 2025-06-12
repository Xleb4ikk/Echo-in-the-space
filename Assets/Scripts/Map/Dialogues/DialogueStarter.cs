using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    public TypewriterEffect typewriter;

    // Массив диалогов
    public string[] dialogueLines = new string[]
    {
        "Закрыл его. Температура вот-вот упадет.",
        "Ты ведь не хочешь сидеть и дрожать во время шторма.",
        "Время шторма."
    };

    // Опционально: время активации (в секундах)
    public float activationDelay = 0f; // 0 = сразу, или задай нужное время

    void Start()
    {
        typewriter.dialogueLines = dialogueLines; // Передаем массив

        if (activationDelay > 0)
        {
            Invoke("ActivateDialogue", activationDelay); // Активация через время
        }
    }

    // Метод для активации диалога (например, из триггера)
    public void ActivateDialogue()
    {
        typewriter.ActivateDialogue();
    }
}