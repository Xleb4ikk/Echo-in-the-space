using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class AlarmControler : MonoBehaviour
{
    [Header("Обычное освещение")]
    [SerializeField] private List<Light> LightPointList;

    [Header("alarm освещение")]
    [SerializeField] private List<Light> AlarmLightSpotList;

    [Header("Невидимые стены")]
    [SerializeField] private List<GameObject> VisibleWalls;

    [Header("alarm звуковые источники")]
    [SerializeField] private List<AudioSource> alarmAudioSources;

    [Header("alarm материал")]
    [SerializeField] private float materialMinEmission = 0.2f;
    [SerializeField] private float materialMaxEmission = 3f;

    [SerializeField] private Material sharedMaterial;
    [SerializeField] private Material sharedMaterial2;
    [SerializeField] private Color baseEmissionColor = Color.red;

    [SerializeField] private Color baseMaterialColorColor;
    float intensity = 2;

    [Header("Мониторы тревоги")]
    public VideoPlayer MainManitor;
    public VideoPlayer LeftManitor;
    public VideoPlayer RightManitor;

    [Header("Мониторы Настройки")]
    public VideoClip DangerOn;
    public VideoClip DangerOff;
    public VideoClip DangerLeftRight;

    [Header("Настройки пульсации света")]
    public float pulseSpeed = 2f;
    public float minIntensity = 0.2f;
    public float maxIntensity = 2f;

    [Header("Тригер")]
    public Collider triggerZone;
    public bool checkEnabled = true; // ← Управляющий флаг

    public GameObject DialogTriger;
    public GameObject DialogTrigerAlaramOff;

    [Header("Тригеры для включения")]
    [SerializeField] private GameObject EventTriger;

    [Header("Игрок")]
    public Transform playerTransform;

    public bool IsPlayerInside { get; private set; }

    private float alarmTimer = 0f;

    bool AlarmStatus = true;

    public bool AlarmEnabled = true;
    
    void Start()
    {
        if (AlarmEnabled == true)
        {
            if (sharedMaterial != null)
            {
                sharedMaterial.EnableKeyword("_EMISSION"); // обязательно включить Emission
            }
            SetLightsForAlarm(true);
            ToggleMonitorAlarm();
        }
    }

    void Update()
    {
        if (AlarmEnabled == true)
        {
            if (!checkEnabled || triggerZone == null || playerTransform == null)
                return;

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                ToggleAlarm();
            }
            if (AlarmStatus)
            {
                PulseAlarmLights();
            }

            if (triggerZone != null && playerTransform != null)
            {
                // Проверка, находится ли игрок внутри границ триггера
                IsPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

                if (IsPlayerInside)
                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        FixAlarm();

                    }
            }
        }
    }

    private void ToggleMonitorAlarm()
    {
        MainManitor.clip = DangerOn;
        LeftManitor.clip = DangerLeftRight;
        RightManitor.clip = DangerLeftRight;
    }

    private void ToggleMonitorSucces()
    {
        MainManitor.clip = DangerOff;
        LeftManitor.clip = DangerOff;
        RightManitor.clip = DangerOff;
    }

    private void FixAlarm()
    {
        SetLightsForAlarm(false);
        ToggleAlarm();
        ToggleMonitorSucces();
        StopAlarmSounds();
        Color finalColor = baseMaterialColorColor * intensity;
        sharedMaterial2.EnableKeyword("_EMISSION");
        sharedMaterial2.SetColor("_EmissionColor", finalColor);
        triggerZone.enabled = false;
        foreach (GameObject ReverseTriger in VisibleWalls)
        {
            ReverseTriger.SetActive(false);
        }
        EventTriger.SetActive(true);
        DialogTriger.SetActive(true);
        DialogTrigerAlaramOff.SetActive(true);
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

        if (AlarmStatus && sharedMaterial2 != null)
        {
            float emissionIntensity = Mathf.Lerp(materialMinEmission, materialMaxEmission, t);
            sharedMaterial2.EnableKeyword("_EMISSION");
            sharedMaterial2.SetColor("_EmissionColor", baseEmissionColor * emissionIntensity);
        }

        UpdateAlarmSounds();
    }

    private void UpdateAlarmSounds()
    {
        if (alarmAudioSources == null || alarmAudioSources.Count == 0)
            return;

        alarmTimer -= Time.deltaTime;

        if (alarmTimer <= 0f)
        {
            foreach (var audioSource in alarmAudioSources)
            {
                if (audioSource != null)
                    audioSource.Play();
            }

            if (alarmAudioSources[0] != null && alarmAudioSources[0].clip != null)
                alarmTimer = alarmAudioSources[0].clip.length;
        }
    }

    private void StopAlarmSounds()
    {
        foreach (var audioSource in alarmAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
