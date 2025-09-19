using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionObject : MonoBehaviour  // Исправлено название класса (опционально)
{
    public Camera mainCam;
    public float interactionDistance = 10f;
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;

    void Update()
    {
        InteractionRay();
    }

    void InteractionRay()
    {
        Ray ray = mainCam.ViewportPointToRay(Vector3.one / 2f);
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red); // Отладочный луч
        RaycastHit hit;

        bool hitSomething = false;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Door")) // Проверка тега для фильтрации
            {
                // НОВОЕ: Проверяем, что коллайдер — именно BoxCollider, игнорируем MeshCollider и другие
                if (hit.collider is BoxCollider)
                {
                    Door interactable = hit.collider.GetComponentInParent<Door>(); // Ищем Door на объекте или parent'е
                    if (interactable != null)
                    {
                        hitSomething = true;
                        interactionText.text = interactable.GetDescription(); // Прямой вызов метода

                        if (Keyboard.current.eKey.wasPressedThisFrame)
                        {
                            interactable.Interact(); // Прямой вызов метода
                            Debug.Log("Interact called on " + interactable.name);
                        }
                    }
                    else
                    {
                        Debug.Log("No Door component found on " + hit.collider.name + " or its parents");
                    }
                }
                else
                {
                    // НОВОЕ: Лог для отладки, если попался не BoxCollider
                    Debug.Log("Ignored collider on " + hit.collider.name + " (not a BoxCollider)");
                }
            }
        }

        interactionUI.SetActive(hitSomething);
    }
}