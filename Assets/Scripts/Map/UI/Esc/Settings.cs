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
    [Header("Ссылки на объекты UI")]
    public GameObject settingsPanel;
    public Slider volumeSlider; // Переименовано в общий слайдер громкости вместо musicVolumeSlider
    public Button backButton;
    
    [Header("Ссылки на аудио")]
    public AudioMixer audioMixer;
    public AudioSource uiAudioSource;
    
    [Header("Системные компоненты")]
    private Pause pauseScript;
    
    // Компоненты для управления слайдерами
    private RectTransform volumeSliderHandle; // Переименовано в общий ползунок
    private float prevVolumeValue = 0.75f; // Общее значение громкости
    
    [Header("Настройки звука")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Настройки графики")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    
    [Header("Звуки интерфейса")]
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip sliderChangeSound;
    
    [Header("Настройки взаимодействия")]
    [SerializeField] private RectTransform musicSliderHandle;
    [SerializeField] private RectTransform sfxSliderHandle;
    
    private static Settings Instance;
    
    private string activeMusicParameterName = "MusicVolume";
    private string activeSFXParameterName = "SFXVolume";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Проверим наличие AudioMixer
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioMixer не назначен в настройках! Попытка найти аудиомиксер в ресурсах...");
            
            // Пытаемся найти аудиомиксер в ресурсах
            audioMixer = Resources.Load<AudioMixer>("MainMixer");
            
            if (audioMixer == null)
            {
                // Проверяем другие пути
                audioMixer = Resources.Load<AudioMixer>("Audio/MainMixer");
            }
            
            if (audioMixer == null)
            {
                // Еще одна попытка с другим именем
                audioMixer = Resources.Load<AudioMixer>("Mixers/AudioMixer");
            }
            
            if (audioMixer != null)
            {
                Debug.Log("AudioMixer успешно найден в ресурсах!");
            }
            else
            {
                Debug.LogError("Не удалось найти AudioMixer в ресурсах. Функции регулировки звука будут недоступны.");
            }
        }
        
        // Пытаемся найти UI AudioSource, если он не задан
        if (uiAudioSource == null)
        {
            uiAudioSource = GetComponent<AudioSource>();
            
            if (uiAudioSource == null && Camera.main != null)
            {
                uiAudioSource = Camera.main.GetComponent<AudioSource>();
            }
        }
        
        // Находим ссылку на скрипт паузы
        pauseScript = FindObjectOfType<Pause>();
        
        // Проверяем наличие EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("EventSystem отсутствует на сцене! UI не будет работать!");
        }
        
        // Отключаем стандартные интеракции слайдеров
        if (musicVolumeSlider != null)
        {
            prevVolumeValue = musicVolumeSlider.value;
            
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
            prevVolumeValue = sfxVolumeSlider.value;
            
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

        // Если settingsPanel равен null, используем этот gameObject
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
        }
        
        // Логируем статус инициализации
        Debug.Log("Settings: Awake");
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
        volumeSliderHandle = null;
        prevVolumeValue = 0.75f;
        
        // Восстанавливаем ползунки
        RestoreSliderHandles();
    }
    
    private void OnDisable()
    {
        // Сбрасываем состояние перетаскивания
        volumeSliderHandle = null;
        prevVolumeValue = 0.75f;
    }
    
    private void Update()
    {
        // Проверяем, открыто ли меню настроек
        bool isActive = (settingsPanel != null) ? settingsPanel.activeInHierarchy : gameObject.activeInHierarchy;
        if (!isActive)
            return;

        // Проверяем, инициализированы ли слайдеры
        if (volumeSlider == null)
            return;
        
        // Проверяем, нужно ли восстановить ползунки
        bool needToRestoreHandles = false;
        
        if (musicSliderHandle != null && !musicSliderHandle.gameObject.activeInHierarchy)
        {
            needToRestoreHandles = true;
        }
        
        if (sfxSliderHandle != null && !sfxSliderHandle.gameObject.activeInHierarchy)
        {
            needToRestoreHandles = true;
        }
        
        // Если ползунки невидимы, восстанавливаем их
        if (needToRestoreHandles)
        {
            RestoreSliderHandles();
        }
        
        // Проверяем, что мышь доступна
        if (Mouse.current == null)
            return;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        bool isMouseDown = Mouse.current.leftButton.isPressed;
        
        // Если кнопка мыши была нажата в этом кадре
        if (isMouseDown && !isMouseDown)
        {
            // Проверяем, попали ли мы по слайдеру
            if (IsPointOverSlider(volumeSlider, mousePosition))
            {
                volumeSliderHandle = volumeSlider.GetComponent<RectTransform>();
                UpdateSliderFromMousePosition(volumeSlider, mousePosition);
                PlaySliderSound();
                Debug.Log("Начато управление слайдером громкости");
            }
        }
        // Если мышь перемещается при зажатой кнопке
        else if (isMouseDown && volumeSliderHandle != null)
        {
            UpdateSliderFromMousePosition(volumeSlider, mousePosition);
        }
        // Если кнопка мыши была отпущена в этом кадре
        else if (!isMouseDown && isMouseDown && volumeSliderHandle != null)
        {
            // Проверяем, изменилось ли значение
            if (Mathf.Abs(prevVolumeValue - volumeSlider.value) > 0.01f)
            {
                // Применяем новое значение громкости
                SetVolume(volumeSlider.value);
                prevVolumeValue = volumeSlider.value;
            }
            
            // После завершения перетаскивания убеждаемся, что ползунки видимы
            RestoreSliderHandles();
            
            volumeSliderHandle = null;
        }
    }
    
    private bool IsPointOverSlider(Slider slider, Vector2 screenPoint)
    {
        if (slider == null)
            return false;
        
        // Получаем RectTransform слайдера
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        if (sliderRect == null)
            return false;
        
        // Создаем список частей слайдера для проверки нажатия
        List<RectTransform> sliderParts = new List<RectTransform>();
        
        // Добавляем основные части
        sliderParts.Add(sliderRect); // Сам слайдер
        
        if (slider.fillRect != null)
            sliderParts.Add(slider.fillRect); // Область заполнения
        
        if (slider.handleRect != null)
            sliderParts.Add(slider.handleRect); // Ползунок
        
        // Также ищем "Sliding Area" и "Handle Slide Area"
        Transform slidingArea = slider.transform.Find("Sliding Area");
        if (slidingArea != null)
            sliderParts.Add(slidingArea.GetComponent<RectTransform>());
        
        Transform handleSlideArea = slider.transform.Find("Handle Slide Area");
        if (handleSlideArea != null)
            sliderParts.Add(handleSlideArea.GetComponent<RectTransform>());
        
        // Находим активную камеру Canvas
        Camera eventCamera = null;
        Canvas canvas = slider.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            eventCamera = canvas.worldCamera;
        
        // Проверяем каждую часть слайдера
        foreach (RectTransform part in sliderParts)
        {
            if (part == null)
                continue;
            
            // Преобразуем координаты экрана в локальные координаты
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(part, screenPoint, eventCamera, out localPoint))
            {
                // Проверяем, находится ли точка внутри прямоугольника
                if (part.rect.Contains(localPoint))
                {
                    Debug.Log($"Нажатие на часть слайдера {slider.name}: {part.name}");
                    return true;
                }
            }
        }
        
        // Дополнительная проверка - расширяем область нажатия слайдера
        Vector2 expandedLocalPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRect, screenPoint, eventCamera, out expandedLocalPoint))
        {
            // Расширяем область нажатия на 20 пикселей вокруг слайдера
            Rect expandedRect = sliderRect.rect;
            expandedRect.xMin -= 20;
            expandedRect.xMax += 20;
            expandedRect.yMin -= 20;
            expandedRect.yMax += 20;
            
            if (expandedRect.Contains(expandedLocalPoint))
            {
                Debug.Log($"Нажатие в расширенной области слайдера {slider.name}");
                return true;
            }
        }
        
        return false;
    }
    
    private void UpdateSliderFromMousePosition(Slider slider, Vector2 mousePosition)
    {
        if (slider == null)
            return;
        
        // Получаем RectTransform слайдера
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        if (sliderRect == null)
            return;
        
        // Находим SlideArea - область перемещения ползунка
        RectTransform slideArea = null;
        Transform slideAreaTransform = slider.transform.Find("Sliding Area");
        if (slideAreaTransform == null)
            slideAreaTransform = slider.transform.Find("SlideArea");
        if (slideAreaTransform == null)
            slideAreaTransform = slider.transform.Find("Slide Area");
        
        if (slideAreaTransform != null)
            slideArea = slideAreaTransform.GetComponent<RectTransform>();
        
        // Если не нашли SlideArea, используем сам слайдер
        if (slideArea == null)
            slideArea = sliderRect;
        
        // Находим активную камеру Canvas
        Camera eventCamera = null;
        Canvas canvas = slider.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            eventCamera = canvas.worldCamera;
        
        // Преобразуем позицию мыши в локальные координаты SlideArea
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(slideArea, mousePosition, eventCamera, out localPoint))
        {
            // Получаем размеры области
            float width = slideArea.rect.width;
            float height = slideArea.rect.height;
            
            // Вычисляем нормализованное значение в зависимости от направления слайдера
            float normalizedValue = 0f;
            
            if (slider.direction == Slider.Direction.LeftToRight)
            {
                // Ограничиваем localPoint границами SlideArea
                localPoint.x = Mathf.Clamp(localPoint.x, -width/2, width/2);
                // Преобразуем координату в нормализованное значение
                normalizedValue = (localPoint.x + width/2) / width;
            }
            else if (slider.direction == Slider.Direction.RightToLeft)
            {
                localPoint.x = Mathf.Clamp(localPoint.x, -width/2, width/2);
                normalizedValue = 1f - ((localPoint.x + width/2) / width);
            }
            else if (slider.direction == Slider.Direction.BottomToTop)
            {
                localPoint.y = Mathf.Clamp(localPoint.y, -height/2, height/2);
                normalizedValue = (localPoint.y + height/2) / height;
            }
            else // TopToBottom
            {
                localPoint.y = Mathf.Clamp(localPoint.y, -height/2, height/2);
                normalizedValue = 1f - ((localPoint.y + height/2) / height);
            }
            
            // Дополнительная проверка границ
            normalizedValue = Mathf.Clamp01(normalizedValue);
            
            // Устанавливаем значение слайдера
            float newValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
            
            if (Mathf.Abs(slider.value - newValue) > 0.001f)
            {
                Debug.Log($"Слайдер {slider.name}: новое значение={newValue} (нормализованное={normalizedValue})");
                slider.value = newValue;
                
                // Применяем изменение громкости в зависимости от слайдера
                SetVolume(newValue);
            }
        }
    }
    
    private void UpdateSliderHandlePosition(RectTransform handle, Slider slider, float normalizedValue)
    {
        if (handle == null || slider == null)
            return;
        
        // Находим различные возможные области слайдера
        RectTransform fillRect = slider.fillRect;
        RectTransform handleSlideArea = null;
        
        // Пытаемся найти HandleSlideArea
        Transform handleSlideAreaTransform = slider.transform.Find("Handle Slide Area");
        if (handleSlideAreaTransform != null)
            handleSlideArea = handleSlideAreaTransform.GetComponent<RectTransform>();
        
        // Если не найдена HandleSlideArea, пробуем найти родительский объект ползунка
        if (handleSlideArea == null && handle.parent != null)
            handleSlideArea = handle.parent.GetComponent<RectTransform>();
        
        // Если всё еще не найдена, используем самого родителя слайдера
        if (handleSlideArea == null)
            handleSlideArea = slider.GetComponent<RectTransform>();
        
        // Находим размеры и позицию области перемещения ползунка
        float width = handleSlideArea.rect.width;
        float height = handleSlideArea.rect.height;
        
        // Если есть fillRect, используем его для уточнения размеров
        if (fillRect != null && fillRect.parent != null)
        {
            RectTransform fillArea = fillRect.parent.GetComponent<RectTransform>();
            if (fillArea != null)
            {
                width = fillArea.rect.width;
                height = fillArea.rect.height;
            }
        }
        
        // Рассчитываем позицию ползунка в зависимости от направления слайдера
        Vector2 newAnchoredPosition = handle.anchoredPosition;
        
        // Фиксируем якорь в центре для простоты расчетов
        handle.anchorMin = new Vector2(0.5f, 0.5f);
        handle.anchorMax = new Vector2(0.5f, 0.5f);
        handle.pivot = new Vector2(0.5f, 0.5f);
        
        // Размер ползунка
        float handleWidth = handle.rect.width;
        float handleHeight = handle.rect.height;
        
        // Корректировки для разных направлений слайдера
        if (slider.direction == Slider.Direction.LeftToRight)
        {
            // Расчет с учетом размера ползунка
            float usableWidth = width - handleWidth;
            float handleOffset = (normalizedValue - 0.5f) * usableWidth;
            newAnchoredPosition.x = handleOffset;
        }
        else if (slider.direction == Slider.Direction.RightToLeft)
        {
            float usableWidth = width - handleWidth;
            float handleOffset = (0.5f - normalizedValue) * usableWidth;
            newAnchoredPosition.x = handleOffset;
        }
        else if (slider.direction == Slider.Direction.BottomToTop)
        {
            float usableHeight = height - handleHeight;
            float handleOffset = (normalizedValue - 0.5f) * usableHeight;
            newAnchoredPosition.y = handleOffset;
        }
        else // TopToBottom
        {
            float usableHeight = height - handleHeight;
            float handleOffset = (0.5f - normalizedValue) * usableHeight;
            newAnchoredPosition.y = handleOffset;
        }
        
        handle.anchoredPosition = newAnchoredPosition;
        Debug.Log($"Обновлена позиция ползунка: {slider.name}, normalizedValue={normalizedValue}, новая позиция={newAnchoredPosition}");
        
        // Делаем ползунок видимым
        handle.gameObject.SetActive(true);
    }
    
    // Добавляем метод для восстановления ползунков
    private void RestoreSliderHandles()
    {
        Debug.Log("Восстанавливаем ползунки слайдеров");
        
        // Восстанавливаем ползунок музыки
        if (musicSliderHandle != null)
        {
            // Активируем объект
            musicSliderHandle.gameObject.SetActive(true);
            
            // Проверяем, есть ли у ползунка компонент Image
            Image handleImage = musicSliderHandle.GetComponent<Image>();
            if (handleImage == null)
            {
                handleImage = musicSliderHandle.gameObject.AddComponent<Image>();
                handleImage.color = Color.white;
            }
            
            // Убедимся, что ползунок виден
            handleImage.enabled = true;
            
            // Устанавливаем размер, если он слишком мал
            if (musicSliderHandle.sizeDelta.x < 10 || musicSliderHandle.sizeDelta.y < 10)
            {
                musicSliderHandle.sizeDelta = new Vector2(20, 20);
            }
            
            // Обновляем позицию ползунка
            UpdateSliderHandlePosition(musicSliderHandle, volumeSlider, volumeSlider.value);
            
            Debug.Log($"Восстановлен ползунок музыки: позиция={musicSliderHandle.anchoredPosition}, размер={musicSliderHandle.sizeDelta}");
        }
        
        // Восстанавливаем ползунок SFX
        if (sfxSliderHandle != null)
        {
            // Активируем объект
            sfxSliderHandle.gameObject.SetActive(true);
            
            // Проверяем, есть ли у ползунка компонент Image
            Image handleImage = sfxSliderHandle.GetComponent<Image>();
            if (handleImage == null)
            {
                handleImage = sfxSliderHandle.gameObject.AddComponent<Image>();
                handleImage.color = Color.white;
            }
            
            // Убедимся, что ползунок виден
            handleImage.enabled = true;
            
            // Устанавливаем размер, если он слишком мал
            if (sfxSliderHandle.sizeDelta.x < 10 || sfxSliderHandle.sizeDelta.y < 10)
            {
                sfxSliderHandle.sizeDelta = new Vector2(20, 20);
            }
            
            // Обновляем позицию ползунка
            UpdateSliderHandlePosition(sfxSliderHandle, sfxVolumeSlider, sfxVolumeSlider.value);
            
            Debug.Log($"Восстановлен ползунок SFX: позиция={sfxSliderHandle.anchoredPosition}, размер={sfxSliderHandle.sizeDelta}");
        }
    }
    
    private void CheckUIElements()
    {
        // Проверяем слайдеры
        if (volumeSlider != null)
        {
            Debug.Log($"Слайдер громкости: активен={volumeSlider.gameObject.activeInHierarchy}, " +
                      $"интерактивен={volumeSlider.interactable}, значение={volumeSlider.value}");
            
            // Принудительно устанавливаем значение для проверки
            volumeSlider.value = 0.75f;
        }
        else
        {
            Debug.LogError("Слайдер громкости не назначен!");
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
    
    private void InitializeComponents()
    {
        // Проверяем и находим панель настроек
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
            Debug.Log("Панель настроек установлена по умолчанию на текущий объект");
        }
        
        // Проверяем и находим слайдеры
        if (volumeSlider == null)
        {
            volumeSlider = transform.Find("VolumeSlider")?.GetComponent<Slider>();
            if (volumeSlider == null)
            {
                // Ищем по имени или тегу
                var sliders = GetComponentsInChildren<Slider>();
                foreach (var slider in sliders)
                {
                    if (slider.name.ToLower().Contains("volume") || slider.CompareTag("VolumeSlider"))
                    {
                        volumeSlider = slider;
                        Debug.Log("Найден слайдер громкости: " + slider.name);
                        break;
                    }
                }
            }
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider = transform.Find("SFXVolumeSlider")?.GetComponent<Slider>();
            if (sfxVolumeSlider == null)
            {
                // Ищем по имени или тегу
                var sliders = GetComponentsInChildren<Slider>();
                foreach (var slider in sliders)
                {
                    if (slider.name.ToLower().Contains("sfx") || slider.name.ToLower().Contains("effect") || 
                        slider.CompareTag("SFXSlider"))
                    {
                        sfxVolumeSlider = slider;
                        Debug.Log("Найден слайдер SFX: " + slider.name);
                        break;
                    }
                }
            }
        }
        
        // Проверяем и находим ползунки слайдеров - более агрессивный поиск
        if (volumeSlider != null && musicSliderHandle == null)
        {
            // Пробуем найти стандартный ползунок Unity UI
            Transform handleSlideArea = volumeSlider.transform.Find("Handle Slide Area");
            if (handleSlideArea != null)
            {
                Transform handle = handleSlideArea.Find("Handle");
                if (handle != null)
                {
                    musicSliderHandle = handle.GetComponent<RectTransform>();
                    Debug.Log("Найден стандартный ползунок слайдера громкости");
                }
            }
            
            // Если не нашли по стандартному пути, ищем любой ползунок
            if (musicSliderHandle == null)
            {
                foreach (Transform child in volumeSlider.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.ToLower().Contains("handle") || child.name.ToLower().Contains("knob") || 
                        child.name.ToLower().Contains("thumb") || child.name.ToLower().Contains("dot"))
                    {
                        musicSliderHandle = child.GetComponent<RectTransform>();
                        Debug.Log($"Найден альтернативный ползунок для слайдера громкости: {child.name}");
                        break;
                    }
                }
            }
            
            // Если всё еще не нашли, создаем новый ползунок
            if (musicSliderHandle == null)
            {
                Debug.LogWarning("Не найден ползунок для слайдера громкости, создаем новый...");
                GameObject newHandle = new GameObject("VolumeSliderHandle");
                newHandle.transform.SetParent(volumeSlider.transform, false);
                musicSliderHandle = newHandle.AddComponent<RectTransform>();
                
                // Добавляем изображение для видимости
                Image img = newHandle.AddComponent<Image>();
                img.color = Color.white;
                
                // Устанавливаем размер
                musicSliderHandle.sizeDelta = new Vector2(20, 20);
            }
        }
        
        // То же самое для SFX слайдера
        if (sfxVolumeSlider != null && sfxSliderHandle == null)
        {
            // Пробуем найти стандартный ползунок Unity UI
            Transform handleSlideArea = sfxVolumeSlider.transform.Find("Handle Slide Area");
            if (handleSlideArea != null)
            {
                Transform handle = handleSlideArea.Find("Handle");
                if (handle != null)
                {
                    sfxSliderHandle = handle.GetComponent<RectTransform>();
                    Debug.Log("Найден стандартный ползунок слайдера SFX");
                }
            }
            
            // Если не нашли по стандартному пути, ищем любой ползунок
            if (sfxSliderHandle == null)
            {
                foreach (Transform child in sfxVolumeSlider.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.ToLower().Contains("handle") || child.name.ToLower().Contains("knob") || 
                        child.name.ToLower().Contains("thumb") || child.name.ToLower().Contains("dot"))
                    {
                        sfxSliderHandle = child.GetComponent<RectTransform>();
                        Debug.Log($"Найден альтернативный ползунок для слайдера SFX: {child.name}");
                        break;
                    }
                }
            }
            
            // Если всё еще не нашли, создаем новый ползунок
            if (sfxSliderHandle == null)
            {
                Debug.LogWarning("Не найден ползунок для слайдера SFX, создаем новый...");
                GameObject newHandle = new GameObject("SFXSliderHandle");
                newHandle.transform.SetParent(sfxVolumeSlider.transform, false);
                sfxSliderHandle = newHandle.AddComponent<RectTransform>();
                
                // Добавляем изображение для видимости
                Image img = newHandle.AddComponent<Image>();
                img.color = Color.white;
                
                // Устанавливаем размер
                sfxSliderHandle.sizeDelta = new Vector2(20, 20);
            }
        }
        
        // Проверяем наличие UI AudioSource
        if (uiAudioSource == null)
        {
            uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null)
            {
                // Создаем новый AudioSource
                uiAudioSource = gameObject.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
                Debug.Log("Создан новый AudioSource для UI звуков");
            }
        }
        
        // Выводим отладочную информацию
        Debug.Log($"Инициализация компонентов: VolumeSlider={volumeSlider != null}, SFXSlider={sfxVolumeSlider != null}, " +
                  $"VolumeHandle={musicSliderHandle != null}, SFXHandle={sfxSliderHandle != null}, AudioSource={uiAudioSource != null}");
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("Settings: Start");
        
        // Инициализируем компоненты
        InitializeComponents();
        
        // Проверяем аудиомиксер и его параметры
        if (audioMixer != null)
        {
            Debug.Log("AudioMixer найден, проверяем параметры...");
            DebugAudioMixerParameters();
        }
        else
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: AudioMixer равен null в Start()!");
        }
        
        // Загружаем сохраненные настройки из PlayerPrefs
        LoadSavedSettings();
        
        // Если окно настроек открыто при старте, обновим ползунки
        if (settingsPanel != null && settingsPanel.activeInHierarchy)
        {
            RestoreSliderHandles();
        }
    }
    
    // Добавляем метод для загрузки сохраненных настроек
    private void LoadSavedSettings()
    {
        // Загружаем сохраненные значения громкости
        float volume = PlayerPrefs.GetFloat("Volume", 0.75f);
        
        Debug.Log($"[LoadSavedSettings] Загружены настройки: Volume={volume}");
        
        // Устанавливаем загруженные значения
        SetVolume(volume);
        
        // Если слайдеры уже инициализированы, устанавливаем их значения принудительно
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
            Debug.Log($"[LoadSavedSettings] Установлено значение слайдера громкости: {volume}");
            
            // Обновляем ползунок
            if (musicSliderHandle != null)
            {
                musicSliderHandle.gameObject.SetActive(true);
                UpdateSliderHandlePosition(musicSliderHandle, volumeSlider, volume);
            }
        }
        
        // Дополнительная проверка - применяем значение напрямую ко всем источникам звука в сцене
        AdjustAllAudioSourcesVolume("All", volume);
    }

    // Настройка обработчиков событий для слайдеров
    private void SetupSliderEvents()
    {
        if (volumeSlider != null)
        {
            // Добавляем обработчик изменения значения
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(SetVolume);
            Debug.Log("Настроен обработчик для слайдера громкости");
        }
    }

    // Улучшаем методы установки громкости для использования всех доступных подходов
    public void SetVolume(float volume)
    {
        Debug.Log($"[SetVolume] Вызван с значением: {volume}");
        
        // Запоминаем значение
        prevVolumeValue = volume;
        bool handledByMixer = false;
        
        // 1. Пробуем использовать AudioMixer
        if (audioMixer != null)
        {
            // Преобразуем линейное значение в логарифмическое для громкости
            float dbValue = volume > 0.001f ? Mathf.Log10(volume) * 20 : -80f;
            Debug.Log($"[SetVolume] Преобразованное значение: {dbValue} dB");
            
            // Пробуем установить громкость с разными именами параметров
            string[] paramNames = { "Volume", "masterVolume", "MasterVolume", "Master", "MasterVol" };
            
            foreach (string paramName in paramNames)
            {
                if (audioMixer.SetFloat(paramName, dbValue))
                {
                    Debug.Log($"[SetVolume] УСПЕХ: Установлена громкость через параметр '{paramName}': {dbValue} dB");
                    
                    // Проверяем, действительно ли значение установлено
                    float currentValue;
                    if (audioMixer.GetFloat(paramName, out currentValue))
                    {
                        Debug.Log($"[SetVolume] Проверка: Текущее значение параметра '{paramName}': {currentValue} dB");
                        handledByMixer = Mathf.Approximately(currentValue, dbValue);
                    }
                    
                    break;
                }
            }
            
            if (!handledByMixer)
            {
                Debug.LogWarning("[SetVolume] AudioMixer не применил значение или вернул ошибку");
            }
        }
        else
        {
            Debug.LogError("[SetVolume] AudioMixer равен null!");
        }
        
        // 2. Если AudioMixer не помог, используем прямой контроль источников
        if (!handledByMixer)
        {
            Debug.Log("[SetVolume] Переход к прямому контролю источников звука");
            
            // Используем AudioListener как общий регулятор для музыки
            AudioListener.volume = volume;
            Debug.Log($"[SetVolume] Установлена общая громкость AudioListener: {volume}");
            
            // Регулируем все аудио источники в сцене
            AdjustAllAudioSourcesVolume("All", volume);
        }
        
        // 3. Сохраняем настройку в PlayerPrefs независимо от результата
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
        Debug.Log($"[SetVolume] Сохранено значение в PlayerPrefs: {volume}");
        
        // 4. Обновляем ползунок
        if (musicSliderHandle != null)
        {
            musicSliderHandle.gameObject.SetActive(true);
            UpdateSliderHandlePosition(musicSliderHandle, volumeSlider, volume);
            Debug.Log($"[SetVolume] Обновлена позиция ползунка громкости");
        }
        else
        {
            Debug.LogWarning("[SetVolume] Ползунок громкости равен null!");
        }
        
        // 5. Проверяем значение слайдера
        if (volumeSlider != null)
        {
            volumeSlider.value = volume; // Принудительно устанавливаем значение слайдера
            Debug.Log($"[SetVolume] Установлено значение слайдера громкости: {volume}");
        }
        else
        {
            Debug.LogWarning("[SetVolume] Слайдер громкости равен null!");
        }
        
        // Воспроизводим звук изменения
        PlaySliderSound();
    }

    // Упрощаем метод загрузки настроек
    private void LoadSettings()
    {
        Debug.Log("Загрузка настроек...");
        
        // Загружаем настройки громкости
        float volume = PlayerPrefs.GetFloat("Volume", 0.75f);
        
        // Устанавливаем значения слайдеров
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
            Debug.Log($"Установлено значение слайдера громкости: {volume}");
        }
        
        // Применяем настройки звука
        if (audioMixer != null)
        {
            ApplyVolumeSettings(volume);
        }
        
        // Загружаем настройку полного экрана
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        
        // Применяем настройку полного экрана
        Screen.fullScreen = isFullscreen;
        
        // Устанавливаем переключатель полного экрана
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
        }
        
        Debug.Log("Настройки успешно загружены");
    }

    // Добавляем вспомогательный метод для применения настроек громкости
    private void ApplyVolumeSettings(float volume)
    {
        // Преобразуем линейные значения в логарифмические для громкости
        float dbValue = volume > 0.001f ? Mathf.Log10(volume) * 20 : -80f;
        
        // Пробуем установить громкость музыки
        bool set = false;
        string[] paramNames = { "Volume", "masterVolume", "MasterVolume", "Master", "MasterVol" };
        
        foreach (string param in paramNames)
        {
            if (audioMixer.SetFloat(param, dbValue))
            {
                Debug.Log($"Применена громкость через параметр '{param}': {dbValue} dB");
                set = true;
                break;
            }
        }
        
        // Запасные варианты, если не удалось установить через AudioMixer
        if (!set)
        {
            AudioListener.volume = volume;
            Debug.Log($"Громкость музыки применена через AudioListener: {volume}");
        }
        
        // Регулируем все аудио источники в сцене
        AdjustAllAudioSourcesVolume("All", volume);
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
        // Сохраняем настройки перед выходом из меню
        SaveSettings();
        
        // Используем новый метод закрытия настроек
        CloseSettings();
        
        // Показываем основную панель паузы, если есть ссылка на скрипт паузы
        if (pauseScript != null)
        {
            pauseScript.ShowPausePanel();
        }
    }
    
    #region Audio Settings
    
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
        Debug.Log("Сохранение настроек...");
        
        // Получаем текущее значение слайдера
        float volume = volumeSlider != null ? volumeSlider.value : 0.75f;
        
        // Сохраняем настройки в PlayerPrefs
        PlayerPrefs.SetFloat("Volume", volume);
        
        // Убедимся, что настройки сохранены
        PlayerPrefs.Save();
        
        Debug.Log($"Настройки сохранены: Volume={volume}");
    }
    
    private void SetSliderValueSafely(Slider slider, float value)
    {
        if (slider == null)
            return;
        
        // Используем SetValueWithoutNotify для установки значения без вызова событий
        slider.SetValueWithoutNotify(value);
        
        // Обновляем позицию ползунка вручную
        if (slider == volumeSlider && musicSliderHandle != null)
        {
            UpdateSliderHandlePosition(musicSliderHandle, slider, value);
        }
        
        Debug.Log($"Безопасно установлено значение слайдера {slider.name}: {value}");
    }

    // Метод для определения имен активных параметров аудиомиксера
    private void CheckAudioMixerParameters()
    {
        if (audioMixer == null)
            return;
        
        // Возможные имена параметров для музыки
        string[] paramNames = { "Volume", "masterVolume", "MasterVolume", "Master", "MasterVol" };
        
        // Ищем активный параметр для музыки
        foreach (string paramName in paramNames)
        {
            float testValue;
            if (audioMixer.GetFloat(paramName, out testValue))
            {
                activeMusicParameterName = paramName;
                Debug.Log($"Найден активный параметр для музыки: '{paramName}'");
                break;
            }
        }
        
        Debug.Log($"Активный параметр аудиомиксера: Music='{activeMusicParameterName}'");
    }
    
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
        if (volumeSlider != null)
        {
            volumeSlider.interactable = true;
            Debug.Log($"Слайдер громкости: интерактивен={volumeSlider.interactable}, значение={volumeSlider.value}");
        }
        
        // Сбрасываем состояние перетаскивания
        volumeSliderHandle = null;
        prevVolumeValue = 0.75f;
        
        Debug.Log("Проверка и исправление UI завершены!");
    }
    
    // Публичный метод для принудительного исправления слайдеров
    public void FixSlidersAndInputSystem()
    {
        FixUIProblems();
    }

    private void SyncSlidersWithMixer()
    {
        if (audioMixer == null)
            return;
        
        // Синхронизируем слайдер громкости
        if (volumeSlider != null && !string.IsNullOrEmpty(activeMusicParameterName))
        {
            float dbValue;
            if (audioMixer.GetFloat(activeMusicParameterName, out dbValue))
            {
                // Преобразуем логарифмическое значение в линейное для слайдера
                float linearValue = dbValue <= -79.9f ? 0f : Mathf.Pow(10, dbValue / 20);
                linearValue = Mathf.Clamp01(linearValue);
                
                // Устанавливаем значение слайдера безопасно
                SetSliderValueSafely(volumeSlider, linearValue);
                prevVolumeValue = linearValue;
                
                Debug.Log($"Синхронизирован слайдер громкости: {dbValue} dB -> {linearValue} (линейная)");
            }
        }
    }

    // Добавляем метод CreateMixerParameters
    private void CreateMixerParameters()
    {
        if (audioMixer == null)
            return;
        
        Debug.Log("Проверка параметров аудиомиксера...");
        
        // Проверяем наличие основных параметров
        CheckAudioMixerParameters();
        
        // Если не найдены параметры, выводим инструкцию
        if (string.IsNullOrEmpty(activeMusicParameterName))
        {
            Debug.LogWarning("Не найдены необходимые параметры в аудиомиксере!");
            Debug.LogWarning("Инструкция по созданию параметров в Unity Editor:");
            Debug.LogWarning("1. Найдите файл аудиомиксера в проекте (обычно в папке Assets/Audio или Assets/Resources)");
            Debug.LogWarning("2. Откройте AudioMixer в Inspector");
            Debug.LogWarning("3. Перейдите на вкладку 'Groups'");
            Debug.LogWarning("4. Для каждой группы (Master, Music, SFX):");
            Debug.LogWarning("   a. Выберите группу");
            Debug.LogWarning("   b. В Inspector найдите параметр Volume");
            Debug.LogWarning("   c. Щелкните правой кнопкой мыши и выберите 'Expose parameter'");
            Debug.LogWarning("   d. Введите имя (например, 'Volume' или 'MasterVolume')");
            Debug.LogWarning("5. Сохраните проект и перезапустите игру");
        }
    }

    public void OpenSettings()
    {
        Debug.Log("[OpenSettings] Открытие настроек");
        
        // Если settingsPanel равен null, используем этот gameObject
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
        }
        
        settingsPanel.SetActive(true);
        
        // Проверяем аудиомиксер
        if (audioMixer != null)
        {
            Debug.Log("[OpenSettings] Проверка параметров AudioMixer");
            DebugAudioMixerParameters();
        }
        else
        {
            Debug.LogError("[OpenSettings] AudioMixer равен null!");
        }
        
        // Загружаем настройки из PlayerPrefs
        float volume = PlayerPrefs.GetFloat("Volume", 0.75f);
        
        Debug.Log($"[OpenSettings] Загружены сохраненные настройки: Volume={volume}");
        
        // Принудительно устанавливаем значение слайдера
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
            Debug.Log($"[OpenSettings] Установлено значение слайдера громкости: {volume}");
            
            // Обновляем ползунок
            if (musicSliderHandle != null)
            {
                musicSliderHandle.gameObject.SetActive(true);
                UpdateSliderHandlePosition(musicSliderHandle, volumeSlider, volume);
            }
        }
        
        // Восстанавливаем видимость ползунков
        RestoreSliderHandles();
        
        // Применяем настройки напрямую к аудиоисточникам для надежности
        AdjustAllAudioSourcesVolume("All", volume);
        
        Debug.Log("[OpenSettings] Настройки открыты и инициализированы");
    }

    // Добавляем метод для закрытия настроек, который гарантирует сохранение
    public void CloseSettings()
    {
        // Сохраняем настройки перед закрытием
        SaveSettings();
        
        // Скрываем панель
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        Debug.Log("Настройки закрыты");
    }

    // Добавляем отладочный метод для выявления проблем в Unity AudioMixer
    private void DebugAudioMixerParameters()
    {
        if (audioMixer == null)
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: AudioMixer равен null!");
            return;
        }
        
        Debug.Log("=== ОТЛАДКА АУДИО МИКСЕРА ===");
        
        // Список возможных имен параметров
        string[] possibleParamNames = { "Volume", "masterVolume", "MasterVolume", "Master", "MasterVol" };
        
        // Проверяем музыкальные параметры
        Debug.Log("Проверка параметров МУЗЫКИ:");
        foreach (string param in possibleParamNames)
        {
            float value;
            bool exists = audioMixer.GetFloat(param, out value);
            Debug.Log($"  - Параметр '{param}': {(exists ? "СУЩЕСТВУЕТ" : "НЕ СУЩЕСТВУЕТ")} {(exists ? $"Значение: {value} dB" : "")}");
        }
        
        Debug.Log("=== КОНЕЦ ОТЛАДКИ АУДИО МИКСЕРА ===");
    }

    // Обновляем метод для регулировки громкости всех AudioSource в сцене
    private void AdjustAllAudioSourcesVolume(string type, float volume)
    {
        Debug.Log($"[AdjustAllAudioSourcesVolume] Регулировка громкости всех AudioSource на значение {volume}");
        
        // Найдем все AudioSource в сцене
        AudioSource[] allAudioSources = GameObject.FindObjectsOfType<AudioSource>();
        Debug.Log($"[AdjustAllAudioSourcesVolume] Найдено аудиоисточников: {allAudioSources.Length}");
        
        // Пройдемся по всем и изменим громкость
        int adjustedCount = 0;
        
        foreach (AudioSource source in allAudioSources)
        {
            // Изменяем громкость всех источников, независимо от типа
            source.volume = volume;
            adjustedCount++;
            Debug.Log($"[AdjustAllAudioSourcesVolume] Изменена громкость источника: {source.gameObject.name}");
        }
        
        Debug.Log($"[AdjustAllAudioSourcesVolume] Изменено источников: {adjustedCount} из {allAudioSources.Length}");
    }
}

#endregion 