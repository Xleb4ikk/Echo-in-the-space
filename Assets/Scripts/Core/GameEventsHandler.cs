using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEventsHandler : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.OnModelClick += HandleModelClick;
    }

    private void OnDisable()
    {
        EventManager.OnModelClick -= HandleModelClick;
    }

    private void HandleModelClick(ButtonType type, GameObject model)
    {
        switch (type)
        {
            case ButtonType.Play:
                Debug.Log("Play button clicked!");
                SceneManager.LoadScene(1); 
                break;
            case ButtonType.Exit:
                Debug.Log("Exit button clicked!");
                Application.Quit();
                break;
            case ButtonType.Settings:
                Debug.Log("Settings button clicked!");
                OpenSettingsMenu();
                break;
        }
    }

    private void OpenSettingsMenu()
    {
        Debug.Log("Открываем меню настроек");
    }
}
