using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportOnKey : MonoBehaviour
{
    Vector3 RunPos = new Vector3(14.80115f, -2.538255f, -50.82444f);

    public void Update()
    {
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            Teleport(RunPos, 90f);
        }
    } 

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