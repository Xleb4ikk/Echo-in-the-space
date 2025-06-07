using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;

public class AlarmControler : MonoBehaviour
{
    [Header("Источники освещения")]
    [SerializeField] private List<Light> LightPointList;
    [SerializeField] private List<Light> AlarmLightSpotList;
    [SerializeField] private List<Renderer> targetRenderer;

    [SerializeField] private Material mat;

    bool AlarmStatus = true;

    public float pulseSpeed = 2f;
    public float minIntensity = 0.2f;
    public float maxIntensity = 2f;

    void Start()
    {
        foreach (var rend in targetRenderer)
        {
            Material mat = rend.material;
            mat.EnableKeyword("_EMISSION");
            materials.Add(mat);
        }
        SetLightsForAlarm(true);
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleAlarm();
        }
        if (AlarmStatus)
        {
            PulseAlarmLights();
        }
    }

    private void ToggleAlarm()
    {
        AlarmStatus = !AlarmStatus;
        SetLightsForAlarm(AlarmStatus);
    }

    private void SetLightsForAlarm(bool isAlarm)
    {
        foreach (var mainLight in LightPointList)
            mainLight.enabled = !isAlarm;

        foreach (var alarmLight in AlarmLightSpotList)
        {
            alarmLight.enabled = isAlarm;

            if (isAlarm)
                alarmLight.intensity = minIntensity;
        }
    }

    private void PulseAlarmLights()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // колебание от 0 до 1
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        foreach (var light in AlarmLightSpotList)
        {
            light.intensity = intensity;
        }
    }
}
