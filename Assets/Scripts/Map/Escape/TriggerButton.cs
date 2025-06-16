using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class TriggerButton : MonoBehaviour
{
    public string animationBoolName = "IsPressed";
    public string animationDoorBoolName = "IsOpen";
    public AudioClip soundEffect;
    public AudioClip secondSoundEffect; // Второй звуковой эффект
    public Animator ButtonAnimator;
    public Animator DoorAnimator; 
    public AudioSource audioSource;
    public AudioSource secondAudioSource; // Второй источник звука
    public Renderer ButtonRenderer;
    public Material ButtonMaterialEnabled;
    public Material ButtonMaterialDisabled;
    public bool IsPlayerInside { get; private set; }
    public Transform playerTransform;
    public Collider triggerZone;
    public Light[] launchLights; // Массив источников света

    public event Action OnButtonPressed;

    public GameObject GameObject;


    private void Start()
    {
        audioSource.clip = soundEffect;
        if (secondAudioSource != null)
        {
            secondAudioSource.clip = secondSoundEffect;
        }
        // Выключаем все источники света при старте
        if (launchLights != null)
        {
            foreach (var light in launchLights)
            {
                if (light != null)
                    light.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (triggerZone != null && playerTransform != null)
        {
            IsPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (IsPlayerInside)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartCoroutine(ButtonPresed());
                }
            }
        }
    }

    IEnumerator ButtonPresed()
    {
        ButtonRenderer.material = ButtonMaterialEnabled;

        if (ButtonAnimator != null)
            ButtonAnimator.SetBool(animationBoolName, true);

        // Воспроизводим первый звук
        if (soundEffect != null && audioSource != null)
            audioSource.Play();

        // Включаем источники света
        if (launchLights != null)
        {
            foreach (var light in launchLights)
            {
                if (light != null)
                    light.enabled = true;
            }
        }

        // Вызываем событие нажатия кнопки
        OnButtonPressed?.Invoke();

        yield return new WaitForSeconds(0.10f);

        if (ButtonAnimator != null)
            ButtonAnimator.SetBool(animationBoolName, false);

        // Воспроизводим второй звук
        if (secondSoundEffect != null && secondAudioSource != null)
            secondAudioSource.Play();

        yield return new WaitForSeconds(0.30f);

        ButtonRenderer.material = ButtonMaterialDisabled;

        yield return new WaitForSeconds(1f);

        if (DoorAnimator != null)
            DoorAnimator.SetBool(animationDoorBoolName, false);
    }
}
