using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public DialogueStarter dialogueStarter;
    public bool isOneTimeOnly = false; // Флаг для одноразового триггера
    private bool hasTriggered = false; // Отслеживание, был ли триггер уже активирован

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Убедись, что у игрока есть тег "Player"
        {
            if (!isOneTimeOnly || !hasTriggered) // Проверяем, можно ли запустить триггер
            {
                dialogueStarter.ActivateDialogue();
                
                if (isOneTimeOnly)
                {
                    hasTriggered = true; // Отмечаем триггер как использованный
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) // Для 3D
    {
        if (other.CompareTag("Player"))
        {
            if (!isOneTimeOnly || !hasTriggered) // Проверяем, можно ли запустить триггер
            {
                dialogueStarter.ActivateDialogue();
                
                if (isOneTimeOnly)
                {
                    hasTriggered = true; // Отмечаем триггер как использованный
                }
            }
        }
    }
}