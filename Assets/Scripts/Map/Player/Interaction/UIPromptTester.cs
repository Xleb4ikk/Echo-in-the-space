using UnityEngine;

/// <summary>
/// Скрипт для тестирования системы подсказок.
/// Добавьте этот компонент на любой объект и нажимайте кнопки в инспекторе для тестирования.
/// </summary>
public class UIPromptTester : MonoBehaviour
{
    [Header("Тестовые сообщения")]
    public string testMessage1 = "Нажмите E чтобы подобрать фонарик";
    public string testMessage2 = "Нажмите F для включения фонарика";
    
    [Header("Автотест")]
    public bool runAutomaticTest = false;
    public float messageDisplayTime = 3.0f; // Время отображения каждого сообщения
    
    private float testTimer = 0;
    private int testPhase = 0;
    private bool testRunning = false;
    
    private void Start()
    {
        if (UIPromptManager.Instance == null)
        {
            Debug.LogError("UIPromptManager не найден в сцене! Автотест не будет выполнен.");
            return;
        }
        
        if (runAutomaticTest)
        {
            StartAutoTest();
        }
    }
    
    public void StartAutoTest()
    {
        testRunning = true;
        testPhase = 0;
        testTimer = 0;
        
        Debug.Log("Запущен автотест системы подсказок...");
    }
    
    private void Update()
    {
        // Тестирование клавишами
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowTestMessage(testMessage1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowTestMessage(testMessage2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            HideMessage();
        }
        
        // Автоматическое тестирование
        if (testRunning)
        {
            testTimer += Time.deltaTime;
            
            if (testPhase == 0 && testTimer >= 1.0f)
            {
                // Фаза 1: показываем первое сообщение
                ShowTestMessage(testMessage1);
                testPhase = 1;
                testTimer = 0;
            }
            else if (testPhase == 1 && testTimer >= messageDisplayTime)
            {
                // Фаза 2: скрываем сообщение
                HideMessage();
                testPhase = 2;
                testTimer = 0;
            }
            else if (testPhase == 2 && testTimer >= 1.0f)
            {
                // Фаза 3: показываем второе сообщение
                ShowTestMessage(testMessage2);
                testPhase = 3;
                testTimer = 0;
            }
            else if (testPhase == 3 && testTimer >= messageDisplayTime)
            {
                // Фаза 4: скрываем сообщение и завершаем тест
                HideMessage();
                testPhase = 4;
                testTimer = 0;
                testRunning = false;
                Debug.Log("Автотест системы подсказок завершен!");
            }
        }
    }
    
    public void ShowTestMessage1()
    {
        ShowTestMessage(testMessage1);
    }
    
    public void ShowTestMessage2()
    {
        ShowTestMessage(testMessage2);
    }
    
    private void ShowTestMessage(string message)
    {
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.ShowPrompt(message);
            Debug.Log("Тест: показано сообщение - " + message);
        }
        else
        {
            Debug.LogError("UIPromptManager не найден! Невозможно показать сообщение.");
        }
    }
    
    public void HideMessage()
    {
        if (UIPromptManager.Instance != null)
        {
            UIPromptManager.Instance.HidePrompt();
            Debug.Log("Тест: скрыто текущее сообщение");
        }
    }
} 