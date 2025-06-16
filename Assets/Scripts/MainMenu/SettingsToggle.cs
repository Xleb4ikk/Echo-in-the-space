using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("UI Ёлементы")]
    public GameObject settingsPanel;

    public Button openSettingsButton;
    public Button closeSettingsButton;

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (openSettingsButton != null)
            openSettingsButton.onClick.AddListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}
