using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // ������ �� ������ ������
    public float initialDistance = 10f; // ��������� ���������� ������ �� ������
    public float finalDistance = 20f; // �������� ���������� ������ �� ������
    public float transitionSpeed = 2f; // �������� ��������� ������

    private Vector3 targetPosition; // ������� ������� ������
    private bool isMoving = false; // ���� ��� ��������, �������� �� ��������

    void Update()
    {
        // ���������, ���� ������ ������ ���������
        if (isMoving)
        {
            // ���������� �������� ������
            float step = transitionSpeed * Time.deltaTime;
            Vector3 direction = (transform.position - player.position).normalized;
            targetPosition = player.position + direction * finalDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            // ������������� ��������, ���� �������� �������� �������
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
    }

    // ����� ��� ����������� ����������� ������ � ������� �������� ���������
    public void TriggerCameraMovement()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        transform.position = player.position + direction * initialDistance;
        isMoving = true;
    }
}
