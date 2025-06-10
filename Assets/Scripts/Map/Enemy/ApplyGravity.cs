using UnityEngine;

public class ApplyGravity : MonoBehaviour
{
    public float gravity = -9.81f; // ���� ����������
    public float groundLevel = 0.0f; // ������� �����
    private float verticalVelocity = 0f;

    void Update()
    {
        // ���� �� �� ����� � ��������� ����������
        if (transform.position.y > groundLevel)
        {
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += new Vector3(0, verticalVelocity * Time.deltaTime, 0);
        }
        else
        {
            // ���� �� ����� � �������� ������������ ��������
            transform.position = new Vector3(transform.position.x, groundLevel, transform.position.z);
            verticalVelocity = 0;
        }
    }
}
