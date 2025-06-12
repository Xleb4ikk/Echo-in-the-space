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

    void Start()
    {
        typewriter.dialogueLines = dialogueLines; // Передаем массив
        typewriter.Start(); // Вызов публичного метода
    }
}