using UnityEngine;

public class AnimationContriler_Script : MonoBehaviour
{
    [Header("Главное Меню")]
    public GameObject mainMenuPanel;

    // Вызывается из Animation Event
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
    }
}
