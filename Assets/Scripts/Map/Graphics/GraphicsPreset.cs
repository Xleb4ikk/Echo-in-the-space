using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "GraphicsPreset", menuName = "Graphics/Graphics Preset")]
public class GraphicsPreset : ScriptableObject
{
    [Header("General")]
    public string presetName;
    public int qualityLevel;

    [Header("Post Processing")]
    public bool postProcessingEnabled;

    [Header("Rendering")]
    [Range(0.5f, 2.0f)] public float renderScale = 1.0f;
    public int antiAliasing = 0; // 0, 2, 4, 8
    public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Enable;
    public int textureQuality = 0; // 0 - Full Res, 1 - Half Res, etc.

    [Header("Shadows Settings")]
    public bool shadowsEnabled = true;
    public ShadowResolution shadowResolution = ShadowResolution.Medium;
    public float shadowDistance = 50f;

    [Header("Other")]
    public float lodBias = 1.0f;
    public bool softParticles = true;
    public bool realtimeReflectionProbes = true;
    public bool billboardsFaceCamera = true;
}
