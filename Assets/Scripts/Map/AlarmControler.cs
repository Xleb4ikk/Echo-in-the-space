using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;

public class AlarmControler : MonoBehaviour
{
    [Header("»сточники освещени€")]
    [SerializeField] public List<Light> LightPointList;
    [SerializeField] public List<Light> AlarmLightSpotList;

    bool AlarmStatus = true;

    public float pulseSpeed = 2f;         // скорость мерцани€
    public float minIntensity = 0.2f;     // минимальна€ €ркость
    public float maxIntensity = 2f;       // максимальна€ €ркость

    void Start()
    {
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
