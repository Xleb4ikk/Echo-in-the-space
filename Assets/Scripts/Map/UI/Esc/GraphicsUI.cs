using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicsUI : MonoBehaviour
{
    public GraphicsManager graphicsManager;

    public GraphicsPreset lowPreset;
    public GraphicsPreset mediumPreset;
    public GraphicsPreset highPreset;

    public TMP_Dropdown graphicsDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    void Start()
    {
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(new System.Collections.Generic.List<string> { "Low", "Medium", "High" });
        graphicsDropdown.onValueChanged.AddListener(OnDropdownChanged);

        // Преобразуем FullScreenMode в bool для toggle
        fullscreenToggle.isOn = graphicsManager.displaySettings.fullscreen == FullScreenMode.FullScreenWindow;

        vsyncToggle.isOn = graphicsManager.displaySettings.vSync;

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        vsyncToggle.onValueChanged.AddListener(SetVSync);

        ApplyPreset(0);
        graphicsDropdown.value = 0;
    }

    void OnDropdownChanged(int index)
    {
        ApplyPreset(index);
    }

    void ApplyPreset(int index)
    {
        switch (index)
        {
            case 0:
                graphicsManager.ApplyAll(lowPreset);
                break;
            case 1:
                graphicsManager.ApplyAll(mediumPreset);
                break;
            case 2:
                graphicsManager.ApplyAll(highPreset);
                break;
        }
    }

    public void SetFullscreen(bool value)
    {
        // Преобразуем bool обратно в FullScreenMode
        graphicsManager.displaySettings.fullscreen = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        graphicsManager.ApplyDisplaySettings();
    }

    public void SetVSync(bool value)
    {
        graphicsManager.displaySettings.vSync = value;
        graphicsManager.ApplyDisplaySettings();
    }
}
