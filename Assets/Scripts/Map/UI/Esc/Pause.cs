using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    
    // Дополнительные элементы настроек
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    
    private bool isSettingsOpen = false;
    
    private void Start()
    {
        // Скрыть панель настроек по умолчанию
        settingsPanel.SetActive(false);
        
        // Настройка обработчиков событий
        continueButton.onClick.AddListener(CloseSettings);
        exitButton.onClick.AddListener(ReturnToMainMenu);
        
        // Настройка элементов управления настройками
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }
    
    private void Update()
    {
        // Проверка нажатия клавиши Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isSettingsOpen)
                CloseSettings();
            else
                OpenSettings();
        }
    }
    
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        isSettingsOpen = true;
        // Опционально: поставить игру на паузу
        Time.timeScale = 0f;
    }
    
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        isSettingsOpen = false;
        // Опционально: снять игру с паузы
        Time.timeScale = 1f;
    }
    
    public void ReturnToMainMenu()
    {
        // Убедитесь, что timeScale сброшен перед загрузкой новой сцены
        Time.timeScale = 1f;
        // Загрузка сцены главного меню
        // Замените "MainMenu" на имя вашей сцены главного меню
        SceneManager.LoadScene("MainMenu");
    }
    
    private void SetVolume(float value)
    {
        // Установка громкости
        AudioListener.volume = value;
    }
    
    private void SetFullscreen(bool isFullscreen)
    {
        // Установка полноэкранного режима
        Screen.fullScreen = isFullscreen;
    }
}