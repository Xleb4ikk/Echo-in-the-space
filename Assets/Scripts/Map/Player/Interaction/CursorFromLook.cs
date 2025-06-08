using UnityEngine;

public class CursorFromLook : MonoBehaviour
{
    public RectTransform canvasRect;
    public RectTransform cursor;

    private void Start()
    {
        // Сразу отключаем точку взаимодействия
        if (cursor != null)
        {
            cursor.gameObject.SetActive(false);
        }
        
        // Отключаем сам скрипт
        enabled = false;
    }

    // Оригинальный код закомментирован, а не удален, чтобы сохранить возможность восстановить его при необходимости
    /*
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        Plane canvasPlane = new Plane(canvasRect.forward, canvasRect.position);
        if (canvasPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector2 localPoint = canvasRect.InverseTransformPoint(hitPoint);

            cursor.localPosition = localPoint;

            Debug.DrawRay(transform.position, transform.forward * 10f, Color.green);
        }
    }
    */
}
