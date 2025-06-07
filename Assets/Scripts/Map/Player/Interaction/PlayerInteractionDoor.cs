using Mono.Cecil.Cil;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // важно!

public class PlayerInteractionDoor : MonoBehaviour
{

    public Animator doorAnimation;

    [Header("Настройки Raycast")]
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Состояние двери")]
    private bool openDoor = false;

    private void Update()
    {
        DoorInteraction();
    }

    private void DoorInteraction()
    {
        Transform cam = Camera.main?.transform;

        if (cam == null)
        {
            return;
        }

        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Door"))
            {
                float distanceToDoor = Vector3.Distance(cam.position, hit.point);

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {

                    if (!openDoor)
                    {
                        Debug.Log("🟢 Открываем дверь");
                        StartCoroutine(OpenDoor());
                    }
                    else
                    {
                        Debug.Log("🔴 Закрываем дверь");
                        StartCoroutine(CloseDoor());
                    }
                }
            }
        }
    }

    private IEnumerator OpenDoor()
    {
        openDoor = true;
        doorAnimation.SetBool("character_nearby", true);
        Debug.Log("🚪 Дверь открывается...");
        yield return new WaitForSeconds(1f); // Заглушка времени открытия
    }

    private IEnumerator CloseDoor()
    {
        doorAnimation.SetBool("character_nearby", false);
        openDoor = false;
        yield return new WaitForSeconds(1f); // Заглушка времени закрытия
    }

    // Визуализация луча в редакторе
    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Camera.main.transform.position,
                        Camera.main.transform.position + Camera.main.transform.forward * rayDistance);
    }
}
