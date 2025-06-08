using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Скрипт для автоматического создания UI системы отображения подсказок.
/// Добавьте этот скрипт на любой объект в сцене и нажмите кнопку "Создать UI Подсказок" в инспекторе.
/// </summary>
public class UIPromptGenerator : MonoBehaviour
{
    // Настройки UI
    public Color panelColor = new Color(0, 0, 0, 0.7f);
    public Color textColor = Color.white;
    public int fontSize = 28;
    public Vector2 panelSize = new Vector2(600, 80);
    public string defaultText = "Система подсказок готова";
    
    private Canvas uiCanvas;
    private GameObject promptPanel;
    private TextMeshProUGUI promptText;
    private UIPromptManager promptManager;
    
    // Метод для создания всех необходимых UI элементов
    public void GeneratePromptUI()
    {
        Debug.Log("Начинаю создание UI системы подсказок...");
        
        // Создаем Canvas, если его нет
        CreateCanvas();
        
        // Создаем панель для подсказок
        CreatePromptPanel();
        
        // Создаем текстовый элемент
        CreatePromptText();
        
        // Создаем и настраиваем менеджер подсказок
        SetupPromptManager();
        
        Debug.Log("Система UI подсказок создана успешно!");
    }
    
    private void CreateCanvas()
    {
        // Проверяем, есть ли уже Canvas в сцене
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null && existingCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiCanvas = existingCanvas;
            Debug.Log("Использую существующий Canvas: " + uiCanvas.gameObject.name);
            return;
        }
        
        // Если Canvas не найден, создаем новый
        GameObject canvasObj = new GameObject("UI Canvas");
        uiCanvas = canvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Добавляем необходимые компоненты для Canvas
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("Создан новый Canvas: " + canvasObj.name);
    }
    
    private void CreatePromptPanel()
    {
        // Создаем GameObject для панели подсказок
        promptPanel = new GameObject("PromptPanel");
        promptPanel.transform.SetParent(uiCanvas.transform, false);
        
        // Добавляем компоненты для панели
        Image panelImage = promptPanel.AddComponent<Image>();
        panelImage.color = panelColor;
        
        // Настраиваем RectTransform для панели
        RectTransform panelRect = promptPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = new Vector2(0, 100); // Отступ от нижнего края экрана
        
        // Изначально деактивируем панель
        promptPanel.SetActive(false);
        
        Debug.Log("Создана панель подсказок: " + promptPanel.name);
    }
    
    private void CreatePromptText()
    {
        // Создаем GameObject для текста подсказок
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptPanel.transform, false);
        
        // Добавляем TextMeshProUGUI компонент
        promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = defaultText;
        promptText.color = textColor;
        promptText.fontSize = fontSize;
        promptText.alignment = TextAlignmentOptions.Center;
        
        // Настраиваем RectTransform для текста
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 10); // Отступы от краев панели
        textRect.offsetMax = new Vector2(-10, -10);
        
        Debug.Log("Создан текст подсказок: " + textObj.name);
    }
    
    private void SetupPromptManager()
    {
        // Проверяем, есть ли уже UIPromptManager в сцене
        UIPromptManager existingManager = FindObjectOfType<UIPromptManager>();
        if (existingManager != null)
        {
            // Если менеджер уже есть, обновляем его настройки
            existingManager.SetPromptPanel(promptPanel);
            existingManager.SetPromptText(promptText);
            promptManager = existingManager;
            Debug.Log("Обновлены настройки существующего UIPromptManager: " + existingManager.gameObject.name);
        }
        else
        {
            // Если менеджера нет, создаем новый
            GameObject managerObj = new GameObject("UIManager");
            promptManager = managerObj.AddComponent<UIPromptManager>();
            promptManager.SetPromptPanel(promptPanel);
            promptManager.SetPromptText(promptText);
            Debug.Log("Создан новый UIPromptManager: " + managerObj.name);
        }
    }
} 