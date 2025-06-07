using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShowBoxColliderInGame : MonoBehaviour
{
    private BoxCollider box;

    void Start()
    {
        box = GetComponent<BoxCollider>();
    }

    void Update()
    {
        DrawBoxCollider();
    }

    void DrawBoxCollider()
    {
        Vector3 center = box.center;
        Vector3 size = box.size;

        Vector3[] corners = new Vector3[8];

        // Calculate world-space corners
        Vector3 extents = size * 0.5f;
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? -extents.x : extents.x,
                (i & 2) == 0 ? -extents.y : extents.y,
                (i & 4) == 0 ? -extents.z : extents.z
            );
            corners[i] = transform.TransformPoint(center + corner);
        }

        // Draw edges
        DrawLine(corners[0], corners[1]);
        DrawLine(corners[1], corners[3]);
        DrawLine(corners[3], corners[2]);
        DrawLine(corners[2], corners[0]);

        DrawLine(corners[4], corners[5]);
        DrawLine(corners[5], corners[7]);
        DrawLine(corners[7], corners[6]);
        DrawLine(corners[6], corners[4]);

        DrawLine(corners[0], corners[4]);
        DrawLine(corners[1], corners[5]);
        DrawLine(corners[2], corners[6]);
        DrawLine(corners[3], corners[7]);
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.green);
    }
}
