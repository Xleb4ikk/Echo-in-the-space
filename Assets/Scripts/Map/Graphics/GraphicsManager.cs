using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class GraphicsManager : MonoBehaviour
{
    [Header("Post Processing Volume")]
    public Volume postProcessVolume;

    [Header("Display Settings")]
    public DisplaySettingsG displaySettings;

    private const string PREF_QUALITY_LEVEL = "qualityLevel";
    private const string PREF_TEXTURE_QUALITY = "textureQuality";
    private const string PREF_ANISOTROPIC_FILTERING = "anisotropicFiltering";
    private const string PREF_LOD_BIAS = "lodBias";
    private const string PREF_POST_PROCESSING = "postProcessingEnabled";

    private const string PREF_FULLSCREEN_MODE = "fullscreenMode";
    private const string PREF_RESOLUTION_WIDTH = "resolutionWidth";
    private const string PREF_RESOLUTION_HEIGHT = "resolutionHeight";
    private const string PREF_VSYNC = "vSync";

    private void Start()
    {
        LoadSettings();
        ApplyAllCurrentSettings();
    }

    public void ApplyGraphicsPreset(GraphicsPreset preset)
    {
        QualitySettings.SetQualityLevel(preset.qualityLevel, true);
        QualitySettings.globalTextureMipmapLimit = preset.textureQuality;
        QualitySettings.anisotropicFiltering = preset.anisotropicFiltering;
        QualitySettings.lodBias = preset.lodBias;

        if (postProcessVolume != null)
            postProcessVolume.enabled = preset.postProcessingEnabled;

        if (preset.presetName.ToLower() == "low")
        {
            SetShadowsEnabled(false, LightShadowResolution.Low);
        }
        else if (preset.presetName.ToLower() == "medium")
        {
            SetShadowsEnabled(true, LightShadowResolution.Medium);
        }
        else // high и остальные
        {
            SetShadowsEnabled(true, LightShadowResolution.High);
        }
    }

    public void ApplyDisplaySettings()
    {
        Screen.SetResolution(displaySettings.resolutionWidth, displaySettings.resolutionHeight, displaySettings.fullscreen);
        QualitySettings.vSyncCount = displaySettings.vSync ? 1 : 0;
    }

    public void ApplyAll(GraphicsPreset preset)
    {
        ApplyGraphicsPreset(preset);
        ApplyDisplaySettings();
        SaveSettings();
    }

    public void ApplyAllCurrentSettings()
    {
        // Здесь можно подставить текущий GraphicsPreset, если есть
        // Или отдельно применять displaySettings
        ApplyDisplaySettings();
    }

    public void SetShadowsEnabled(bool enabled, LightShadowResolution shadowRes)
    {
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in allLights)
        {
            if (enabled)
            {
                light.shadows = LightShadows.Soft;
                light.shadowResolution = shadowRes;
            }
            else
            {
                light.shadows = LightShadows.None;
            }
        }
    }

    public void SetResolution(int width, int height, FullScreenMode mode)
    {
        Screen.SetResolution(width, height, mode);
        Debug.Log($"[Запрос] Установить разрешение: {width}x{height}, режим: {mode}");

        StartCoroutine(VerifyResolutionSet(width, height, mode));
        SaveDisplaySettings(width, height, mode);
    }

    private IEnumerator VerifyResolutionSet(int targetWidth, int targetHeight, FullScreenMode targetMode)
    {
        yield return null; // Ждем 1 кадр

        bool resolutionMatch = Screen.width == targetWidth && Screen.height == targetHeight;
        bool modeMatch = Screen.fullScreenMode == targetMode;

        if (resolutionMatch && modeMatch)
        {
            Debug.Log($"✅ Разрешение успешно установлено: {Screen.width}x{Screen.height}, режим: {Screen.fullScreenMode}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Разрешение НЕ применено корректно.\nТекущее: {Screen.width}x{Screen.height}, режим: {Screen.fullScreenMode}");
        }
    }

    // -------------------- Сохранение и Загрузка --------------------

    public void SaveSettings()
    {
        PlayerPrefs.SetInt(PREF_QUALITY_LEVEL, QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt(PREF_TEXTURE_QUALITY, QualitySettings.globalTextureMipmapLimit);
        PlayerPrefs.SetInt(PREF_ANISOTROPIC_FILTERING, (int)QualitySettings.anisotropicFiltering);
        PlayerPrefs.SetFloat(PREF_LOD_BIAS, QualitySettings.lodBias);

        PlayerPrefs.SetInt(PREF_POST_PROCESSING, (postProcessVolume != null && postProcessVolume.enabled) ? 1 : 0);

        SaveDisplaySettings(displaySettings.resolutionWidth, displaySettings.resolutionHeight, displaySettings.fullscreen);

        PlayerPrefs.SetInt(PREF_VSYNC, QualitySettings.vSyncCount > 0 ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void SaveDisplaySettings(int width, int height, FullScreenMode mode)
    {
        PlayerPrefs.SetInt(PREF_RESOLUTION_WIDTH, width);
        PlayerPrefs.SetInt(PREF_RESOLUTION_HEIGHT, height);
        PlayerPrefs.SetInt(PREF_FULLSCREEN_MODE, (int)mode);
        PlayerPrefs.Save();

        displaySettings.resolutionWidth = width;
        displaySettings.resolutionHeight = height;
        displaySettings.fullscreen = mode;
        displaySettings.vSync = QualitySettings.vSyncCount > 0;
    }

    public void LoadSettings()
    {
        displaySettings.resolutionWidth = PlayerPrefs.GetInt(PREF_RESOLUTION_WIDTH, Screen.currentResolution.width);
        displaySettings.resolutionHeight = PlayerPrefs.GetInt(PREF_RESOLUTION_HEIGHT, Screen.currentResolution.height);
        displaySettings.fullscreen = (FullScreenMode)PlayerPrefs.GetInt(PREF_FULLSCREEN_MODE, (int)FullScreenMode.FullScreenWindow);

        displaySettings.vSync = PlayerPrefs.GetInt(PREF_VSYNC, 1) == 1;

        if (postProcessVolume != null)
            postProcessVolume.enabled = PlayerPrefs.GetInt(PREF_POST_PROCESSING, 1) == 1;
    }
}
