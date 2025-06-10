using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPromptManager : MonoBehaviour
{
    public static UIPromptManager Instance { get; private set; }
    
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Проверяем, является ли объект корневым в иерархии
            if (transform.parent != null)
            {
                // Если объект не корневой, создаем новый корневой объект
                Debug.Log("UIPromptManager: Преобразование в корневой объект для DontDestroyOnLoad");
                transform.SetParent(null);
            }
            
            // Теперь можно безопасно вызвать DontDestroyOnLoad
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Скрываем подсказку при старте
        HidePrompt();
    }
    
    // Проверяем, готова ли система UI
    private bool IsReady()
    {
        return promptPanel != null && promptText != null;
    }
    
    // Установка ссылки на панель
    public void SetPromptPanel(GameObject panel)
    {
        promptPanel = panel;
        Debug.Log("Установлена панель для подсказок: " + (panel != null ? panel.name : "null"));
    }
    
    // Установка ссылки на текст
    public void SetPromptText(TextMeshProUGUI text)
    {
        promptText = text;
        Debug.Log("Установлен текст для подсказок: " + (text != null ? text.name : "null"));
    }
    
    public void ShowPrompt(string message)
    {
        if (!IsReady())
        {
            Debug.LogWarning("UIPromptManager: Не найдены компоненты UI для отображения подсказки!");
            return;
        }
        
        if (promptPanel != null && promptText != null)
        {
            promptText.text = message;
            promptPanel.SetActive(true);
            Debug.Log("Показана подсказка: " + message);
        }
    }
    
    public void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
} 