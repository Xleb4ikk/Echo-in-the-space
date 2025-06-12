using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportOnKey : MonoBehaviour
{
    public void Teleport(Vector3 newPosition, float yRotationDegrees)
    {
        Debug.Log("Телепортирование...");

        Debug.Log("Позиция до телепорта: " + transform.position);
        Debug.Log("Поворот до телепорта: " + transform.eulerAngles);

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log("CharacterController отключён");
        }

        transform.position = newPosition;

        Vector3 newEulerAngles = transform.eulerAngles;
        newEulerAngles.y = yRotationDegrees;
        transform.eulerAngles = newEulerAngles;

        Debug.Log("Позиция после телепорта: " + transform.position);
        Debug.Log("Поворот после телепорта: " + transform.eulerAngles);

        if (controller != null)
        {
            controller.enabled = true;
            Debug.Log("CharacterController включён");
        }
    }
}