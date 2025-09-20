using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAction : MonoBehaviour
{
    public ButtonType buttonType; // Выбирается в Inspector

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Console.WriteLine("Кнопка нажата");
        EventManager.ModelClicked(buttonType, gameObject);
    }
}
