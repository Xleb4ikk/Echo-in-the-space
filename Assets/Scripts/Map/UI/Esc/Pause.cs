using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class Pause : MonoBehaviour
{
    [Header("UI Панели")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    [Header("Кнопки")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton;

    [Header("Аудио клипы")]
    [SerializeField] private AudioClip pauseMusicClip;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField, Range(0f, 1f)] private float buttonSoundVolume = 0.5f;

    [Header("Аудио источники (назначить вручную)")]
    [SerializeField] private AudioSource pauseMusicSource;
    [SerializeField] private AudioSource buttonSoundSource;

    [Header("Визуальные эффекты")]
    [SerializeField] private Volume depthOfFieldVolume;

    [Header("Компоненты управления")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private MonoBehaviour cameraControlScript;
    [SerializeField] private Player playerMovement;
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;

    private Settings settingsScript;
    private bool isSettingsOpen = false;
    private bool needToResetMovement = false;

    private void Awake()
    {
        if (settingsMenuPanel != null)
        {
            settingsScript = settingsMenuPanel.GetComponent<Settings>();
        }

        if (pauseMusicSource != null)
        {
            pauseMusicSource.clip = pauseMusicClip;
            pauseMusicSource.loop = true;
            pauseMusicSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        EnsureInputSystemUIModule();

        settingsPanel?.SetActive(false);
        settingsMenuPanel?.SetActive(false);

        SetupButtonSounds();

        continueButton.onClick.AddListener(CloseSettings);
        exitButton.onClick.AddListener(ReturnToMainMenu);
        settingsButton?.onClick.AddListener(OpenSettingsMenu);
        backButton?.onClick.AddListener(ReturnToPauseMenu);

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<Player>();

        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<PlayerCamera>();
    }

    private void EnsureInputSystemUIModule()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            if (!eventSystem.GetComponent<InputSystemUIInputModule>())
            {
                DestroyImmediate(eventSystem.GetComponent<StandaloneInputModule>());
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }
        else
        {
            Debug.LogError("EventSystem не найден на сцене!");
        }
    }

    private void SetupButtonSounds()
    {
        Button[] allButtons = settingsPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in allButtons)
        {
            button.onClick.AddListener(() => PlayButtonClickSound());

            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>() ??
                                        button.gameObject.AddComponent<EventTrigger>();

            bool hasPointerEnter = false;
            foreach (var entry in eventTrigger.triggers)
            {
                if (entry.eventID == EventTriggerType.PointerEnter)
                {
                    hasPointerEnter = true;
                    break;
                }
            }

            if (!hasPointerEnter)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                entry.callback.AddListener((_) => PlayButtonHoverSound());
                eventTrigger.triggers.Add(entry);
            }
        }
    }

    private void PlayButtonHoverSound()
    {
        if (buttonHoverSound != null && buttonSoundSource != null)
            buttonSoundSource.PlayOneShot(buttonHoverSound, buttonSoundVolume);
    }

    private void PlayButtonClickSound()
    {
        if (buttonClickSound != null && buttonSoundSource != null)
            buttonSoundSource.PlayOneShot(buttonClickSound, buttonSoundVolume);
    }

    private void Update()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            Debug.Log("Кнопка esc нажата");
            if (settingsMenuPanel.activeSelf)
            {
                settingsMenuPanel.SetActive(false);
                settingsPanel.SetActive(true);
            }
            else if (isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }

        if (needToResetMovement)
        {
            needToResetMovement = false;
            Debug.Log("Движение сброшено - нажмите клавиши заново");
        }
    }

    public void OpenSettings()
    {
        settingsPanel?.SetActive(true);
        isSettingsOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
        DisableAllControls();

        foreach (AudioSource audio in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (audio != pauseMusicSource && audio.isPlaying)
                audio.Pause();
        }

        if (pauseMusicSource != null && pauseMusicClip != null)
            pauseMusicSource.Play();

        if (depthOfFieldVolume != null)
            depthOfFieldVolume.weight = 1f;

        if (playerMovement != null)
            playerMovement.canMove = false;

        if (playerCamera != null)
            playerCamera.canMove = false;
    }

    public void CloseSettings()
    {
        if (settingsMenuPanel != null && settingsMenuPanel.activeSelf)
        {
            if (settingsScript != null)
            {
                // settingsScript.SaveSettings(); // если нужно
            }
            settingsMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (pauseMusicSource != null)
            pauseMusicSource.Stop();

        foreach (AudioSource audio in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (audio != pauseMusicSource && audio != buttonSoundSource && !audio.isPlaying)
                audio.UnPause();
        }

        EnableAllControls();

        if (playerMovement != null)
        {
            playerMovement.ResetInput();
            playerMovement.canMove = true;
        }

        if (playerCamera != null)
        {
            playerCamera.ResetInput();
            playerCamera.canMove = true;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        settingsPanel?.SetActive(false);
        isSettingsOpen = false;
    }

    private void DisableAllControls()
    {
        if (playerControlScripts != null)
        {
            foreach (var script in playerControlScripts)
                if (script != null) script.enabled = false;
        }

        if (cameraControlScript != null)
            cameraControlScript.enabled = false;
    }

    private void EnableAllControls()
    {
        if (playerControlScripts != null)
        {
            foreach (var script in playerControlScripts)
                if (script != null) script.enabled = true;
        }

        if (cameraControlScript != null)
            cameraControlScript.enabled = true;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettingsMenu()
    {
        PlayButtonClickSound();
        settingsPanel?.SetActive(false);
        settingsMenuPanel?.SetActive(true);
    }

    public void ShowPausePanel()
    {
        settingsPanel?.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        PlayButtonClickSound();
        settingsMenuPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
    }
}
