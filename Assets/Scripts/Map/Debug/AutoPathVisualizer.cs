using UnityEngine;

public class AutoPathVisualizer : MonoBehaviour
{
    public Color lineColor = Color.green;
    public float pointRadius = 0.2f;
    public bool loop = true;

    private void OnDrawGizmos()
    {
        // Получаем все дочерние точки
        Transform[] points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            points[i] = transform.GetChild(i);
        }

        // Ничего не рисуем, если точек меньше двух
        if (points.Length < 2) return;

        Gizmos.color = lineColor;

        for (int i = 0; i < points.Length; i++)
        {
            Transform current = points[i];
            Transform next = (i < points.Length - 1) ? points[i + 1] : (loop ? points[0] : null);

            Gizmos.DrawSphere(current.position, pointRadius);

            if (next != null)
            {
                Gizmos.DrawLine(current.position, next.position);
            }
        }
    }
}
