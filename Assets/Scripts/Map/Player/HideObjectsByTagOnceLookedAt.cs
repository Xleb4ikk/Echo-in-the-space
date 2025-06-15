using UnityEngine;
using System.Collections;

public class HideOnLookedAt : MonoBehaviour
{
    public Camera playerCamera;
    public float viewAngle = 10f;        // Угол обзора (градусы)
    public float maxDistance = 100f;     // Максимальная дистанция
    public float delay = 0.2f;           // Задержка перед исчезновением

    private bool isHiding = false;
    private bool isHidden = false;

    void Update()
    {
        if (isHidden || isHiding || playerCamera == null)
            return;

        Vector3 toObject = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, toObject);

        if (angle < viewAngle && toObject.sqrMagnitude <= maxDistance * maxDistance)
        {
            StartCoroutine(HideAfterDelay());
            isHiding = true;
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        // Проверка: всё ещё в угле обзора?
        Vector3 toObject = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, toObject);

        if (angle < viewAngle && toObject.sqrMagnitude <= maxDistance * maxDistance)
        {
            gameObject.SetActive(false);
            isHidden = true;
        }

        isHiding = false;
    }
}
