using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionDoor : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Animator doorAnimation;

    [Header("Настройки Raycast")]
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;
    private Transform cameraTransform;

    [Header("Состояние двери")]
    private bool isOpen = false;
    private bool isAnimating = false;

    [Header("Звуки")]
    public AudioClip clip; // прикрепите файл в инспекторе
    public AudioSource audioSource;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    [Header("Настройки взаимодействия")]
    [SerializeField] private string interactableTag = "Door";


    private void Start()
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Не найдена основная камера (Main Camera)!");
        }
    }

    private void Update()
    {
        if (cameraTransform == null) return;

        HandleDoorInteraction();
        
    }

    private void HandleDoorInteraction()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            // Проверяем тег, заданный в инспекторе
            if (hit.collider.CompareTag(interactableTag) && hit.collider.gameObject == this.gameObject)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame && !isAnimating)
                {
                    if (!isOpen)
                    {
                        Debug.Log("🟢 Открываем дверь");
                        OpenDoor();
                    }
                    else
                    {
                        Debug.Log("🔴 Закрываем дверь");
                        CloseDoor();
                    }
                }
            }
        }
    }


    private void OpenDoor()
    {
        isAnimating = true;
        doorAnimation.SetBool("character_nearby", true);
        PlaySound();
        Debug.Log("🚪 Анимация открытия двери...");
        isOpen = true;
        isAnimating = false;
    }

    private void CloseDoor()
    {
        isAnimating = true;
        doorAnimation.SetBool("character_nearby", false);
        PlaySound();
        Debug.Log("🚪 Анимация закрытия двери...");
        isOpen = false;
        isAnimating = false;
    }

    public void PlaySound()
    {
        audioSource.Play();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || Camera.main == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Camera.main.transform.position,
                        Camera.main.transform.position + Camera.main.transform.forward * rayDistance);
    }
}
