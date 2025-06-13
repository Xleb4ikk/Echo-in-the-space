using System.Collections.Generic;
using UnityEngine;

public class VisibleInFOVOnly : MonoBehaviour
{
    [Header("Объекты, которые нужно отслеживать")]
    public List<GameObject> targetObjects;

    [Header("Параметры поля зрения")]
    public Transform playerHead;
    public float fieldOfView = 60f;
    public float viewDistance = 15f;
    public float disappearDelay = 2f;

    [Header("Слои, через которые можно видеть (например, Игрок и Монстр)")]
    public LayerMask visionMask;

    private bool playerInsideTrigger = false;

    private Dictionary<GameObject, float> lastSeenTimes = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> isPermanentlyHidden = new Dictionary<GameObject, bool>();

    void Start()
    {
        foreach (var obj in targetObjects)
        {
            if (obj != null)
            {
                lastSeenTimes[obj] = Time.time;
                isPermanentlyHidden[obj] = false;
            }
        }
    }

    void Update()
    {
        if (!playerInsideTrigger) return;

        foreach (var obj in targetObjects)
        {
            if (obj == null || !obj.activeSelf || isPermanentlyHidden[obj]) continue;

            Vector3 direction = obj.transform.position - playerHead.position;
            float distance = direction.magnitude;
            direction.Normalize();

            float angle = Vector3.Angle(playerHead.forward, direction);
            bool inFOV = angle < fieldOfView / 2f && distance < viewDistance;

            // Проверка на прямую видимость
            bool hasLineOfSight = false;
            RaycastHit hit;
            Vector3 rayOrigin = playerHead.position + playerHead.forward * 0.1f;
            if (Physics.Raycast(rayOrigin, direction, out hit, viewDistance, visionMask))
            {
                if (hit.transform == obj.transform)
                {
                    hasLineOfSight = true;
                }
            }

            if (inFOV && hasLineOfSight)
            {
                lastSeenTimes[obj] = Time.time;
            }

            if (Time.time - lastSeenTimes[obj] > disappearDelay)
            {
                obj.SetActive(false);
                isPermanentlyHidden[obj] = true; // помечаем, что больше не активируем
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = true;

            foreach (var obj in targetObjects)
            {
                if (obj != null && !isPermanentlyHidden[obj])
                {
                    obj.SetActive(true);
                    lastSeenTimes[obj] = Time.time;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = false;
        }
    }
}
