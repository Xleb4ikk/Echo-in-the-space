using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsResolutionUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;

    [Header("Manager")]
    public GraphicsManager graphicsManager;

    private Resolution[] resolutions;
    private List<string> resolutionOptions = new List<string>();
    private List<string> screenModeOptions = new List<string> { "Fullscreen", "Windowed", "Maximized" };

    private void Start()
    {
        PopulateResolutions();
        PopulateScreenModes();

        resolutionDropdown.onValueChanged.AddListener(SetResolutionFromDropdown);
        screenModeDropdown.onValueChanged.AddListener(SetScreenModeFromDropdown);
    }

    void PopulateResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        resolutionOptions.Clear();

        HashSet<string> unique = new HashSet<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            if (unique.Add(option))
            {
                resolutionOptions.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = resolutionOptions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void PopulateScreenModes()
    {
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(screenModeOptions);

        int currentModeIndex = (int)Screen.fullScreenMode;
        screenModeDropdown.value = currentModeIndex;
        screenModeDropdown.RefreshShownValue();
    }

    void SetResolutionFromDropdown(int index)
    {
        string[] parts = resolutionOptions[index].Split('x');
        int width = int.Parse(parts[0].Trim());
        int height = int.Parse(parts[1].Trim());

        graphicsManager.SetResolution(width, height, Screen.fullScreenMode);
    }

    void SetScreenModeFromDropdown(int index)
    {
        FullScreenMode selectedMode = (FullScreenMode)index;
        graphicsManager.SetResolution(Screen.width, Screen.height, selectedMode);
    }
}
