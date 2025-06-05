using UnityEngine;

public class AnimationContriler_Script : MonoBehaviour
{
    [Header("Ui Элементы")]
    public GameObject mainMenuPanel;

    //[Header("Контролеры Анимаций")]

    // Вызывается из Animation Event
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
    }
}
