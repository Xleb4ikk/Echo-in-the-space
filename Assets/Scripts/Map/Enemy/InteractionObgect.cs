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
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.3f, interactionDistance);
        bool hitSomething = false;
        float closestDistance = Mathf.Infinity;
        Door bestDoor = null;

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Door") && h.collider is BoxCollider)
            {
                Door interactable = h.collider.GetComponentInParent<Door>();
                if (interactable != null)
                {
                    float dist = h.distance;
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        bestDoor = interactable;
                    }
                }
            }
        }

        if (bestDoor != null)
        {
            hitSomething = true;
            interactionText.text = bestDoor.GetDescription();

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                bestDoor.Interact();
                Debug.Log("Interact called on " + bestDoor.name);
            }
        }

        interactionUI.SetActive(hitSomething);
    }
}