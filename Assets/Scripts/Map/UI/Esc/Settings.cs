using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class Settings : MonoBehaviour
{
    [Header("Настройки звука")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Настройки графики")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    
    [Header("Навигация")]
    [SerializeField] private Button backButton;
    
    [Header("Звуки интерфейса")]
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip sliderChangeSound;
    
    [Header("Настройки взаимодействия")]
    [SerializeField] private RectTransform musicSliderHandle;
    [SerializeField] private RectTransform sfxSliderHandle;
    
    private AudioSource uiAudioSource;
    private Pause pauseScript;
    
    // Переменные для прямого управления слайдерами
    private Slider activeSlider = null;
    private bool wasMouseDown = false;
    private float prevMusicValue;
    private float prevSFXValue;
    
    private void Awake()
    {
        // Создаем источник звука для UI
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        
        // Находим ссылку на скрипт паузы
        pauseScript = GetComponentInParent<Pause>();
        if (pauseScript == null)
            pauseScript = FindObjectOfType<Pause>();
            
        // Проверяем наличие EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("EventSystem отсутствует на сцене! UI не будет работать!");
        }
        
        // Отключаем стандартные интеракции слайдеров
        if (musicVolumeSlider != null)
        {
            prevMusicValue = musicVolumeSlider.value;
            
            // Если не назначен handle, используем первый дочерний элемент
            if (musicSliderHandle == null && musicVolumeSlider.transform.childCount > 0)
            {
                Transform handleTransform = musicVolumeSlider.transform.Find("Handle Slide Area/Handle");
                if (handleTransform != null)
                {
                    musicSliderHandle = handleTransform.GetComponent<RectTransform>();
                }
            }
        }
        
        if (sfxVolumeSlider != null)
        {
            prevSFXValue = sfxVolumeSlider.value;
            
            // Если не назначен handle, используем первый дочерний элемент
            if (sfxSliderHandle == null && sfxVolumeSlider.transform.childCount > 0)
            {
                Transform handleTransform = sfxVolumeSlider.transform.Find("Handle Slide Area/Handle");
                if (handleTransform != null)
                {
                    sfxSliderHandle = handleTransform.GetComponent<RectTransform>();
                }
            }
        }
    }
    
    private void OnEnable()
    {
        // Убеждаемся, что курсор видим и не заблокирован
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Убеждаемся, что время не нулевое
        if (Time.timeScale <= 0)
            Time.timeScale = 0.1f;
        
        // Проверяем все UI элементы при активации панели
        CheckUIElements();
        
        // Сбрасываем состояние
        activeSlider = null;
        wasMouseDown = false;
        
        // Восстанавливаем ползунки
        RestoreSliderHandles();
    }
    
    private void OnDisable()
    {
        // Сбрасываем состояние перетаскивания
        activeSlider = null;
        wasMouseDown = false;
    }
    
    private void Update()
    {
        // Проверяем видимость ползунков и восстанавливаем их при необходимости
        if ((musicVolumeSlider != null && musicSliderHandle != null && !musicSliderHandle.gameObject.activeInHierarchy) ||
            (sfxVolumeSlider != null && sfxSliderHandle != null && !sfxSliderHandle.gameObject.activeInHierarchy))
        {
            RestoreSliderHandles();
        }
        
        // Проверяем, что мышь доступна
        if (Mouse.current == null)
            return;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        bool isMouseDown = Mouse.current.leftButton.isPressed;
        
        // Если кнопка мыши была нажата в этом кадре
        if (isMouseDown && !wasMouseDown)
        {
            // Проверяем, попали ли мы по слайдеру
            if (IsPointOverSlider(musicVolumeSlider, mousePosition))
            {
                activeSlider = musicVolumeSlider;
                UpdateSliderFromMousePosition(activeSlider, mousePosition);
                PlaySliderSound();
                Debug.Log("Начато управление музыкальным слайдером");
            }
            else if (IsPointOverSlider(sfxVolumeSlider, mousePosition))
            {
                activeSlider = sfxVolumeSlider;
                UpdateSliderFromMousePosition(activeSlider, mousePosition);
                PlaySliderSound();
                Debug.Log("Начато управление слайдером звуков");
            }
        }
        // Если мышь перемещается при зажатой кнопке
        else if (isMouseDown && activeSlider != null)
        {
            UpdateSliderFromMousePosition(activeSlider, mousePosition);
        }
        // Если кнопка мыши была отпущена в этом кадре
        else if (!isMouseDown && wasMouseDown && activeSlider != null)
        {
            // Проверяем, изменилось ли значение
            if (activeSlider == musicVolumeSlider && Mathf.Abs(prevMusicValue - activeSlider.value) > 0.01f)
            {
                PlaySliderSound();
                prevMusicValue = activeSlider.value;
            }
            else if (activeSlider == sfxVolumeSlider && Mathf.Abs(prevSFXValue - activeSlider.value) > 0.01f)
            {
                PlaySliderSound();
                prevSFXValue = activeSlider.value;
            }
            
            // После завершения перетаскивания убеждаемся, что ползунки видимы
            RestoreSliderHandles();
            
            activeSlider = null;
        }
        
        // Запоминаем состояние мыши для следующего кадра
        wasMouseDown = isMouseDown;
    }
    
    private bool IsPointOverSlider(Slider slider, Vector2 screenPoint)
    {
        if (slider == null)
            return false;
        
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        if (sliderRect == null)
            return false;
        
        // Преобразуем координаты экрана в локальные координаты
        Vector2 localPoint;
        Camera eventCamera = null;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            eventCamera = canvas.worldCamera;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRect, screenPoint, eventCamera, out localPoint))
        {
            // Проверяем, находится ли точка внутри прямоугольника слайдера
            return sliderRect.rect.Contains(localPoint);
        }
        
        return false;
    }
    
    private void UpdateSliderFromMousePosition(Slider slider, Vector2 screenPoint)
    {
        if (slider == null)
            return;
        
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        if (sliderRect == null)
            return;
        
        // Преобразуем координаты экрана в локальные координаты
        Vector2 localPoint;
        Camera eventCamera = null;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            eventCamera = canvas.worldCamera;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRect, screenPoint, eventCamera, out localPoint))
        {
            // Получаем ссылку на область, которая содержит слайдер
            RectTransform fillArea = slider.fillRect != null ? slider.fillRect.parent.GetComponent<RectTransform>() : null;
            
            if (fillArea == null)
                return;
            
            // Получаем ширину области заполнения, а не всего слайдера
            float fillAreaWidth = fillArea.rect.width;
            float fillAreaHeight = fillArea.rect.height;
            
            // Переводим локальные координаты в координаты относительно области заполнения
            Vector2 fillLocalPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(fillArea, screenPoint, eventCamera, out fillLocalPoint))
                return;
            
            // Ограничиваем локальную точку размерами области заполнения
            fillLocalPoint.x = Mathf.Clamp(fillLocalPoint.x, -fillAreaWidth * 0.5f, fillAreaWidth * 0.5f);
            fillLocalPoint.y = Mathf.Clamp(fillLocalPoint.y, -fillAreaHeight * 0.5f, fillAreaHeight * 0.5f);
            
            // Вычисляем нормализованное значение в зависимости от направления слайдера
            float normalizedValue;
            
            if (slider.direction == Slider.Direction.LeftToRight)
            {
                normalizedValue = (fillLocalPoint.x + fillAreaWidth * 0.5f) / fillAreaWidth;
            }
            else if (slider.direction == Slider.Direction.RightToLeft)
            {
                normalizedValue = 1 - (fillLocalPoint.x + fillAreaWidth * 0.5f) / fillAreaWidth;
            }
            else if (slider.direction == Slider.Direction.BottomToTop)
            {
                normalizedValue = (fillLocalPoint.y + fillAreaHeight * 0.5f) / fillAreaHeight;
            }
            else // TopToBottom
            {
                normalizedValue = 1 - (fillLocalPoint.y + fillAreaHeight * 0.5f) / fillAreaHeight;
            }
            
            // Добавляем дополнительное ограничение
            normalizedValue = Mathf.Clamp01(normalizedValue);
            
            // Преобразуем нормализованное значение в значение слайдера
            float newValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
            
            // Обновляем значение слайдера
            if (Mathf.Abs(slider.value - newValue) > 0.001f)
            {
                slider.value = newValue;
                
                // Вызываем соответствующую функцию в зависимости от типа слайдера
                if (slider == musicVolumeSlider)
                {
                    SetMusicVolume(newValue);
                }
                else if (slider == sfxVolumeSlider)
                {
                    SetSFXVolume(newValue);
                }
            }
            
            // Обновляем позицию ползунка вручную, если ссылка на него есть
            if (slider == musicVolumeSlider && musicSliderHandle != null)
            {
                UpdateSliderHandlePosition(musicSliderHandle, slider, normalizedValue);
            }
            else if (slider == sfxVolumeSlider && sfxSliderHandle != null)
            {
                UpdateSliderHandlePosition(sfxSliderHandle, slider, normalizedValue);
            }
        }
    }
    
    private void UpdateSliderHandlePosition(RectTransform handle, Slider slider, float normalizedValue)
    {
        if (handle == null || slider == null)
            return;
        
        // Убеждаемся, что ползунок видим
        handle.gameObject.SetActive(true);
        
        // Получаем родительский RectTransform ползунка (обычно это SlideArea)
        RectTransform handleParent = handle.parent as RectTransform;
        if (handleParent == null)
            return;
        
        // Получаем размеры родителя ползунка
        float parentWidth = handleParent.rect.width;
        float parentHeight = handleParent.rect.height;
        
        // НЕ меняем настройки привязки (anchor), так как это может нарушить стандартное поведение
        // Используем только позиционирование через anchoredPosition
        
        // Используем стандартное поведение Unity для позиционирования ползунка
        // Это сохранит корректное отображение слайдера в соответствии со стилем Unity UI
        
        if (slider.direction == Slider.Direction.LeftToRight)
        {
            // Рассчитываем новую позицию относительно родительского элемента
            handle.anchorMin = new Vector2(normalizedValue, handle.anchorMin.y);
            handle.anchorMax = new Vector2(normalizedValue, handle.anchorMax.y);
            handle.anchoredPosition = new Vector2(0, handle.anchoredPosition.y);
        }
        else if (slider.direction == Slider.Direction.RightToLeft)
        {
            handle.anchorMin = new Vector2(1 - normalizedValue, handle.anchorMin.y);
            handle.anchorMax = new Vector2(1 - normalizedValue, handle.anchorMax.y);
            handle.anchoredPosition = new Vector2(0, handle.anchoredPosition.y);
        }
        else if (slider.direction == Slider.Direction.BottomToTop)
        {
            handle.anchorMin = new Vector2(handle.anchorMin.x, normalizedValue);
            handle.anchorMax = new Vector2(handle.anchorMax.x, normalizedValue);
            handle.anchoredPosition = new Vector2(handle.anchoredPosition.x, 0);
        }
        else // TopToBottom
        {
            handle.anchorMin = new Vector2(handle.anchorMin.x, 1 - normalizedValue);
            handle.anchorMax = new Vector2(handle.anchorMax.x, 1 - normalizedValue);
            handle.anchoredPosition = new Vector2(handle.anchoredPosition.x, 0);
        }
        
        // Обновляем заполнение слайдера, чтобы оно соответствовало положению ползунка
        if (slider.fillRect != null)
        {
            // Обновляем размер области заполнения в соответствии с значением слайдера
            slider.fillRect.gameObject.SetActive(true);
        }
    }
    
    // Добавим метод, который будет восстанавливать слайдеры при необходимости
    private void RestoreSliderHandles()
    {
        if (musicVolumeSlider != null && musicSliderHandle != null)
        {
            // Убеждаемся, что ползунок видим
            musicSliderHandle.gameObject.SetActive(true);
            
            // Обновляем его позицию
            float normalizedValue = (musicVolumeSlider.value - musicVolumeSlider.minValue) / 
                                   (musicVolumeSlider.maxValue - musicVolumeSlider.minValue);
            UpdateSliderHandlePosition(musicSliderHandle, musicVolumeSlider, normalizedValue);
        }
        
        if (sfxVolumeSlider != null && sfxSliderHandle != null)
        {
            // Убеждаемся, что ползунок видим
            sfxSliderHandle.gameObject.SetActive(true);
            
            // Обновляем его позицию
            float normalizedValue = (sfxVolumeSlider.value - sfxVolumeSlider.minValue) / 
                                   (sfxVolumeSlider.maxValue - sfxVolumeSlider.minValue);
            UpdateSliderHandlePosition(sfxSliderHandle, sfxVolumeSlider, normalizedValue);
        }
    }
    
    private void CheckUIElements()
    {
        // Проверяем слайдеры
        if (musicVolumeSlider != null)
        {
            Debug.Log($"Музыкальный слайдер: активен={musicVolumeSlider.gameObject.activeInHierarchy}, " +
                      $"интерактивен={musicVolumeSlider.interactable}, значение={musicVolumeSlider.value}");
            
            // Принудительно устанавливаем значение для проверки
            musicVolumeSlider.value = 0.75f;
        }
        else
        {
            Debug.LogError("Музыкальный слайдер не назначен!");
        }
        
        if (sfxVolumeSlider != null)
        {
            Debug.Log($"Слайдер звуков: активен={sfxVolumeSlider.gameObject.activeInHierarchy}, " +
                      $"интерактивен={sfxVolumeSlider.interactable}, значение={sfxVolumeSlider.value}");
            
            // Принудительно устанавливаем значение для проверки
            sfxVolumeSlider.value = 0.75f;
        }
        else
        {
            Debug.LogError("Слайдер звуков не назначен!");
        }
        
        // Проверяем выпадающий список качества
        if (qualityDropdown != null)
        {
            Debug.Log($"Выпадающий список качества: активен={qualityDropdown.gameObject.activeInHierarchy}, " +
                      $"интерактивен={qualityDropdown.interactable}, значение={qualityDropdown.value}");
        }
        else
        {
            Debug.LogError("Выпадающий список качества не назначен!");
        }
        
        // Проверяем переключатель полноэкранного режима
        if (fullscreenToggle != null)
        {
            Debug.Log($"Переключатель полноэкранного режима: активен={fullscreenToggle.gameObject.activeInHierarchy}, " +
                      $"интерактивен={fullscreenToggle.interactable}, значение={fullscreenToggle.isOn}");
        }
        else
        {
            Debug.LogError("Переключатель полноэкранного режима не назначен!");
        }
        
        // Проверяем кнопку назад
        if (backButton != null)
        {
            Debug.Log($"Кнопка назад: активна={backButton.gameObject.activeInHierarchy}, " +
                      $"интерактивна={backButton.interactable}");
        }
        else
        {
            Debug.LogError("Кнопка назад не назначена!");
        }
    }
    
    private void Start()
    {
        // Проверяем и настраиваем EventSystem для работы с Input System
        ConfigureInputSystem();
        
        // Настраиваем элементы управления звуком
        SetupAudioControls();
        
        // Настраиваем элементы управления графикой
        SetupGraphicsControls();
        
        // Настраиваем кнопку назад
        if (backButton != null)
            backButton.onClick.AddListener(() => { PlayButtonClickSound(); BackToPauseMenu(); });
        
        // Добавляем звуки наведения на все кнопки
        AddHoverSoundToButtons();
        
        // Загружаем сохраненные настройки
        LoadSettings();
    }
    
    // Настраиваем EventSystem для работы с Input System
    private void ConfigureInputSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            // Проверяем, есть ли InputSystemUIInputModule
            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                // Удаляем StandaloneInputModule, если есть
                StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    Debug.Log("Удаляем устаревший StandaloneInputModule");
                    DestroyImmediate(oldModule);
                }
                
                // Добавляем InputSystemUIInputModule
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("Добавлен InputSystemUIInputModule для EventSystem");
            }
        }
        else
        {
            Debug.LogError("EventSystem не найден на сцене! Создаем новый...");
            
            // Создаем новый EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            
            // Добавляем InputSystemUIInputModule
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            Debug.Log("Создан новый EventSystem с InputSystemUIInputModule");
        }
    }
    
    private void AddHoverSoundToButtons()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        
        foreach (Button button in allButtons)
        {
            // Добавляем EventTrigger для звука наведения
            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = button.gameObject.AddComponent<EventTrigger>();
                
            // Проверяем, есть ли уже триггер для PointerEnter
            bool hasPointerEnterTrigger = false;
            foreach (EventTrigger.Entry entry in eventTrigger.triggers)
            {
                if (entry.eventID == EventTriggerType.PointerEnter)
                {
                    hasPointerEnterTrigger = true;
                    break;
                }
            }
            
            // Добавляем триггер, если его еще нет
            if (!hasPointerEnterTrigger)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => { PlayButtonHoverSound(); });
                eventTrigger.triggers.Add(entry);
            }
        }
    }
    
    private void SetupAudioControls()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            Debug.Log("Добавлен обработчик для слайдера музыки");
        }
            
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            Debug.Log("Добавлен обработчик для слайдера звуков");
        }
    }
    
    private void SetupGraphicsControls()
    {
        // Настраиваем выпадающий список качества
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            List<string> qualityOptions = new List<string>();
            string[] qualityNames = QualitySettings.names;
            
            foreach (string name in qualityNames)
            {
                qualityOptions.Add(name);
            }
            
            qualityDropdown.AddOptions(qualityOptions);
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
            Debug.Log("Настроен выпадающий список качества");
        }
        
        // Настраиваем переключатель полноэкранного режима
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            Debug.Log("Настроен переключатель полноэкранного режима");
        }
    }
    
    public void BackToPauseMenu()
    {
        // Сохраняем настройки перед выходом
        SaveSettings();
        
        // Скрываем панель настроек
        gameObject.SetActive(false);
        
        // Показываем основную панель паузы, если есть ссылка на скрипт паузы
        if (pauseScript != null)
        {
            pauseScript.ShowPausePanel();
        }
    }
    
    #region Audio Settings
    
    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            // Преобразуем линейное значение слайдера в логарифмическую шкалу для громкости
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
        }
        PlaySliderSound();
    }
    
    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
        }
        PlaySliderSound();
    }
    
    #endregion
    
    #region Graphics Settings
    
    public void SetQualityLevel(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayButtonClickSound();
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayButtonClickSound();
    }
    
    #endregion
    
    #region UI Sounds
    
    private void PlayButtonHoverSound()
    {
        if (buttonHoverSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonHoverSound, 0.5f);
        }
    }
    
    private void PlayButtonClickSound()
    {
        if (buttonClickSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound, 0.5f);
        }
    }
    
    private void PlaySliderSound()
    {
        if (sliderChangeSound != null && uiAudioSource != null && !uiAudioSource.isPlaying)
        {
            uiAudioSource.PlayOneShot(sliderChangeSound, 0.3f);
        }
    }
    
    #endregion
    
    #region Save/Load Settings
    
    public void SaveSettings()
    {
        // Сохраняем настройки звука
        if (audioMixer != null)
        {
            float musicVolume, sfxVolume;
            audioMixer.GetFloat("MusicVolume", out musicVolume);
            audioMixer.GetFloat("SFXVolume", out sfxVolume);
            
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }
        
        // Сохраняем настройки графики
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        
        // Сохраняем изменения
        PlayerPrefs.Save();
        Debug.Log("Настройки сохранены!");
    }
    
    private void LoadSettings()
    {
        // Загружаем настройки звука
        if (audioMixer != null)
        {
            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                float musicVolume = PlayerPrefs.GetFloat("MusicVolume");
                audioMixer.SetFloat("MusicVolume", musicVolume);
                if (musicVolumeSlider != null)
                    musicVolumeSlider.value = Mathf.Pow(10, musicVolume / 20);
            }
            
            if (PlayerPrefs.HasKey("SFXVolume"))
            {
                float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
                audioMixer.SetFloat("SFXVolume", sfxVolume);
                if (sfxVolumeSlider != null)
                    sfxVolumeSlider.value = Mathf.Pow(10, sfxVolume / 20);
            }
        }
        
        // Загружаем настройки графики
        if (PlayerPrefs.HasKey("QualityLevel"))
        {
            int qualityLevel = PlayerPrefs.GetInt("QualityLevel");
            QualitySettings.SetQualityLevel(qualityLevel);
            if (qualityDropdown != null)
                qualityDropdown.value = qualityLevel;
        }
        
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            Screen.fullScreen = isFullscreen;
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = isFullscreen;
        }
        
        Debug.Log("Настройки загружены!");
    }
    
    #endregion
    
    // Публичный метод для проверки и исправления проблем с UI
    public void FixUIProblems()
    {
        Debug.Log("Исправление проблем с UI...");
        
        // Проверяем настройки EventSystem
        ConfigureInputSystem();
        
        // Убеждаемся, что курсор видим и не заблокирован
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Убеждаемся, что время не нулевое
        if (Time.timeScale <= 0)
        {
            Time.timeScale = 0.1f;
            Debug.Log("Time.timeScale был 0, установлен на 0.1");
        }
        
        // Проверяем Canvas и его компоненты
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("Добавлен GraphicRaycaster на Canvas");
            }
        }
        
        // Проверяем состояние слайдеров
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.interactable = true;
            Debug.Log($"Музыкальный слайдер: интерактивен={musicVolumeSlider.interactable}, значение={musicVolumeSlider.value}");
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.interactable = true;
            Debug.Log($"Слайдер звуков: интерактивен={sfxVolumeSlider.interactable}, значение={sfxVolumeSlider.value}");
        }
        
        // Сбрасываем состояние перетаскивания
        activeSlider = null;
        wasMouseDown = false;
        
        Debug.Log("Проверка и исправление UI завершены!");
    }
    
    // Публичный метод для принудительного исправления слайдеров
    public void FixSlidersAndInputSystem()
    {
        FixUIProblems();
    }
} 