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

    [Header("Состояние двери")]
    private bool isOpen = false;
    private bool isAnimating = false;

    private Transform cameraTransform;

    private void Start()
    {
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

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Door"))
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
        Debug.Log("🚪 Анимация открытия двери...");
        isOpen = true;
        isAnimating = false;
    }

    private void CloseDoor()
    {
        isAnimating = true;
        doorAnimation.SetBool("character_nearby", false);
        Debug.Log("🚪 Анимация закрытия двери...");
        isOpen = false;
        isAnimating = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || Camera.main == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Camera.main.transform.position,
                        Camera.main.transform.position + Camera.main.transform.forward * rayDistance);
    }
}
