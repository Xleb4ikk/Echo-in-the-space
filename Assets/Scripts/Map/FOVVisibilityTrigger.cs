using System.Collections.Generic;
using UnityEngine;

public class VisibleInFOVOnly : MonoBehaviour
{
    [Header("�������, ������� ����� ��������")]
    public List<GameObject> targetObjects;

    [Header("��������� ���� ������")]
    public Transform playerHead;           // ������ ��� "�����" ������
    public float fieldOfView = 60f;        // ���� ������
    public float viewDistance = 15f;       // ���������, �� ������� ������ �����
    public float disappearDelay = 2f;      // �������� ����� �������������

    private bool playerInsideTrigger = false;
    private Dictionary<GameObject, float> lastSeenTimes = new Dictionary<GameObject, float>();

    void Start()
    {
        foreach (var obj in targetObjects)
        {
            if (obj != null)
            {
                lastSeenTimes[obj] = Time.time;
            }
        }
    }

    void Update()
    {
        if (!playerInsideTrigger) return;

        foreach (var obj in targetObjects)
        {
            if (obj == null || !obj.activeSelf) continue;

            Vector3 direction = obj.transform.position - playerHead.position;
            float distance = direction.magnitude;
            direction.Normalize();

            float angle = Vector3.Angle(playerHead.forward, direction);

            if (angle < fieldOfView / 2f && distance < viewDistance)
            {
                lastSeenTimes[obj] = Time.time;
            }

            if (Time.time - lastSeenTimes[obj] > disappearDelay)
            {
                obj.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = true;

            // ��������� �����, ����� ������� �� ������� ��������� ��� �����
            foreach (var obj in targetObjects)
            {
                if (obj != null)
                    lastSeenTimes[obj] = Time.time;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = false;

            // ������������ ��� ������� �����, ����� ����� ������� �� ��������
            foreach (var obj in targetObjects)
            {
                if (obj != null && obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
