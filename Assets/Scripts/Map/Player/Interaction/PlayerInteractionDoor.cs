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
        // Получаем трансформ камеры
        Transform cam = Camera.main?.transform;

        if (cam == null)
        {
            Debug.LogError("❌ Главная камера не найдена! Убедись, что у камеры стоит тег 'MainCamera'");
            return;
        }

        // Создаём луч из камеры вперёд
        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;

        // Отладка: рисуем луч
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
        Debug.Log($"📡 Луч из камеры: позиция {ray.origin}, направление {ray.direction}, длина {rayDistance}");

        // Проверка попадания
        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            Debug.Log($"✅ Попадание в объект: {hit.collider.name} (Тег: {hit.collider.tag})");

            if (hit.collider.CompareTag("Door"))
            {
                float distanceToDoor = Vector3.Distance(cam.position, hit.point);
                Debug.Log($"🚪 Обнаружена дверь. Расстояние: {distanceToDoor:F2} м");

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Debug.Log("⌨️ Нажата клавиша E");

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
            else
            {
                Debug.Log("⚠️ Объект не имеет тег 'Door'");
            }
        }
        else
        {
            Debug.Log("❌ Ничего не зацеплено лучом.");
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
