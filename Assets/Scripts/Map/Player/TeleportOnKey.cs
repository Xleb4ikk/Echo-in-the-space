using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportOnKey : MonoBehaviour
{
    public void Teleport(Vector3 newPosition, float yRotationDegrees)
    {
        Debug.Log("����������������...");

        Debug.Log("������� �� ���������: " + transform.position);
        Debug.Log("������� �� ���������: " + transform.eulerAngles);

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log("CharacterController ��������");
        }

        transform.position = newPosition;

        Vector3 newEulerAngles = transform.eulerAngles;
        newEulerAngles.y = yRotationDegrees;
        transform.eulerAngles = newEulerAngles;

        Debug.Log("������� ����� ���������: " + transform.position);
        Debug.Log("������� ����� ���������: " + transform.eulerAngles);

        if (controller != null)
        {
            controller.enabled = true;
            Debug.Log("CharacterController �������");
        }
    }
}