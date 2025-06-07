using UnityEngine;

public class CursorFromLook : MonoBehaviour
{
    public RectTransform canvasRect;
    public RectTransform cursor;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        Plane canvasPlane = new Plane(canvasRect.forward, canvasRect.position);
        if (canvasPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector2 localPoint = canvasRect.InverseTransformPoint(hitPoint);

            cursor.localPosition = localPoint;

            // Визуальный луч
            Debug.DrawRay(transform.position, transform.forward * 10f, Color.green);
        }
    }
}
