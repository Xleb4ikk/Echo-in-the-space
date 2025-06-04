using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class StaminaSliderUI : MonoBehaviour
{
    [SerializeField] private Color fillColor = Color.white;
    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField, Range(0.1f, 5f)] private float outlineWidth = 1f;
    [SerializeField] private Color outlineColor = Color.white;

    private Slider slider;
    private Image fillImage;
    private Image backgroundImage;

    void Start()
    {
        slider = GetComponent<Slider>();
        
        // Находим компоненты изображений
        fillImage = slider.fillRect.GetComponent<Image>();
        backgroundImage = slider.transform.Find("Background").GetComponent<Image>();

        // Настраиваем цвета
        if (fillImage != null)
        {
            fillImage.color = fillColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        // Добавляем обводку
        AddOutline();
    }

    private void AddOutline()
    {
        // Добавляем обводку к фону слайдера
        if (backgroundImage != null)
        {
            // Удаляем старую обводку, если она есть
            Outline oldOutline = backgroundImage.GetComponent<Outline>();
            if (oldOutline != null)
            {
                Destroy(oldOutline);
            }

            // Добавляем новую обводку
            Outline outline = backgroundImage.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(outlineWidth, outlineWidth);
        }
    }

    // Обновляем обводку при изменении параметров в инспекторе
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            AddOutline();
        }
    }
} 