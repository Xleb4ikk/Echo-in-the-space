using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFlashlight : MonoBehaviour
{
    public Light flashlight; // Перетащите сюда Spot Light в инспекторе
    public KeyCode toggleKey = KeyCode.F;
    [SerializeField] private AudioClip flashlightSound; // Звук фонарика
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Получаем или добавляем компонент AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            flashlight.enabled = !flashlight.enabled;
            // Воспроизводим звук при переключении
            if (flashlightSound != null)
            {
                audioSource.PlayOneShot(flashlightSound);
            }
        }
    }
}
