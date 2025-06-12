using System.Collections.Generic;
using UnityEngine;

public class VisibleInFOVOnly : MonoBehaviour
{
    [Header("Объекты, которые будут исчезать")]
    public List<GameObject> targetObjects;

    [Header("Параметры поля зрения")]
    public Transform playerHead;           // Камера или "глаза" игрока
    public float fieldOfView = 60f;        // Угол зрения
    public float viewDistance = 15f;       // Дистанция, на которой объект виден
    public float disappearDelay = 2f;      // Задержка перед исчезновением

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

            // Обновляем время, чтобы объекты не исчезли мгновенно при входе
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

            // Деактивируем все объекты сразу, когда игрок выходит из триггера
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
