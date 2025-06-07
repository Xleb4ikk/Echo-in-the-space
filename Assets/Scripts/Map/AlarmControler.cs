using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;

public class AlarmControler : MonoBehaviour
{
    [Header("Обычное освещение")]
    [SerializeField] private List<Light> LightPointList;

    [Header("alarm освещение")]
    [SerializeField] private List<Light> AlarmLightSpotList;

    [SerializeField] private float materialMinEmission = 0.2f;
    [SerializeField] private float materialMaxEmission = 3f;

    [SerializeField] private Material sharedMaterial;
    [SerializeField] private Color baseEmissionColor = Color.red;

    bool AlarmStatus = true;

    public float pulseSpeed = 2f;
    public float minIntensity = 0.2f;
    public float maxIntensity = 2f;

    void Start()
    {
        if (sharedMaterial != null)
        {
            sharedMaterial.EnableKeyword("_EMISSION"); // обязательно включить Emission
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
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        float lightIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        foreach (var light in AlarmLightSpotList)
        {
            light.intensity = lightIntensity;
        }

        if (sharedMaterial != null)
        {
            float emissionIntensity = Mathf.Lerp(materialMinEmission, materialMaxEmission, t);
            sharedMaterial.EnableKeyword("_EMISSION");
            sharedMaterial.SetColor("_EmissionColor", baseEmissionColor * emissionIntensity);
        }
    }

}
